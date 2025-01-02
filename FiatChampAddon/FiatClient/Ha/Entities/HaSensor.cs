using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSensor : HaEntity
{
    public string Value { get; set; }
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; }
    public string DeviceClass { get; set; }

    public HaSensor(IHaMqttClient mqttClient, HaDevice device, string name) : base("sensor", mqttClient, device, name)
    {
    }

    public override async Task PublishStateAsync() =>
        await MqttClient.PublishAsync(StateTopic, $"{Value}");

    public override async Task AnnounceAsync() => await MqttClient.PublishJsonAsync(ConfigTopic, new HaAnnouncement
    {
        Device = Device,
        Name = Name,
        UnitOfMeasurement = Unit,
        DeviceClass = DeviceClass,
        Icon = Icon,
        StateTopic = StateTopic,
        UniqueId = Id,
        Platform = "mqtt",
    });
}