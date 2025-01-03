using FiatChamp.Ha.Model;

namespace FiatChamp.Ha.Entities;

public class HaSwitch : HaCommand<HaSwitch>, IHaEntityState, IHaEntityCommand
{
    public bool IsOn { get; private set; }

    public string State => IsOn ? "ON" : "OFF";

    public HaSwitch(IHaMqttClient mqttClient, HaDevice device, string name, Func<HaSwitch, Task> action) : base("switch", mqttClient, device, name, action)
    {
        _ = mqttClient.SubscribeAsync(CommandTopic, async message =>
        {
            IsOn = message == "ON";
            await Task.Delay(100);
            await action.Invoke(this);
            _ = PublishStateAsync();
        });
    }
}