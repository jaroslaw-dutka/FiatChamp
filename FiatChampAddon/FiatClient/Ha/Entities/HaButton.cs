using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaButton : HaCommand<HaButton>
{
    public HaButton(IHaMqttClient mqttClient, HaDevice device, string name, Func<HaButton, Task> action) : base("button", mqttClient, device, name, action)
    {
        _ = mqttClient.SubscribeAsync(CommandTopic, async _ => { await action.Invoke(this); });
    }
}