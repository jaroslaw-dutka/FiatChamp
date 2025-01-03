using CoordinateSharp;
using FiatChamp.Fiat;
using FiatChamp.Ha;
using Flurl.Http;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FiatChamp.Fiat.Model;
using FiatChamp.Extensions;
using FiatChamp.Ha.Entities;
using Microsoft.Extensions.Logging;

namespace FiatChamp.App
{
    public class AppService : IAppService
    {
        private readonly AutoResetEvent _forceLoopResetEvent = new(false);
        private readonly ConcurrentDictionary<string, CarContext> _cars = new();

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

            _logger.LogInformation("Connecting to HomeAssistant");
            await _haClient.ConnectAsync(cancellationToken);

            _logger.LogInformation("Connecting to Fiat");
            await _fiatClient.ConnectAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await FetchData(cancellationToken);
                }
                catch (FlurlHttpException exception)
                {
                    _logger.LogWarning("Error connecting to the FIAT API.\nThis can happen from time to time. Retrying in {interval} minutes.", _appSettings.RefreshInterval);
                    
                    var responseTask = exception.Call?.Response?.GetStringAsync();
                    var response = responseTask != null ? await responseTask : string.Empty;

                    _logger.LogDebug(exception, "STATUS: {status}, MESSAGE: {message}, RESPONSE: {}", exception.StatusCode, exception.Message, response);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, exception.Message);
                }

                _logger.LogInformation("Fetching COMPLETED. Next update in {delay} minutes.", _appSettings.RefreshInterval);

                WaitHandle.WaitAny([cancellationToken.WaitHandle, _forceLoopResetEvent], TimeSpan.FromMinutes(_appSettings.RefreshInterval));
            }
        }

        private async Task FetchData(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Now fetching new data...");

            var config = await _haClient.ApiClient.GetConfigAsync();
            _logger.LogInformation("Using unit system: {unit}", config.UnitSystem.Dump());

            var shouldConvertKmToMiles = _appSettings.ConvertKmToMiles || config.UnitSystem.Length != "km";
            _logger.LogInformation("Convert km -> miles ? {shouldConvertKmToMiles}", shouldConvertKmToMiles);

            var states = await _haClient.ApiClient.GetStatesAsync();

            foreach (var vehicleInfo in await _fiatClient.GetVehiclesAsync())
            {
                _logger.LogInformation("FOUND CAR: {vin}", vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshBattery) 
                    await TrySendCommand(FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);

                if (_appSettings.AutoRefreshLocation) 
                    await TrySendCommand(FiatCommands.VF, vehicleInfo.Vehicle.Vin);

                // await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                if (!_cars.TryGetValue(vehicleInfo.Vehicle.Vin, out var context))
                    context = new CarContext(_haClient.MqttClient, vehicleInfo.Vehicle);

                // Location
                var location = new Coordinate(vehicleInfo.Location.Latitude, vehicleInfo.Location.Longitude);
                var zones = states.GetZones().OrderByDistance(location);
                _logger.LogDebug("Zones: {zones}", zones.Dump());
                await context.UpdateLocationAsync(location, zones.FirstOrDefault()?.FriendlyName ?? _appSettings.CarUnknownLocation);

                // Sensors
                await context.UpdateSensorsAsync(vehicleInfo.Details, shouldConvertKmToMiles);
                
                // Entities
                await BindButton(context, "UpdateLocation", FiatCommands.VF, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "RefreshBatteryStatus", FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "DeepRefresh", FiatCommands.DEEPREFRESH, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "Blink", FiatCommands.HBLF, vehicleInfo.Vehicle.Vin);
                await BindButton(context, "ChargeNOW", FiatCommands.CNOW, vehicleInfo.Vehicle.Vin);
                await BindSwitch(context, "Trunk", FiatCommands.ROTRUNKUNLOCK, FiatCommands.ROTRUNKLOCK, vehicleInfo.Vehicle.Vin);
                await BindSwitch(context, "HVAC", FiatCommands.ROPRECOND, FiatCommands.ROPRECOND_OFF, vehicleInfo.Vehicle.Vin);
                await BindSwitch(context, "DoorLock", FiatCommands.RDL, FiatCommands.RDU, vehicleInfo.Vehicle.Vin);

                // Timestamp
                await context.UpdateTimestampAsync();
            }
        }

        private async Task BindButton(CarContext context, string name, FiatCommand command, string vin) => await context.UpdateEntityAsync<HaButton>(name, async _ =>
        {
            if (await TrySendCommand(command, vin))
                _forceLoopResetEvent.Set();
        });

        private async Task BindSwitch(CarContext context, string name, FiatCommand offCommand, FiatCommand onCommand, string vin) => await context.UpdateEntityAsync<HaSwitch>(name, async sw =>
        {
            if (await TrySendCommand(sw.IsOn ? offCommand : onCommand, vin))
                _forceLoopResetEvent.Set();
        });

        private async Task<bool> TrySendCommand(FiatCommand command, string vin)
        {
            if (!command.IsDangerous || _appSettings.EnableDangerousCommands) 
                return await _fiatClient.TrySendCommandAsync(vin, command.Message, _fiatSettings.Pin, command.Action);

            _logger.LogWarning("{command} not sent. Set \"EnableDangerousCommands\" option if you want to use it. ", command.Message);
            return false;
        }
    }
}
