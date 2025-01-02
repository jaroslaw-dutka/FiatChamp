using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaTracker : HaEntity
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    public string StateValue { get; set; }

    public HaTracker(IHaMqttClient mqttClient, HaDevice device, string name) : base("sensor", mqttClient, device, name)
    {
    }

    public override async Task PublishStateAsync()
    {
        await MqttClient.PublishAsync(StateTopic, $"{StateValue}");
        await MqttClient.PublishJsonAsync(AttributesTopic, new HaLocation
        {
            Latitude = Lat,
            Longitude = Lon,
            SourceType = "gps",
            GpsAccuracy = 2
        });
    }

    public override async Task AnnounceAsync() => await MqttClient.PublishJsonAsync(ConfigTopic, new HaAnnouncement
    {
        Device = Device,
        Name = Name,
        StateTopic = StateTopic,
        UniqueId = Id,
        Platform = "mqtt",
        JsonAttributesTopic = AttributesTopic
    });
}