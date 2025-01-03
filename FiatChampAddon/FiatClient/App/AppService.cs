using CoordinateSharp;
using FiatChamp.Fiat;
using FiatChamp.Ha;
using Flurl.Http;
using System.Collections.Concurrent;
using System.Text.Json;
using FiatChamp.Extensions;
using FiatChamp.Fiat.Entities;
using Microsoft.Extensions.Options;
using FiatChamp.Fiat.Model;
using FiatChamp.Ha.Model;
using Microsoft.Extensions.Logging;

namespace FiatChamp.App
{
    public class AppService : IAppService
    {
        private readonly AutoResetEvent _forceLoopResetEvent = new(false);
        private readonly ConcurrentDictionary<string, CarContext> _cars = new();

        private readonly ILogger<AppService> _logger;
        private readonly AppSettings _appSettings;
        private readonly FiatSettings _fiatSettings;
        private readonly IFiatClient _fiatClient;
        private readonly IHaApiClient _haApiClient;
        private readonly IHaMqttClient _haMqttClient;

        public AppService(ILogger<AppService> logger, IOptions<AppSettings> appConfig, IOptions<FiatSettings> fiatConfig, IFiatClient fiatClient, IHaApiClient haApiClient, IHaMqttClient haMqttClient)
        {
            _logger = logger;
            _appSettings = appConfig.Value;
            _fiatSettings = fiatConfig.Value;
            _fiatClient = fiatClient;
            _haApiClient = haApiClient;
            _haMqttClient = haMqttClient;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delay start for seconds: {delay}", _appSettings.StartDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_appSettings.StartDelaySeconds), cancellationToken);

            _logger.LogInformation("Connecting to HomeAssistant");
            await _haMqttClient.ConnectAsync(cancellationToken);

            _logger.LogInformation("Connecting to Fiat");
            await _fiatClient.ConnectAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await FetchData(cancellationToken);
                    _logger.LogInformation("Processing COMPLETED.");
                }
                catch (FlurlHttpException exception)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        var responseTask = exception.Call?.Response?.GetStringAsync();
                        var response = responseTask != null ? await responseTask : string.Empty;
                        _logger.LogDebug(exception, "Processing FAILED. STATUS: {status}, MESSAGE: {message}, RESPONSE: {response}", exception.StatusCode, exception.Message, response);
                    }
                    else
                        _logger.LogWarning("Processing FAILED. Error connecting to the FIAT API. This can happen from time to time.");
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Processing FAILED");
                }
                finally
                {
                    _logger.LogInformation("Next update in {delay} minutes.", _appSettings.RefreshInterval);
                }
                
                WaitHandle.WaitAny([cancellationToken.WaitHandle, _forceLoopResetEvent], TimeSpan.FromMinutes(_appSettings.RefreshInterval));
            }
        }

        private async Task FetchData(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching new data...");

            var config = await _haApiClient.GetConfigAsync();
            _logger.LogInformation("Using unit system: {unit}", config.UnitSystem.Dump());

            var shouldConvertKmToMiles = _appSettings.ConvertKmToMiles || config.UnitSystem.Length != "km";
            _logger.LogInformation("Convert km -> miles ? {shouldConvertKmToMiles}", shouldConvertKmToMiles);

            var states = await _haApiClient.GetStatesAsync();
            var zones = states
                .Where(state => state.EntityId.StartsWith("zone."))
                .Select(state => state.Attributes.Deserialize<HaRestApiZone>()!)
                .ToList();

            foreach (var vehicleInfo in await _fiatClient.GetVehiclesAsync())
            {
                _logger.LogInformation("Processing CAR: {vin}", vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshBattery) 
                    await TrySendCommand(FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshLocation) 
                    await TrySendCommand(FiatCommands.VF, vehicleInfo.Vehicle.Vin);

                if (!_cars.TryGetValue(vehicleInfo.Vehicle.Vin, out var context))
                {
                    context = new CarContext(_haMqttClient, vehicleInfo.Vehicle);
                    _cars.TryAdd(vehicleInfo.Vehicle.Vin, context);
                }

                // Location
                var currentZone = GetZone(zones, vehicleInfo.Location);
                await context.ProcessLocationAsync(vehicleInfo.Location, currentZone);

                // Details
                await context.ProcessDetailsAsync(vehicleInfo.Details, shouldConvertKmToMiles);
                
                // Buttons
                await BindButton(context, "Blink", FiatCommands.HBLF, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "Charge", FiatCommands.CNOW, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "UpdateAll", FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "UpdateLocation", FiatCommands.VF, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "UpdateBattery", FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);

                // Switches
                await BindSwitch(context, "Doors", FiatCommands.RDL, FiatCommands.RDU, vehicleInfo.Vehicle.Vin);
                await BindSwitch(context, "Hvac", FiatCommands.ROPRECOND, FiatCommands.ROPRECOND_OFF, vehicleInfo.Vehicle.Vin);
                await BindSwitch(context, "Trunk", FiatCommands.ROTRUNKUNLOCK, FiatCommands.ROTRUNKLOCK, vehicleInfo.Vehicle.Vin);

                // Timestamp
                await context.ProcessTimestampAsync();
            }
        }

        private string GetZone(IReadOnlyList<HaRestApiZone> zones, VehicleLocation location)
        {
            var coordinate = new Coordinate(location.Latitude, location.Longitude);
            var zone = zones
                .Select(zone => new { Zone = zone, DistanceToZone = new Coordinate(zone.Latitude, zone.Longitude).Get_Distance_From_Coordinate(coordinate).Meters })
                .Where(item => item.DistanceToZone <= item.Zone.Radius)
                .OrderBy(item => item.DistanceToZone)
                .Select(i => i.Zone)
                .FirstOrDefault();
            return zone?.FriendlyName ?? _appSettings.CarUnknownLocation;
        }

        private async Task BindButton(CarContext context, string name, FiatCommand command, string vin) => await context.ProcessButtonAsync(name, async (entity, state) =>
        {
            if (await TrySendCommand(command, vin))
                _forceLoopResetEvent.Set();
        });

        private async Task BindSwitch(CarContext context, string name, FiatCommand offCommand, FiatCommand onCommand, string vin) => await context.ProcessSwitchAsync(name, async (entity, state) =>
        {
            if (await TrySendCommand(entity.IsOn ? offCommand : onCommand, vin))
                _forceLoopResetEvent.Set();
        });

        private async Task<bool> TrySendCommand(FiatCommand command, string vin)
        {
            if (command.IsDangerous && !_appSettings.EnableDangerousCommands)
            {
                _logger.LogWarning("{command} not sent. Set \"EnableDangerousCommands\" option if you want to use it. ", command.Message);
                return false;
            }

            return await _fiatClient.TrySendCommandAsync(vin, command.Message, _fiatSettings.Pin, command.Action);
        }
    }
}
