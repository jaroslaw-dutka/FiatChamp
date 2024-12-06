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

    public HaSwitch(SimpleMqttClient mqttClient, string name, HaDevice haDevice, Func<HaSwitch, Task> onSwitchCommand)
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
        await _mqttClient.Pub(_configTopic, $$""" 
                                              {
                                                "device":{
                                                  "identifiers":["{{_haDevice.Identifier}}"],
                                                  "manufacturer":"{{_haDevice.Manufacturer}}", 
                                                  "model":"{{_haDevice.Model}}",
                                                  "name":"{{_haDevice.Name}}",
                                                  "sw_version":"{{_haDevice.Version}}"},
                                                "name":"{{_name}}",
                                                "command_topic":"{{_commandTopic}}",
                                                "state_topic":"{{_stateTopic}}",
                                                "unique_id":"{{_id}}",
                                                "platform":"mqtt"
                                              }

                                              """);
    }
}