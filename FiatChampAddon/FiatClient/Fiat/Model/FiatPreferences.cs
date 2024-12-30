using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatPreferences
{
    [JsonPropertyName("isConsentGranted")]
    public bool IsConsentGranted { get; set; }

    [JsonPropertyName("docDate")]
    public DateTime DocDate { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; }

    [JsonPropertyName("lastConsentModified")]
    public DateTime LastConsentModified { get; set; }

    [JsonPropertyName("actionTimestamp")]
    public DateTime ActionTimestamp { get; set; }

    [JsonPropertyName("tags")]
    public List<object> Tags { get; set; }

    [JsonPropertyName("customData")]
    public List<object> CustomData { get; set; }

    [JsonPropertyName("entitlements")]
    public List<object> Entitlements { get; set; }
}