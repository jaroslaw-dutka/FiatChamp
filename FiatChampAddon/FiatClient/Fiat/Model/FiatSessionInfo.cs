using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatSessionInfo
{
    [JsonPropertyName("login_token")]
    public string LoginToken { get; set; }

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; }
}