using FiatChamp.Ha.Model;
using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public class HaSensor : HaEntity
{
    public string Value { get; set; }
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; }
    public string DeviceClass { get; set; }

    private readonly string _stateTopic;
    private readonly string _configTopic;

    public HaSensor(IMqttClient mqttClient, string name, HaDevice haDevice)
        : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
    }

    public override async Task PublishStateAsync() => 
        await _mqttClient.PubAsync(_stateTopic, $"{Value}");

    public override async Task AnnounceAsync()
    {
        await _mqttClient.PubJsonAsync(_configTopic, new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            UnitOfMeasurement = Unit,
            DeviceClass = DeviceClass,
            Icon = Icon,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
        });

        await Task.Delay(200);
    }
}