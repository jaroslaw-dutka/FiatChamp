using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatIgnoredParam
{
    [JsonPropertyName("paramName")]
    public string ParamName { get; set; }

    [JsonPropertyName("warningCode")]
    public int WarningCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}