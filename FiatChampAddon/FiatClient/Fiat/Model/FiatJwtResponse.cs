using Newtonsoft.Json;

public class FiatJwtResponse : FiatResponse
{
    [JsonProperty("id_token")] public string IdToken { get; set; }
}