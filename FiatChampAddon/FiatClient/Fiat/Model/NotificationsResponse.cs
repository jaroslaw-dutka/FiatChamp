using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class NotificationsResponse
{
    [JsonPropertyName("userid")]
    public string Userid { get; set; }

    [JsonPropertyName("startTS")]
    public long StartTimeStamp { get; set; }

    [JsonPropertyName("endTS")]
    public long EndTimeStamp { get; set; }

    [JsonPropertyName("notifications")]
    public List<NotificationsGroup> Notifications { get; set; }
}