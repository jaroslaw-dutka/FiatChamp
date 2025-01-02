using System.Text;
using System.Text.Json.Nodes;
using FiatChamp.Extensions;
using FiatChamp.Fiat.Entities;
using FiatChamp.Fiat.Model;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Options;

namespace FiatChamp.Fiat;

public class FiatApiClient : IFiatApiClient
{
    private readonly CookieJar _cookieJar = new();
    private readonly FiatSettings _settings;
    private readonly FiatApiConfig _apiConfig;
    private readonly IFlurlClient _flurlClient;

    public FiatApiClient(IOptions<FiatSettings> options, IFiatApiConfigFactory configFactory, IFlurlClientCache flurlClientCache)
    {
        _settings = options.Value;
        _apiConfig = configFactory.Create(options.Value);
        _flurlClient = flurlClientCache.GetOrAdd(string.Empty);
    }

    public async Task<FiatBootstrapResponse> Bootstrap() => await _flurlClient
        .Request(_apiConfig.LoginUrl)
        .AppendPathSegment("accounts.webSdkBootstrap")
        .SetQueryParam("apiKey", _apiConfig.LoginApiKey)
        .WithCookies(_cookieJar)
        .GetJsonAsync<FiatBootstrapResponse>();

    public async Task<FiatLoginResponse> Login() => await _flurlClient
        .Request(_apiConfig.LoginUrl)
        .AppendPathSegment("accounts.login")
        .WithCookies(_cookieJar)
        .PostUrlEncodedAsync(WithFiatParameters(new()
        {
            { "loginID", _settings.User },
            { "password", _settings.Password },
            { "sessionExpiration", TimeSpan.FromMinutes(5).TotalSeconds },
            { "include", "profile,data,emails,subscriptions,preferences" },
        }))
        .ReceiveJson<FiatLoginResponse>();

    public async Task<FiatTokenResponse> GetToken(string loginToken) => await _flurlClient
        .Request(_apiConfig.LoginUrl)
        .AppendPathSegment("accounts.getJWT")
        .SetQueryParams(WithFiatParameters(new()
        {
            { "fields", "profile.firstName,profile.lastName,profile.email,country,locale,data.disclaimerCodeGSDP" },
            { "login_token", loginToken }
        }))
        .WithCookies(_cookieJar)
        .GetJsonAsync<FiatTokenResponse>();

    public async Task<FcaIdentityResponse> GetIdentity(string idToken) => await _flurlClient
        .Request(_apiConfig.TokenUrl)
        .WithHeader("content-type", "application/json")
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .PostJsonAsync(new
        {
            gigya_token = idToken,
        })
        .ReceiveJson<FcaIdentityResponse>();

    public async Task<FcaPinAuthResponse> AuthenticatePin(FiatSession session, string pin) => await _flurlClient
        .Request(_apiConfig.AuthUrl)
        .AppendPathSegments("v1", "accounts", session.UserId, "ignite", "pin", "authenticate")
        .WithHeaders(WithAwsHeaders(_apiConfig.AuthApiKey))
        .AwsSignAndPostJsonAsync(session.AwsCredentials, _apiConfig.AwsEndpoint, new
        {
            pin = Convert.ToBase64String(Encoding.UTF8.GetBytes(pin))
        })
        .ReceiveJson<FcaPinAuthResponse>();

    public async Task<FcaCommandResponse> SendCommand(FiatSession session, string pinAuth, string vin, string action, string command) => await _flurlClient
        .Request(_apiConfig.ApiUrl)
        .AppendPathSegments("v1", "accounts", session.UserId, "vehicles", vin, action)
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .AwsSignAndPostJsonAsync(session.AwsCredentials, _apiConfig.AwsEndpoint, new
        {
            command,
            pinAuth
        })
        .ReceiveJson<FcaCommandResponse>();

    public async Task<VehicleResponse> GetVehicles(FiatSession session) => await _flurlClient
        .Request(_apiConfig.ApiUrl)
        .AppendPathSegments("v4", "accounts", session.UserId, "vehicles")
        .SetQueryParam("stage", "ALL")
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .AwsSign(session.AwsCredentials, _apiConfig.AwsEndpoint)
        .GetJsonAsync<VehicleResponse>();

    public async Task<JsonObject> GetVehicleDetails(FiatSession session, string vin) => await _flurlClient
        .Request(_apiConfig.ApiUrl)
        .AppendPathSegments("v2", "accounts", session.UserId, "vehicles", vin, "status")
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .AwsSign(session.AwsCredentials, _apiConfig.AwsEndpoint)
        .GetJsonAsync<JsonObject>();

    public async Task<VehicleLocation> GetVehicleLocation(FiatSession session, string vin) => await _flurlClient
        .Request(_apiConfig.ApiUrl)
        .AppendPathSegments("v1", "accounts", session.UserId, "vehicles", vin, "location", "lastknown")
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .AwsSign(session.AwsCredentials, _apiConfig.AwsEndpoint)
        .GetJsonAsync<VehicleLocation>();

    public async Task<NotificationsResponse> GetNotifications(FiatSession session) => await _flurlClient
        .Request(_apiConfig.ApiUrl)
        .AppendPathSegments("v4", "accounts", session.UserId, "notifications", "summary")
        .SetQueryParam("brand", "ALL")
        .SetQueryParam("since", 1732399706408)
        .SetQueryParam("till", 1735647290831)
        .WithHeaders(WithAwsHeaders(_apiConfig.ApiKey))
        .AwsSign(session.AwsCredentials, _apiConfig.AwsEndpoint)
        .GetJsonAsync<NotificationsResponse>();

    private Dictionary<string, object> WithFiatParameters(Dictionary<string, object>? parameters = null)
    {
        var dict = new Dictionary<string, object>
        {
            { "targetEnv", "jssdk" },
            { "loginMode", "standard" },
            { "sdk", "js_latest" },
            { "authMode", "cookie" },
            { "sdkBuild", "12234" },
            { "format", "json" },
            { "APIKey", _apiConfig.LoginApiKey },
        };

        foreach (var parameter in parameters ?? new())
            dict.Add(parameter.Key, parameter.Value);

        return dict;
    }

    private Dictionary<string, object> WithAwsHeaders(string apiKey) => new()
    {
        { "X-Api-Key", apiKey },
        { "X-ClientApp-Name", "CWP" },
        { "X-ClientApp-Version", "1.0" },
        { "A-Originator-Type", "web" },
        { "ClientRequestId", Guid.NewGuid().ToString("N")[..16] },
        { "Locale", _apiConfig.Locale },
    };
}