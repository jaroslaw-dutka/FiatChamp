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
        if (this is IHaEntityState haState)
            await MqttClient.PublishAsync(StateTopic, haState.State);
        if (this is IHaEntityAttributes haAttributes)
            await MqttClient.PublishAsync(AttributesTopic, haAttributes.SerializedAttributes);
    }

    public virtual async Task AnnounceAsync()
    {
        var interfaces = GetType().GetInterfaces();
        var announcement = new HaAnnouncement
        {
            Device = Device,
            Name = Name,
            UniqueId = Id,
            Platform = "mqtt",
            StateTopic = interfaces.Contains(typeof(IHaEntityState)) ? StateTopic : null,
            AttributesTopic = interfaces.Contains(typeof(IHaEntityAttributes)) ? AttributesTopic : null,
            CommandTopic = interfaces.Contains(typeof(IHaEntityCommand)) ? CommandTopic : null,
        };
        BuildAnnouncement(announcement);
        await MqttClient.PublishJsonAsync(ConfigTopic, announcement);
    }

    protected virtual void BuildAnnouncement(HaAnnouncement announcement)
    {
    }
}