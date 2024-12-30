using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatResponse : IFiatResponse
{
    [JsonPropertyName("callId")]
    public string CallId { get; set; }

    [JsonPropertyName("errorCode")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("errorDetails")]
    public string ErrorDetails { get; set; }

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("apiVersion")]
    public int ApiVersion { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("statusReason")]
    public string StatusReason { get; set; }

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("hasGmid")]
    public string HasGmid { get; set; }

    [JsonPropertyName("ignoredParams")]
    public List<FiatIgnoredParam> IgnoredParams { get; set; }

    public bool CheckForError() => StatusCode != 200;

    public void ThrowOnError(string message)
    {
        if (CheckForError())
            throw new Exception(message + $" {ErrorCode} {StatusReason} {ErrorMessage}");
    }
}