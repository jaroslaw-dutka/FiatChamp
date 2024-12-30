using System.Text.Json.Serialization;

public class FiatJwtResponse : FiatResponse
{
    [JsonPropertyName("id_token")] 
    public string IdToken { get; set; }
}