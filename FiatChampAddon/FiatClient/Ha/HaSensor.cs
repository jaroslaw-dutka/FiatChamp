namespace FiatChamp.Ha;

public class HaSensor : HaEntity
{
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "mdi:eye";
    public string Unit { get; set; } = "";
    public string DeviceClass { get; set; } = "";

    private readonly string _stateTopic;
    private readonly string _configTopic;

    public HaSensor(SimpleMqttClient mqttClient, string name, HaDevice haDevice) : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
    }

    public override async Task PublishState()
    {
        await _mqttClient.Pub(_stateTopic, $"{Value}");
    }

    public override async Task Announce()
    {

        var unitOfMeasurementJson =
            string.IsNullOrWhiteSpace(Unit) ? "" : $"\"unit_of_measurement\":\"{Unit}\",";
        var deviceClassJson =
            string.IsNullOrWhiteSpace(DeviceClass) ? "" : $"\"device_class\":\"{DeviceClass}\",";
        var iconJson =
            string.IsNullOrWhiteSpace(DeviceClass) ? $"\"icon\":\"{Icon}\"," : "";

        await _mqttClient.Pub(_configTopic, $$""" 
                                              {
                                                "device":{
                                                  "identifiers":["{{_haDevice.Identifier}}"],
                                                  "manufacturer":"{{_haDevice.Manufacturer}}", 
                                                  "model":"{{_haDevice.Model}}",
                                                  "name":"{{_haDevice.Name}}",
                                                  "sw_version":"{{_haDevice.Version}}"},
                                                "name":"{{_name}}",
                                                {{unitOfMeasurementJson}}
                                                {{deviceClassJson}}
                                                {{iconJson}}
                                                "state_topic":"{{_stateTopic}}",
                                                "unique_id":"{{_id}}",
                                                "platform":"mqtt"
                                              }

                                              """);

        await Task.Delay(200);
    }
}