namespace FiatChamp.Ha;

public class HaDeviceTracker : HaEntity
{
    private readonly string _stateTopic;
    private readonly string _configTopic;
    private readonly string _attributesTopic;

    public double Lat { get; set; }
    public double Lon { get; set; }
    public string StateValue { get; set; }

    public HaDeviceTracker(SimpleMqttClient mqttClient, string name, HaDevice haDevice)
        : base(mqttClient, name, haDevice)
    {
        _stateTopic = $"homeassistant/sensor/{_id}/state";
        _configTopic = $"homeassistant/sensor/{_id}/config";
        _attributesTopic = $"homeassistant/sensor/{_id}/attributes";
    }

    public override async Task PublishState()
    {
        await _mqttClient.Pub(_stateTopic, $"{StateValue}");

        await _mqttClient.Pub(_attributesTopic, $$"""
                                                  {
                                                    "latitude": {{Lat}}, 
                                                    "longitude": {{Lon}},
                                                    "source_type":"gps", 
                                                    "gps_accuracy": 2
                                                  }

                                                  """);
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
                                                "state_topic":"{{_stateTopic}}",
                                                "unique_id":"{{_id}}",
                                                "platform":"mqtt",
                                                "json_attributes_topic": "{{_attributesTopic}}"
                                              }

                                              """);

        await Task.Delay(200);
    }
}