using System.Text.Json;
using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public class HaButton : HaEntity
{
    private readonly string _commandTopic;
    private readonly string _configTopic;

    public HaButton(IMqttClient mqttClient, string name, HaDevice haDevice, Func<HaButton, Task> onPressedCommand) : base(mqttClient, name, haDevice)
    {
        _commandTopic = $"homeassistant/button/{_id}/set";
        _configTopic = $"homeassistant/button/{_id}/config";
        _ = mqttClient.Sub(_commandTopic, async _ =>
        {
            await onPressedCommand.Invoke(this);
        });
    }

    public override Task PublishState()
    {
        return Task.CompletedTask;
    }

    public override async Task Announce()
    {
        await _mqttClient.Pub(_configTopic, JsonSerializer.Serialize(new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            CommandTopic = _commandTopic,
            UniqueId = _id,
            Platform = "mqtt",
        }));
    }
}