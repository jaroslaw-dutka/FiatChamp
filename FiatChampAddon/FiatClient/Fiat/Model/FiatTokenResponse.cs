using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatTokenResponse : FiatResponse
{
    [JsonPropertyName("id_token")] 
    public string IdToken { get; set; }
}