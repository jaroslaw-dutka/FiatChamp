using CoordinateSharp;
using FiatChamp.Fiat;
using FiatChamp.Ha.Model;
using FiatChamp.Ha;
using Flurl.Http;
using System.Globalization;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FiatChamp.Fiat.Model;
using FiatChamp.Extensions;
using Microsoft.Extensions.Logging;
using FiatChamp.Fiat.Entities;
using FiatChamp.Ha.Entities;

namespace FiatChamp.App
{
    public class AppService : IAppService
    {
        private readonly AutoResetEvent _forceLoopResetEvent = new(false);
        private readonly ConcurrentDictionary<string, IEnumerable<HaEntity>> _persistentHaEntities = new();

        private readonly AppSettings _appSettings;
        private readonly FiatSettings _fiatSettings;

        private readonly ILogger<AppService> _logger;
        private readonly IFiatClient _fiatClient;
        private readonly IHaClient _haClient;

        public AppService(ILogger<AppService> logger, IOptions<AppSettings> appConfig, IOptions<FiatSettings> fiatConfig, IFiatClient fiatClient, IHaClient haClient)
        {
            _appSettings = appConfig.Value;
            _fiatSettings = fiatConfig.Value;
            _logger = logger;
            _fiatClient = fiatClient;
            _haClient = haClient;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delay start for seconds: {delay}", _appSettings.StartDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_appSettings.StartDelaySeconds), cancellationToken);

            if (_fiatSettings.Brand is FcaBrand.Ram or FcaBrand.Dodge or FcaBrand.AlfaRomeo) 
                _logger.LogWarning("{brand} support is experimental.", _fiatSettings.Brand);

            // await _fiatClient.LoginAndKeepSessionAliveAsync();
            // await _fiatClient.ConnectToMqtt();
            // await _fiatClient.SendCommandAsync(vehicleInfo.Vehicle.Vin, "ROLIGHTS", _fiatSettings.Pin, "remote");
            // return;

            await _haClient.MqttClient.ConnectAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                await TryFetchData(cancellationToken);
                WaitHandle.WaitAny([cancellationToken.WaitHandle, _forceLoopResetEvent], TimeSpan.FromMinutes(_appSettings.RefreshInterval));
            }
        }

