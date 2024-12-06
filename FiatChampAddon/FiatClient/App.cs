﻿using CoordinateSharp;
using FiatChamp.Fiat;
using FiatChamp.Ha.Model;
using FiatChamp.Ha;
using Flurl.Http;
using Serilog;
using System.Globalization;
using FiatChamp.Mqtt;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using FiatChamp.Fiat.Model;

namespace FiatChamp
{
    public class App : IApp
    {
        private readonly AutoResetEvent _forceLoopResetEvent = new(false);
        private readonly ConcurrentDictionary<string, IEnumerable<HaEntity>> _persistentHaEntities = new();

        private readonly AppConfig _appConfig;
        private readonly FiatConfig _fiatConfig;

        private readonly IFiatClient _fiatClient;
        private readonly IMqttClient _mqttClient;
        private readonly IHaRestApi _haClient;

        public App(IOptions<AppConfig> appConfig, IOptions<FiatConfig> fiatConfig, IFiatClient fiatClient, IMqttClient mqttClient, IHaRestApi haClient)
        {
            _appConfig = appConfig.Value;
            _fiatConfig = fiatConfig.Value;
            _mqttClient = mqttClient;
            _fiatClient = fiatClient;
            _haClient = haClient;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Log.Information("Delay start for seconds: {0}", _appConfig.StartDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_appConfig.StartDelaySeconds), cancellationToken);

            if (_fiatConfig.Brand is FcaBrand.Ram or FcaBrand.Dodge or FcaBrand.AlfaRomeo)
            {
                Log.Warning("{0} support is experimental.", _fiatConfig.Brand);
            }
            
            await _mqttClient.Connect();

            while (!cancellationToken.IsCancellationRequested)
            {
                Log.Information("Now fetching new data...");

                GC.Collect();

                try
                {
                    await _fiatClient.LoginAndKeepSessionAlive();

                    foreach (var vehicle in await _fiatClient.Fetch())
                    {
                        Log.Information("FOUND CAR: {0}", vehicle.Vin);

                        if (_appConfig.AutoRefreshBattery)
                        {
                            await TrySendCommand(_fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin);
                        }

                        if (_appConfig.AutoRefreshLocation)
                        {
                            await TrySendCommand(_fiatClient, FiatCommands.VF, vehicle.Vin);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                        var vehicleName = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname;
                        var suffix = _appConfig.DevMode ? "DEV" : "";

                        var haDevice = new HaDevice()
                        {
                            Name = vehicleName + suffix,
                            Identifiers = vehicle.Vin + suffix,
                            Manufacturer = vehicle.Make,
                            Model = vehicle.ModelDescription,
                            Version = "1.0"
                        };

                        var currentCarLocation = new Coordinate(vehicle.Location.Latitude, vehicle.Location.Longitude);

                        var zones = await _haClient.GetZonesAscending(currentCarLocation);

                        Log.Debug("Zones: {0}", zones.Dump());

                        var tracker = new HaDeviceTracker(_mqttClient, "CAR_LOCATION", haDevice)
                        {
                            Lat = currentCarLocation.Latitude.ToDouble(),
                            Lon = currentCarLocation.Longitude.ToDouble(),
                            StateValue = zones.FirstOrDefault()?.FriendlyName ?? _appConfig.CarUnknownLocation
                        };

                        Log.Information("Car is at location: {0}", tracker.Dump());

                        Log.Debug("Announce sensor: {0}", tracker.Dump());
                        await tracker.Announce();
                        await tracker.PublishState();

                        var compactDetails = vehicle.Details.Compact("car");
                        var unitSystem = await _haClient.GetUnitSystem();

                        Log.Information("Using unit system: {0}", unitSystem.Dump());

                        var shouldConvertKmToMiles = (_appConfig.ConvertKmToMiles || unitSystem.Length != "km");

                        Log.Information("Convert km -> miles ? {0}", shouldConvertKmToMiles);

                        var sensors = compactDetails.Select(detail =>
                        {
                            var sensor = new HaSensor(_mqttClient, detail.Key, haDevice)
                            {
                                Value = detail.Value
                            };

                            if (detail.Key.EndsWith("_value"))
                            {
                                var unitKey = detail.Key.Replace("_value", "_unit");

                                compactDetails.TryGetValue(unitKey, out var tmpUnit);

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

                        Log.Debug("Announce sensors: {0}", sensors.Dump());
                        Log.Information("Pushing new sensors and values to Home Assistant");

                        await Parallel.ForEachAsync(sensors.Values, async (sensor, token) => { await sensor.Announce(); });

                        Log.Debug("Waiting for home assistant to process all sensors");
                        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                        await Parallel.ForEachAsync(sensors.Values, async (sensor, token) => { await sensor.PublishState(); });

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
                            Log.Debug("Announce sensor: {0}", haEntity.Dump());
                            await haEntity.Announce();
                        }
                    }
                }
                catch (FlurlHttpException httpException)
                {
                    Log.Warning($"Error connecting to the FIAT API. \n" +
                                $"This can happen from time to time. Retrying in {_appConfig.RefreshInterval} minutes.");

                    Log.Debug("ERROR: {0}", httpException.Message);
                    Log.Debug("STATUS: {0}", httpException.StatusCode);

                    var task = httpException.Call?.Response?.GetStringAsync();

                    if (task != null)
                    {
                        Log.Debug("RESPONSE: {0}", await task);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("{0}", e);
                }

                Log.Information("Fetching COMPLETED. Next update in {0} minutes.", _appConfig.RefreshInterval);

                WaitHandle.WaitAny(new[]
                {
                    cancellationToken.WaitHandle,
                    _forceLoopResetEvent
                }, TimeSpan.FromMinutes(_appConfig.RefreshInterval));
            }
        }

        private async Task<bool> TrySendCommand(IFiatClient fiatClient, FiatCommand command, string vin)
        {
            Log.Information("SEND COMMAND {0}: ", command.Message);

            if (string.IsNullOrWhiteSpace(_fiatConfig.Pin))
            {
                throw new Exception("PIN NOT SET");
            }

            var pin = _fiatConfig.Pin;

            if (command.IsDangerous && !_appConfig.EnableDangerousCommands)
            {
                Log.Warning("{0} not sent. " +
                            "Set \"EnableDangerousCommands\" option if you want to use it. ", command.Message);
                return false;
            }

            try
            {
                await fiatClient.SendCommand(vin, command.Message, pin, command.Action);
                await Task.Delay(TimeSpan.FromSeconds(5));
                Log.Information("Command: {0} SUCCESSFUL", command.Message);
            }
            catch (Exception e)
            {
                Log.Error("Command: {0} ERROR. Maybe wrong pin?", command.Message);
                Log.Debug("{0}", e);
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

            return new HaEntity[]
            {
    hvacSwitch,
    trunkSwitch,
    chargeNowButton,
    deepRefreshButton,
    locateLightsButton,
    updateLocationButton,
    lockSwitch,
    batteryRefreshButton
            };
        }
    }
}
