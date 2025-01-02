using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSwitch : HaEntity
{
    private readonly string _commandTopic;
    private readonly string _stateTopic;
    private readonly string _configTopic;

    public bool IsOn { get; private set; }

    public void SwitchTo(bool onOrOff)
    {
        IsOn = onOrOff;
        _ = PublishStateAsync();
    }

    public HaSwitch(IHaMqttClient mqttClient, string name, HaDevice haDevice, Func<HaSwitch, Task> onSwitchCommand)
        : base(mqttClient, name, haDevice)
    {
        _commandTopic = $"homeassistant/switch/{_id}/set";
        _stateTopic = $"homeassistant/switch/{_id}/state";
        _configTopic = $"homeassistant/switch/{_id}/config";

        _ = mqttClient.SubscribeAsync(_commandTopic, async message =>
        {
            SwitchTo(message == "ON");
            await Task.Delay(100);
            await onSwitchCommand.Invoke(this);
        });
    }

    public override async Task PublishStateAsync() =>
        await _mqttClient.PublishAsync(_stateTopic, IsOn ? "ON" : "OFF");

    public override async Task AnnounceAsync() =>
        await _mqttClient.PublishJsonAsync(_configTopic, new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            CommandTopic = _commandTopic,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
        });
}