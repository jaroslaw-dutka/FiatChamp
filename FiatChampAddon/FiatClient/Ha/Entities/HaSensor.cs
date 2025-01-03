using FiatChamp.Ha.Model;
using System.Text.Json;

namespace FiatChamp.Ha.Entities;

public class HaSensor : HaEntity, IHaEntityState
{
    public string State { get; set; }
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; }
    public string DeviceClass { get; set; }

    public HaSensor(IHaMqttClient mqttClient, HaDevice device, string name) : base("sensor", mqttClient, device, name)
    {
    }

    protected override void BuildAnnouncement(HaAnnouncement announcement)
    {
        announcement.UnitOfMeasurement = Unit;
        announcement.DeviceClass = DeviceClass;
        announcement.Icon = Icon;
    }
}

public class HaSensor<T> : HaSensor, IHaEntityAttributes
{
    public string SerializedAttributes => JsonSerializer.Serialize(Attributes);
    public T Attributes { get; set; }

    public HaSensor(IHaMqttClient mqttClient, HaDevice device, string name) : base(mqttClient, device, name)
    {
    }
}