        private async Task TryFetchData(CancellationToken cancellationToken)
        {
            try
            {
                await FetchData(cancellationToken);
            }
            catch (FlurlHttpException httpException)
            {
                _logger.LogWarning("Error connecting to the FIAT API.\nThis can happen from time to time. Retrying in {interval} minutes.", _appSettings.RefreshInterval);
                _logger.LogDebug(httpException, "STATUS: {status}, MESSAGE: {message}", httpException.StatusCode, httpException.Message);

                var task = httpException.Call?.Response?.GetStringAsync();

                if (task != null) 
                    _logger.LogDebug("RESPONSE: {response}", await task);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            _logger.LogInformation("Fetching COMPLETED. Next update in {delay} minutes.", _appSettings.RefreshInterval);
        }

        private async Task FetchData(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Now fetching new data...");

            GC.Collect();

            await _fiatClient.LoginAndKeepSessionAliveAsync();

            var config = await _haClient.ApiClient.GetConfigAsync();
            _logger.LogInformation("Using unit system: {unit}", config.UnitSystem.Dump());

            var shouldConvertKmToMiles = _appSettings.ConvertKmToMiles || config.UnitSystem.Length != "km";
            _logger.LogInformation("Convert km -> miles ? {shouldConvertKmToMiles}", shouldConvertKmToMiles);

            var states = await _haClient.ApiClient.GetStatesAsync();

            foreach (var vehicleInfo in await _fiatClient.FetchAsync())
            {
                _logger.LogInformation("FOUND CAR: {vin}", vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshBattery) 
                    await TrySendCommand(_fiatClient, FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshLocation) 
                    await TrySendCommand(_fiatClient, FiatCommands.VF, vehicleInfo.Vehicle.Vin);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                var haDevice = new HaDevice
                {
                    Name = string.IsNullOrEmpty(vehicleInfo.Vehicle.Nickname) ? "Car" : vehicleInfo.Vehicle.Nickname,
                    Identifiers = [vehicleInfo.Vehicle.Vin],
                    Manufacturer = vehicleInfo.Vehicle.Make,
                    Model = vehicleInfo.Vehicle.ModelDescription,
                    Version = "1.0"
                };

                var currentCarLocation = new Coordinate(vehicleInfo.Location.Latitude, vehicleInfo.Location.Longitude);

                var zones = states.GetZones().OrderByDistance(currentCarLocation);
                _logger.LogDebug("Zones: {zones}", zones.Dump());

                var tracker = new HaDeviceTracker(_haClient.MqttClient, "CAR_LOCATION", haDevice)
                {
                    Lat = currentCarLocation.Latitude.ToDouble(),
                    Lon = currentCarLocation.Longitude.ToDouble(),
                    StateValue = zones.FirstOrDefault()?.FriendlyName ?? _appSettings.CarUnknownLocation
                };

                _logger.LogInformation("Car is at location: {location}", tracker.Dump());

                _logger.LogDebug("Announce sensor: {sensor}", tracker.Dump());
                await tracker.AnnounceAsync();
                await tracker.PublishStateAsync();

                var flattenDetails = vehicleInfo.Details.Flatten("car");
                var sensors = flattenDetails.Select(detail =>
                {
                    var sensor = new HaSensor(_haClient.MqttClient, detail.Key, haDevice)
                    {
                        Value = detail.Value
                    };

                    if (detail.Key.EndsWith("_value"))
                    {
                        var unitKey = detail.Key.Replace("_value", "_unit");

                        flattenDetails.TryGetValue(unitKey, out var tmpUnit);

                        if (tmpUnit == "km")
                        {
                            sensor.DeviceClass = "distance";

                            if (shouldConvertKmToMiles && int.TryParse(detail.Value, out var kmValue))
                            {
                                var miValue = Math.Round(kmValue * 0.62137, 2);
                                sensor.Value = miValue.ToString(CultureInfo.InvariantCulture);
                                tmpUnit = "mi";
                            }
                        }

                        switch (tmpUnit)
                        {
                            case "volts":
                                sensor.DeviceClass = "voltage";
                                sensor.Unit = "V";
                                break;
                            case null or "null":
                                sensor.Unit = "";
                                break;
                            default:
                                sensor.Unit = tmpUnit;
                                break;
                        }
                    }

                    return sensor;
                }).ToDictionary(k => k.Name, v => v);

                if (sensors.TryGetValue("car_evInfo_battery_stateOfCharge", out var stateOfChargeSensor))
                {
                    stateOfChargeSensor.DeviceClass = "battery";
                    stateOfChargeSensor.Unit = "%";
                }

                if (sensors.TryGetValue("car_evInfo_battery_timeToFullyChargeL2", out var timeToFullyChargeSensor))
                {
                    timeToFullyChargeSensor.DeviceClass = "duration";
                    timeToFullyChargeSensor.Unit = "min";
                }

                _logger.LogDebug("Announce sensors: {sensor}", sensors.Dump());
                _logger.LogInformation("Pushing new sensors and values to Home Assistant");

                await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.AnnounceAsync(); });

                _logger.LogDebug("Waiting for home assistant to process all sensors");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.PublishStateAsync(); });

                var lastUpdate = new HaSensor(_haClient.MqttClient, "LAST_UPDATE", haDevice)
                {
                    Value = DateTime.Now.ToString("O"),
                    DeviceClass = "timestamp"
                };

                await lastUpdate.AnnounceAsync();
                await lastUpdate.PublishStateAsync();

                var haEntities = _persistentHaEntities.GetOrAdd(vehicleInfo.Vehicle.Vin, s => CreateInteractiveEntities(vehicleInfo, haDevice));

                foreach (var haEntity in haEntities)
                {
                    _logger.LogDebug("Announce sensor: {sensor}", haEntity.Dump());
                    await haEntity.AnnounceAsync();
                }
            }
        }

        private async Task<bool> TrySendCommand(IFiatClient fiatClient, FiatCommand command, string vin)
        {
            _logger.LogInformation("SEND COMMAND {command}: ", command.Message);

            if (string.IsNullOrWhiteSpace(_fiatSettings.Pin))
            {
                throw new Exception("PIN NOT SET");
            }

            var pin = _fiatSettings.Pin;

            if (command.IsDangerous && !_appSettings.EnableDangerousCommands)
            {
                _logger.LogWarning("{command} not sent. Set \"EnableDangerousCommands\" option if you want to use it. ", command.Message);
                return false;
            }

            try
            {
                await fiatClient.SendCommandAsync(vin, command.Message, pin, command.Action);
                await Task.Delay(TimeSpan.FromSeconds(5));
                _logger.LogInformation("Command: {command} SUCCESSFUL", command.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Command: {command} ERROR. Maybe wrong pin?", command.Message);
                _logger.LogDebug(e, e.Message);
                return false;
            }

            return true;
        }

        private IEnumerable<HaEntity> CreateInteractiveEntities(VehicleInfo vehicle, HaDevice haDevice) =>
        [
            CreateButton("UpdateLocation", haDevice, FiatCommands.VF, vehicle.Vehicle.Vin),
            CreateButton("RefreshBatteryStatus", haDevice, FiatCommands.DEEPREFRESH, vehicle.Vehicle.Vin),
            CreateButton("DeepRefresh", haDevice, FiatCommands.DEEPREFRESH, vehicle.Vehicle.Vin),
            CreateButton("Blink", haDevice, FiatCommands.HBLF, vehicle.Vehicle.Vin),
            CreateButton("ChargeNOW", haDevice, FiatCommands.CNOW, vehicle.Vehicle.Vin),
            CreateSwitch("Trunk", haDevice, FiatCommands.ROTRUNKUNLOCK, FiatCommands.ROTRUNKLOCK, vehicle.Vehicle.Vin),
            CreateSwitch("HVAC", haDevice, FiatCommands.ROPRECOND, FiatCommands.ROPRECOND_OFF, vehicle.Vehicle.Vin),
            CreateSwitch("DoorLock", haDevice, FiatCommands.RDL, FiatCommands.RDU, vehicle.Vehicle.Vin)
        ];

        private HaButton CreateButton(string name, HaDevice device, FiatCommand command, string vin) => new(_haClient.MqttClient, name, device, async _ =>
        {
            if (await TrySendCommand(_fiatClient, command, vin))
                _forceLoopResetEvent.Set();
        });

        private HaSwitch CreateSwitch(string name, HaDevice device, FiatCommand offCommand, FiatCommand onCommand, string vin) => new(_haClient.MqttClient, name, device, async sw =>
        {
            if (await TrySendCommand(_fiatClient, sw.IsOn ? offCommand : onCommand, vin))
                _forceLoopResetEvent.Set();
        });
    }
}
