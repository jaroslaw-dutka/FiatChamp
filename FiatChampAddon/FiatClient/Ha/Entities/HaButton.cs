using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaEntity
{
    public HaButton(IHaMqttClient mqttClient, HaDevice device, string name, Func<HaButton, Task> onPressedCommand) : base("button", mqttClient, device, name)
    {
        _ = mqttClient.SubscribeAsync(CommandTopic, async _ => { await onPressedCommand.Invoke(this); });
    }

    public override Task PublishStateAsync() =>
        Task.CompletedTask;

    public override async Task AnnounceAsync() => await MqttClient.PublishJsonAsync(ConfigTopic, new HaAnnouncement
    {
        Device = Device,
        Name = Name,
        CommandTopic = CommandTopic,
        UniqueId = Id,
        Platform = "mqtt",
    });
}