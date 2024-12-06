using System.Text.Json;
using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public class HaDeviceTracker : HaEntity
{
    private readonly string _stateTopic;
    private readonly string _configTopic;
    private readonly string _attributesTopic;

    public double Lat { get; set; }
    public double Lon { get; set; }
    public string StateValue { get; set; }

    public HaDeviceTracker(IMqttClient mqttClient, string name, HaDevice haDevice) : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
        _attributesTopic = $"homeassistant/sensor/{_id}/attributes";
    }

    public override async Task PublishState()
    {
        await _mqttClient.Pub(_stateTopic, $"{StateValue}");

        await _mqttClient.Pub(_attributesTopic, $$"""
                                                  {
                                                    "latitude": {{Lat}}, 
                                                    "longitude": {{Lon}},
                                                    "source_type":"gps", 
                                                    "gps_accuracy": 2
                                                  }

                                                  """);
    }

    public override async Task Announce()
    {
        await _mqttClient.Pub(_configTopic, JsonSerializer.Serialize(new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
            JsonAttributesTopic = _attributesTopic
        }));

        await Task.Delay(200);
    }
}