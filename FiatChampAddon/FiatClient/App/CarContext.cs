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
    private readonly IHaMqttClient _haMqttClient;

    public string Vin { get; }
    public HaDevice Device { get; }
    public HaSensor<HaLocation>? Location { get; private set;}
    public HaSensor? Timestamp { get; private set; }
    public Dictionary<string, HaSensor> Sensors { get; } = new();
    public Dictionary<string, HaEntity> Entities { get;} = new();
    
    public CarContext(IHaMqttClient haMqttClient, Vehicle vehicle)
    {
        _haMqttClient = haMqttClient;

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

    public async Task ProcessLocationAsync(VehicleLocation location, string zone)
    {
        if (Location is null)
        {
            Location = new HaSensor<HaLocation>(Device, "CAR_LOCATION")
            {
                Icon = "mdi:map-marker"
            };
            await _haMqttClient.AnnounceAsync(Location);
        }

        Location.State = zone;
        Location.Attributes = new HaLocation
        {
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            SourceType = "gps",
            GpsAccuracy = 2
        };

        await _haMqttClient.PublishAsync(Location);
    }

    public async Task ProcessDetailsAsync(JsonNode vehicleInfoDetails, bool shouldConvertKmToMiles)
    {
        var flattenDetails = vehicleInfoDetails.Flatten("car");
        foreach (var detail in flattenDetails)
        {
            if (detail.Key.EndsWith("_unit"))
                continue;

            if (!Sensors.TryGetValue(detail.Key, out var sensor))
            {
                sensor = new HaSensor(Device, detail.Key);
                Sensors.Add(detail.Key, sensor);
                await _haMqttClient.AnnounceAsync(sensor);
            }

            sensor.State = detail.Value;

            if (sensor.Name == "car_evInfo_battery_stateOfCharge")
            {
                sensor.DeviceClass = "battery";
                sensor.UnitOfMeasurement = "%";
            }
            else if (sensor.Name == "car_evInfo_battery_timeToFullyChargeL2")
            {
                sensor.DeviceClass = "duration";
                sensor.UnitOfMeasurement = "min";
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
                        sensor.UnitOfMeasurement = "V";
                        break;
                    case null or "null":
                        sensor.UnitOfMeasurement = "";
                        break;
                    default:
                        sensor.UnitOfMeasurement = tmpUnit;
                        break;
                }
            }

            await _haMqttClient.PublishAsync(sensor);
        }
    }

    public async Task ProcessButtonAsync(string name, Func<HaButton, string, Task> action)
    {
        if (Entities.ContainsKey(name))
            return;

        var button = new HaButton(Device, name, action);
        Entities.Add(name, button);

        _haMqttClient.Subscribe(button);
        await _haMqttClient.AnnounceAsync(button);
    }

    public async Task ProcessSwitchAsync(string name, Func<HaSwitch, string, Task> action)
    {
        if (Entities.ContainsKey(name))
            return;

        var @switch = new HaSwitch(Device, name, action);
        Entities.Add(name, @switch);

        _haMqttClient.Subscribe(@switch);
        await _haMqttClient.AnnounceAsync(@switch);
    }

    public async Task ProcessTimestampAsync()
    {
        if (Timestamp is null)
        {
            Timestamp = new HaSensor(Device, "LAST_UPDATE")
            {
                DeviceClass = "timestamp",
                Icon = "mdi:timer-sync"
            };
            await _haMqttClient.AnnounceAsync(Timestamp);
        }

        Timestamp.State = DateTime.Now.ToString("O");

        await _haMqttClient.PublishAsync(Timestamp);
    }
}