using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model;

public class HaDevice
{
    [JsonPropertyName("identifiers")]
    public string Identifiers { get; set; }

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("sw_version")]
    public string Version { get; set; }
}