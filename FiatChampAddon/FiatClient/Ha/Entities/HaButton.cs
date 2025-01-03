using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaActionEntity<HaButton>
{
    public HaButton(IHaMqttClient mqttClient, HaDevice device, string name, Func<HaButton, Task> action) : base("button", mqttClient, device, name, action)
    {
        _ = mqttClient.SubscribeAsync(CommandTopic, async _ => { await action.Invoke(this); });
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