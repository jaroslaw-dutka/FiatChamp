using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaEntity
{
    private readonly string _commandTopic;
    private readonly string _configTopic;

    public HaButton(IHaMqttClient mqttClient, string name, HaDevice haDevice, Func<HaButton, Task> onPressedCommand) : base(mqttClient, name, haDevice)
    {
        _commandTopic = $"homeassistant/button/{_id}/set";
        _configTopic = $"homeassistant/button/{_id}/config";
        _ = mqttClient.SubscribeAsync(_commandTopic, async _ => { await onPressedCommand.Invoke(this); });
    }

    public override Task PublishStateAsync() =>
        Task.CompletedTask;

    public override async Task AnnounceAsync() =>
        await _mqttClient.PublishJsonAsync(_configTopic, new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            CommandTopic = _commandTopic,
            UniqueId = _id,
            Platform = "mqtt",
        });
}