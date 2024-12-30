using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class VehicleTC
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }
}