using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class NotificationItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; }

    [JsonPropertyName("notification")]
    public Notification Notification { get; set; }

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    [JsonPropertyName("read")]
    public bool Read { get; set; }
}