using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class NotificationsGroup
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }

    [JsonPropertyName("items")]
    public List<NotificationItem> Items { get; set; }
}