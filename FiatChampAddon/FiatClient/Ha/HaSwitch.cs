using System.Text.Json;
using FiatChamp.Mqtt;

namespace FiatChamp.Ha;

public class HaSwitch : HaEntity
{
    private readonly string _commandTopic;
    private readonly string _stateTopic;
    private readonly string _configTopic;

    public bool IsOn { get; private set; }

    public void SwitchTo(bool onOrOff)
    {
        IsOn = onOrOff;
        _ = PublishState();
    }

    public HaSwitch(IMqttClient mqttClient, string name, HaDevice haDevice, Func<HaSwitch, Task> onSwitchCommand)
        : base(mqttClient, name, haDevice)
    {
        _commandTopic = $"homeassistant/switch/{_id}/set";
        _stateTopic = $"homeassistant/switch/{_id}/state";
        _configTopic = $"homeassistant/switch/{_id}/config";

        _ = mqttClient.Sub(_commandTopic, async message =>
        {
            SwitchTo(message == "ON");
            await Task.Delay(100);
            await onSwitchCommand.Invoke(this);
        });
    }

    public override async Task PublishState()
    {
        var mqttState = IsOn ? "ON" : "OFF";

        await _mqttClient.Pub(_stateTopic, $"{mqttState}");
    }

    public override async Task Announce()
    {
        await _mqttClient.Pub(_configTopic, JsonSerializer.Serialize(new HaAnnouncement
        {
            Device = _haDevice,
            Name = _name,
            CommandTopic = _commandTopic,
            StateTopic = _stateTopic,
            UniqueId = _id,
            Platform = "mqtt",
        }));
    }
}