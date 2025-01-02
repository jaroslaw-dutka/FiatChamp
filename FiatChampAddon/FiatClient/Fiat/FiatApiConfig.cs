using Amazon;

namespace FiatChamp.Fiat;

public class FiatApiConfig
{
    public string ClientId { get; } = Guid.NewGuid().ToString("N")[..16];
    public string LoginApiKey { get; set; }
    public string ApiKey { get; set; }
    public string AuthApiKey { get; set; }
    public string LoginUrl { get; set; }
    public string TokenUrl { get; set; }
    public string ApiUrl { get; set; }
    public string AuthUrl { get; set; }
    public string Locale { get; set; }
    public RegionEndpoint AwsEndpoint { get; set; }
}