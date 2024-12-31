using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FcaPinAuthResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("expiry")]
    public long Expiry { get; set; }
}