using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class NotificationInApp
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; }

    [JsonPropertyName("body")]
    public string Body { get; set; }

    [JsonPropertyName("criticality")]
    public string Criticality { get; set; }
}