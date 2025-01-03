using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaEntity
{
    protected IHaMqttClient MqttClient { get; }
    protected HaDevice Device { get; }
    protected string Id { get; }

    protected string ConfigTopic { get; }
    protected string StateTopic { get; }
    protected string AttributesTopic { get; }
    protected string CommandTopic { get; }

    public string Name { get; }

    protected HaEntity(string type, IHaMqttClient mqttClient, HaDevice device, string name)
    {
        MqttClient = mqttClient;
        Device = device;
        Id = $"{device.Identifiers.First()}_{name}";

        ConfigTopic = $"homeassistant/{type}/{Id}/config";
        StateTopic = $"homeassistant/{type}/{Id}/state";
        AttributesTopic = $"homeassistant/{type}/{Id}/attributes";
        CommandTopic = $"homeassistant/{type}/{Id}/set";
        
        Name = name;
    }

    public virtual async Task PublishStateAsync()
    {
        var state = GetState();
        if (state is not null)
            await MqttClient.PublishJsonAsync(StateTopic, GetState());
    }

    public virtual async Task AnnounceAsync()
    {
        var announcement = new HaAnnouncement
        {
            Device = Device,
            Name = Name,
            UniqueId = Id,
            Platform = "mqtt"
        };
        BuildAnnouncement(announcement);
        await MqttClient.PublishJsonAsync(ConfigTopic, announcement);
    }

    protected virtual string? GetState() => null;

    protected virtual void BuildAnnouncement(HaAnnouncement announcement)
    {
    }
}