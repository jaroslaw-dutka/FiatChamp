using CoordinateSharp;
using FiatChamp.Fiat;
using FiatChamp.Ha.Model;
using FiatChamp.Ha;
using Flurl.Http;
using System.Globalization;
using FiatChamp.Mqtt;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FiatChamp.Fiat.Model;
using FiatChamp.Extensions;
using Microsoft.Extensions.Logging;

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
        private readonly IMqttClient _mqttClient;
        private readonly IHaRestApi _haClient;

        public AppService(ILogger<AppService> logger, IOptions<AppSettings> appConfig, IOptions<FiatSettings> fiatConfig, IFiatClient fiatClient, IMqttClient mqttClient, IHaRestApi haClient)
        {
            _appSettings = appConfig.Value;
            _fiatSettings = fiatConfig.Value;
            _logger = logger;
            _mqttClient = mqttClient;
            _fiatClient = fiatClient;
            _haClient = haClient;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delay start for seconds: {delay}", _appSettings.StartDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_appSettings.StartDelaySeconds), cancellationToken);

            if (_fiatSettings.Brand is FcaBrand.Ram or FcaBrand.Dodge or FcaBrand.AlfaRomeo) 
                _logger.LogWarning("{brand} support is experimental.", _fiatSettings.Brand);

            await _mqttClient.Connect();

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

            await _fiatClient.LoginAndKeepSessionAlive();

            foreach (var vehicle in await _fiatClient.Fetch())
            {
                _logger.LogInformation("FOUND CAR: {vin}", vehicle.Vin);

                if (_appSettings.AutoRefreshBattery) 
                    await TrySendCommand(_fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin);

                if (_appSettings.AutoRefreshLocation) 
                    await TrySendCommand(_fiatClient, FiatCommands.VF, vehicle.Vin);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                var haDevice = new HaDevice
                {
                    Name = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname,
                    Identifiers = [vehicle.Vin],
                    Manufacturer = vehicle.Make,
                    Model = vehicle.ModelDescription,
                    Version = "1.0"
                };

                var currentCarLocation = new Coordinate(vehicle.Location.Latitude, vehicle.Location.Longitude);

                var zones = await _haClient.GetZonesAscending(currentCarLocation);

                _logger.LogDebug("Zones: {zones}", zones.Dump());

                var tracker = new HaDeviceTracker(_mqttClient, "CAR_LOCATION", haDevice)
                {
                    Lat = currentCarLocation.Latitude.ToDouble(),
                    Lon = currentCarLocation.Longitude.ToDouble(),
                    StateValue = zones.FirstOrDefault()?.FriendlyName ?? _appSettings.CarUnknownLocation
                };

                _logger.LogInformation("Car is at location: {location}", tracker.Dump());

                _logger.LogDebug("Announce sensor: {sensor}", tracker.Dump());
                await tracker.Announce();
                await tracker.PublishState();

                var unitSystem = await _haClient.GetUnitSystem();
                _logger.LogInformation("Using unit system: {unit}", unitSystem.Dump());

                var shouldConvertKmToMiles = _appSettings.ConvertKmToMiles || unitSystem.Length != "km";
                _logger.LogInformation("Convert km -> miles ? {shouldConvertKmToMiles}", shouldConvertKmToMiles);

                var flattenDetails = vehicle.Details.Flatten("car");
                var sensors = flattenDetails.Select(detail =>
                {
                    var sensor = new HaSensor(_mqttClient, detail.Key, haDevice)
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

                await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.Announce(); });

                _logger.LogDebug("Waiting for home assistant to process all sensors");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.PublishState(); });

                var lastUpdate = new HaSensor(_mqttClient, "LAST_UPDATE", haDevice)
                {
                    Value = DateTime.Now.ToString("O"),
                    DeviceClass = "timestamp"
                };

                await lastUpdate.Announce();
                await lastUpdate.PublishState();

                var haEntities = _persistentHaEntities.GetOrAdd(vehicle.Vin, s =>
                    CreateInteractiveEntities(_fiatClient, _mqttClient, vehicle, haDevice));

                foreach (var haEntity in haEntities)
                {
                    _logger.LogDebug("Announce sensor: {sensor}", haEntity.Dump());
                    await haEntity.Announce();
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
                await fiatClient.SendCommand(vin, command.Message, pin, command.Action);
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

        private IEnumerable<HaEntity> CreateInteractiveEntities(IFiatClient fiatClient, IMqttClient mqttClient, Vehicle vehicle, HaDevice haDevice)
        {
            var updateLocationButton = new HaButton(mqttClient, "UpdateLocation", haDevice, async button =>
            {
                if (await TrySendCommand(fiatClient, FiatCommands.VF, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var batteryRefreshButton = new HaButton(mqttClient, "RefreshBatteryStatus", haDevice, async button =>
            {
                if (await TrySendCommand(fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var deepRefreshButton = new HaButton(mqttClient, "DeepRefresh", haDevice, async button =>
            {
                if (await TrySendCommand(fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var locateLightsButton = new HaButton(mqttClient, "Blink", haDevice, async button =>
            {
                if (await TrySendCommand(fiatClient, FiatCommands.HBLF, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var chargeNowButton = new HaButton(mqttClient, "ChargeNOW", haDevice, async button =>
            {
                if (await TrySendCommand(fiatClient, FiatCommands.CNOW, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var trunkSwitch = new HaSwitch(mqttClient, "Trunk", haDevice, async sw =>
            {
                if (await TrySendCommand(fiatClient, sw.IsOn ? FiatCommands.ROTRUNKUNLOCK : FiatCommands.ROTRUNKLOCK, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var hvacSwitch = new HaSwitch(mqttClient, "HVAC", haDevice, async sw =>
            {
                if (await TrySendCommand(fiatClient, sw.IsOn ? FiatCommands.ROPRECOND : FiatCommands.ROPRECOND_OFF, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            var lockSwitch = new HaSwitch(mqttClient, "DoorLock", haDevice, async sw =>
            {
                if (await TrySendCommand(fiatClient, sw.IsOn ? FiatCommands.RDL : FiatCommands.RDU, vehicle.Vin))
                    _forceLoopResetEvent.Set();
            });

            return
            [
                hvacSwitch,
                trunkSwitch,
                chargeNowButton,
                deepRefreshButton,
                locateLightsButton,
                updateLocationButton,
                lockSwitch,
                batteryRefreshButton
            ];
        }
    }
}
