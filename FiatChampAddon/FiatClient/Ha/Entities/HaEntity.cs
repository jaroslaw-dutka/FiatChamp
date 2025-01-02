using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public abstract class HaEntity
{
    protected IHaMqttClient MqttClient { get; }
    protected HaDevice Device { get; }
    protected string Id { get; }

    protected string CommandTopic { get; }
    protected string ConfigTopic { get; }
    protected string StateTopic { get; }
    protected string AttributesTopic { get; }

    public string Name { get; }

    protected HaEntity(string type, IHaMqttClient mqttClient, HaDevice device, string name)
    {
        MqttClient = mqttClient;
        Device = device;
        Id = $"{device.Identifiers.First()}_{name}";

        CommandTopic = $"homeassistant/{type}/{Id}/set";
        ConfigTopic = $"homeassistant/{type}/{Id}/config";
        StateTopic = $"homeassistant/{type}/{Id}/state";
        AttributesTopic = $"homeassistant/{type}/{Id}/attributes";

        Name = name;
    }

    public abstract Task PublishStateAsync();
    public abstract Task AnnounceAsync();
}