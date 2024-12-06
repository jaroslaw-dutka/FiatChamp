using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model;

public class HaAnnouncement
{
    [JsonPropertyName("device")]
    public HaDevice Device { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("command_topic")]
    public string CommandTopic { get; set; }

    [JsonPropertyName("state_topic")]
    public string StateTopic { get; set; }

    [JsonPropertyName("json_attributes_topic")]
    public string JsonAttributesTopic { get; set; }

    [JsonPropertyName("unit_of_measurement")]
    public string UnitOfMeasurement { get; set; }
    [JsonPropertyName("device_class")]
    public string DeviceClass { get; set; }
    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("unique_id")]
    public string UniqueId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; }
}