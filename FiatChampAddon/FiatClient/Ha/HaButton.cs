namespace FiatChamp.Ha;

public class HaButton : HaEntity
{
    private readonly string _commandTopic;
    private readonly string _configTopic;

    public HaButton(SimpleMqttClient mqttClient, string name, HaDevice haDevice, Func<HaButton, Task> onPressedCommand)
        : base(mqttClient, name, haDevice)
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
                                                "unique_id":"{{_id}}",
                                                "platform":"mqtt"
                                              }

                                              """);
    }
}