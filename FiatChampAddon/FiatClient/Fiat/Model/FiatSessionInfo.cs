using Newtonsoft.Json;

public class FiatSessionInfo
{
    [JsonProperty("login_token")] 
    public string LoginToken { get; set; }
}