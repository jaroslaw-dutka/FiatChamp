using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaDeviceTracker : HaEntity
{
    private readonly string _stateTopic;
    private readonly string _configTopic;
    private readonly string _attributesTopic;

    public double Lat { get; set; }
    public double Lon { get; set; }
    public string StateValue { get; set; }

    public HaDeviceTracker(IHaMqttClient mqttClient, string name, HaDevice haDevice) : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
        _attributesTopic = $"homeassistant/sensor/{_id}/attributes";
    }

    public override async Task PublishStateAsync()
    {
        await _mqttClient.PublishAsync(_stateTopic, $"{StateValue}");
        await _mqttClient.PublishJsonAsync(_attributesTopic, new HaLocation
        {
            Latitude = Lat,
            Longitude = Lon,
            SourceType = "gps",
            GpsAccuracy = 2
        });
    }

    public override async Task AnnounceAsync()
    {
        await _mqttClient.PublishJsonAsync(_configTopic, new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
            JsonAttributesTopic = _attributesTopic
        });

        await Task.Delay(200);
    }
}