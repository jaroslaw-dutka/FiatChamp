using System.Text.Json.Serialization;

public class FiatSessionInfo
{
    [JsonPropertyName("login_token")] 
    public string LoginToken { get; set; }
}