using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class NotificationData
{
    [JsonPropertyName("roRequestId")]
    public string RoRequestId { get; set; }

    [JsonPropertyName("response")]
    public string Response { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("notificationId")]
    public string NotificationId { get; set; }
}