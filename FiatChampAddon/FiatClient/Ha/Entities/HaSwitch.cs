using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSwitch : HaEntity
{
    public bool IsOn { get; private set; }

    public HaSwitch(IHaMqttClient mqttClient, HaDevice device, string name, Func<HaSwitch, Task> onSwitchCommand) : base("switch", mqttClient, device, name)
    {
        _ = mqttClient.SubscribeAsync(CommandTopic, async message =>
        {
            IsOn = message == "ON";
            await Task.Delay(100);
            await onSwitchCommand.Invoke(this);
            _ = PublishStateAsync();
        });
    }

    public override async Task PublishStateAsync() =>
        await MqttClient.PublishAsync(StateTopic, IsOn ? "ON" : "OFF");

    public override async Task AnnounceAsync() => await MqttClient.PublishJsonAsync(ConfigTopic, new HaAnnouncement
    {
        Device = Device,
        Name = Name,
        CommandTopic = CommandTopic,
        StateTopic = StateTopic,
        UniqueId = Id,
        Platform = "mqtt",
    });
}