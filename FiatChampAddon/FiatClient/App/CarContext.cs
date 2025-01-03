using System.Globalization;
using System.Text.Json.Nodes;
using CoordinateSharp;
using FiatChamp.Extensions;
using FiatChamp.Fiat.Model;
using FiatChamp.Ha;
using FiatChamp.Ha.Entities;
using FiatChamp.Ha.Model;

namespace FiatChamp.App;

public class CarContext
{
    private readonly IHaMqttClient _mqtt;

    public string Vin { get; }
    public HaDevice Device { get; }
    public HaSensor<HaLocation>? Location { get; private set;}
    public HaSensor? Timestamp { get; private set; }
    public Dictionary<string, HaSensor> Sensors { get; } = new();
    public Dictionary<string, HaEntity> Entities { get;} = new();
    
    public CarContext(IHaMqttClient mqtt, Vehicle vehicle)
    {
        _mqtt = mqtt;

        Vin = vehicle.Vin;
        Device = new HaDevice
        {
            Name = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname,
            Identifiers = [vehicle.Vin],
            Manufacturer = vehicle.Make,
            Model = vehicle.ModelDescription,
            Version = "1.0"
        };
    }

    public async Task UpdateLocationAsync(Coordinate location, string zone)
    {
        if (Location is null)
        {
            Location = new HaSensor<HaLocation>(_mqtt, Device, "CAR_LOCATION");
            await Location.AnnounceAsync();
        }

        Location.State = zone;
        Location.Attributes = new HaLocation
        {
            Latitude = location.Latitude.ToDouble(),
            Longitude = location.Longitude.ToDouble(),
            SourceType = "gps",
            GpsAccuracy = 2
        };

        await Location.PublishStateAsync();

        // _logger.LogInformation("Car is at location: {location}", context.Tracker.Dump());
        // _logger.LogDebug("Announce sensor: {sensor}", context.Tracker.Dump());
    }

    public async Task UpdateSensorsAsync(JsonNode vehicleInfoDetails, bool shouldConvertKmToMiles)
    {
        var flattenDetails = vehicleInfoDetails.Flatten("car");
        foreach (var detail in flattenDetails)
        {
            if (detail.Key.EndsWith("_unit"))
                continue;

            if (!Sensors.TryGetValue(detail.Key, out var sensor))
            {
                sensor = new HaSensor(_mqtt, Device, detail.Key);
                Sensors.Add(detail.Key, sensor);
                // _logger.LogDebug("Announce sensors: {sensor}", sensors.Dump());
                await sensor.AnnounceAsync();
            }

            sensor.State = detail.Value;

            if (sensor.Name == "car_evInfo_battery_stateOfCharge")
            {
                sensor.DeviceClass = "battery";
                sensor.Unit = "%";
            }
            else if (sensor.Name == "car_evInfo_battery_timeToFullyChargeL2")
            {
                sensor.DeviceClass = "duration";
                sensor.Unit = "min";
            }
            else if (detail.Key.EndsWith("_value"))
            {
                var unitKey = detail.Key.Replace("_value", "_unit");

                flattenDetails.TryGetValue(unitKey, out var tmpUnit);

                if (tmpUnit == "km")
                {
                    sensor.DeviceClass = "distance";

                    if (shouldConvertKmToMiles && int.TryParse(detail.Value, out var kmValue))
                    {
                        var miValue = Math.Round(kmValue * 0.62137, 2);
                        sensor.State = miValue.ToString(CultureInfo.InvariantCulture);
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

            await sensor.PublishStateAsync();
        }

        // _logger.LogInformation("Pushing new sensors and values to Home Assistant");

        // await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.AnnounceAsync(); });

        // _logger.LogDebug("Waiting for home assistant to process all sensors");
        // await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        // await Parallel.ForEachAsync(sensors.Values, cancellationToken, async (sensor, token) => { await sensor.PublishStateAsync(); });
    }

    public async Task UpdateEntityAsync<TEntity>(string name, Func<TEntity, Task> onPressedCommand) where TEntity : HaCommand<TEntity>
    {
        if (Entities.ContainsKey(name))
            return;
        var button = Activator.CreateInstance(typeof(TEntity), _mqtt, Device, name, onPressedCommand) as TEntity;
        Entities.Add(name, button!);
        await button!.AnnounceAsync();
    }

    public async Task UpdateTimestampAsync()
    {
        if (Timestamp is null)
        {
            Timestamp = new HaSensor(_mqtt, Device, "LAST_UPDATE")
            {
                DeviceClass = "timestamp"
            };
            await Timestamp.AnnounceAsync();
        }

        Timestamp.State = DateTime.Now.ToString("O");

        await Timestamp.PublishStateAsync();
    }
}