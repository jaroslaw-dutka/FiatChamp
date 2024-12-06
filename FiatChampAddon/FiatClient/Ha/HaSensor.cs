using System.Text.Json;
using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public class HaSensor : HaEntity
{
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; } = "";
    public string DeviceClass { get; set; } = "";

    private readonly string _stateTopic;
    private readonly string _configTopic;

    public HaSensor(IMqttClient mqttClient, string name, HaDevice haDevice)
        : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
    }

    public override async Task PublishState()
    {
        await _mqttClient.Pub(_stateTopic, $"{Value}");
    }

    public override async Task Announce()
    {
        await _mqttClient.Pub(_configTopic, JsonSerializer.Serialize(new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            UnitOfMeasurement = Unit,
            DeviceClass = DeviceClass,
            Icon = Icon,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
        }));

        await Task.Delay(200);
    }
}