using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class Notification
{
    [JsonPropertyName("in-app")]
    public NotificationInApp InApp { get; set; }

    [JsonPropertyName("context")]
    public NotificationContext Context { get; set; }

    [JsonPropertyName("data")]
    public NotificationData Data { get; set; }

    [JsonPropertyName("click_action")]
    public string ClickAction { get; set; }
}