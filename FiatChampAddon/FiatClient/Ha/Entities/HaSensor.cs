using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSensor : HaEntity
{
    public string State { get; set; }
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; }
    public string DeviceClass { get; set; }

    public HaSensor(IHaMqttClient mqttClient, HaDevice device, string name) : base("sensor", mqttClient, device, name)
    {
    }

    protected override string GetState() => State;

    protected override void BuildAnnouncement(HaAnnouncement announcement)
    {
        announcement.UnitOfMeasurement = Unit;
        announcement.DeviceClass = DeviceClass;
        announcement.Icon = Icon;
        announcement.StateTopic = StateTopic;
    }
}

public class HaSensor<TAttributes> : HaSensor
{
    public TAttributes Attributes { get; set; }

    public HaSensor(IHaMqttClient mqttClient, HaDevice device, string name) : base(mqttClient, device, name)
    {
    }

    public override async Task PublishStateAsync()
    {
        await base.PublishStateAsync();
        await MqttClient.PublishJsonAsync(AttributesTopic, Attributes);
    }

    protected override void BuildAnnouncement(HaAnnouncement announcement) => 
        announcement.JsonAttributesTopic = AttributesTopic;
}