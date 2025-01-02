using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FcaCommandResponse
{
    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("correlationId")]
    public Guid CorrelationId { get; set; }

    [JsonPropertyName("responseStatus")]
    public string ResponseStatus { get; set; }

    [JsonPropertyName("statusTimestamp")]
    public long StatusTimestamp { get; set; }

    [JsonPropertyName("asyncRespTimeout")]
    public long AsyncRespTimeout { get; set; }
}