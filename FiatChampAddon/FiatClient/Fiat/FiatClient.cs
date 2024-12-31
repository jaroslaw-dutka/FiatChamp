using System.Text;
using System.Text.Json.Nodes;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using FiatChamp.Extensions;
using FiatChamp.Fiat.Entities;
using FiatChamp.Fiat.Model;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FiatChamp.Fiat;

public class FiatClient : IFiatClient
{
    private readonly CookieJar _cookieJar = new();

    private readonly ILogger<FiatClient> _logger;
    private readonly FiatSettings _settings;

    private readonly FiatApiConfig _apiConfig;
    private readonly IFlurlClient _httpClient;

    private (string userUid, ImmutableCredentials awsCredentials)? _loginInfo = null;

    public FiatClient(ILogger<FiatClient> logger, IOptions<FiatSettings> config, IFlurlClientCache flurlClientCache)
    {
        _logger = logger;
        
        _settings = config.Value;
        _apiConfig = new FiatApiConfig(_settings);
        _httpClient = flurlClientCache.GetOrAdd(string.Empty);
    }

    public async Task LoginAndKeepSessionAliveAsync()
    {
        if (_loginInfo is not null)
            return;

        await Login();

        _ = Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));

            while (await timer.WaitForNextTickAsync())
            {
                try
                {
                    _logger.LogInformation("REFRESH SESSION");
                    await Login();
                }
                catch (Exception e)
                {

                    _logger.LogError("ERROR WHILE REFRESH SESSION");
                    _logger.LogDebug("{0}", e);
                }
            }
        });
    }

    private async Task Login()
    {
        var loginResponse = await _httpClient
            .Request(_apiConfig.LoginUrl)
            .AppendPathSegment("accounts.webSdkBootstrap")
            .SetQueryParam("apiKey", _apiConfig.LoginApiKey)
            .WithCookies(_cookieJar)
            .GetJsonAsync<FiatBootstrapResponse>();

        _logger.LogDebug(loginResponse.Dump());

        loginResponse.ThrowOnError("Login failed.");

        var authResponse = await _httpClient
            .Request(_apiConfig.LoginUrl)
            .AppendPathSegment("accounts.login")
            .WithCookies(_cookieJar)
            .PostUrlEncodedAsync(
                WithFiatDefaultParameter(new()
                {
                    { "loginID", _settings.User },
                    { "password", _settings.Password },
                    { "sessionExpiration", TimeSpan.FromMinutes(5).TotalSeconds },
                    { "include", "profile,data,emails,subscriptions,preferences" },
                }))
            .ReceiveJson<FiatLoginResponse>();

        _logger.LogDebug(authResponse.Dump());

        authResponse.ThrowOnError("Authentication failed.");

        var tokenResponse = await _httpClient
            .Request(_apiConfig.LoginUrl)
            .AppendPathSegment("accounts.getJWT")
            .SetQueryParams(
                WithFiatDefaultParameter(new()
                {
                    { "fields", "profile.firstName,profile.lastName,profile.email,country,locale,data.disclaimerCodeGSDP" },
                    { "login_token", authResponse.SessionInfo.LoginToken }
                }))
            .WithCookies(_cookieJar)
            .GetJsonAsync<FiatTokenResponse>();

        _logger.LogDebug(tokenResponse.Dump());

        tokenResponse.ThrowOnError("Authentication failed.");

        var identityResponse = await _httpClient
            .Request(_apiConfig.TokenUrl)
            .WithHeader("content-type", "application/json")
            .WithHeaders(WithAwsDefaultParameter(_apiConfig.ApiKey))
            .PostJsonAsync(new
            {
                gigya_token = tokenResponse.IdToken,
            })
            .ReceiveJson<FcaIdentityResponse>();

        _logger.LogDebug(identityResponse.Dump());

        identityResponse.ThrowOnError("Identity failed.");

        var client = new AmazonCognitoIdentityClient(new AnonymousAWSCredentials(), _apiConfig.AwsEndpoint);

        var res = await client.GetCredentialsForIdentityAsync(identityResponse.IdentityId,
            new Dictionary<string, string>
            {
                { "cognito-identity.amazonaws.com", identityResponse.Token }
            });

        _loginInfo = (authResponse.UID, new ImmutableCredentials(res.Credentials.AccessKeyId,
            res.Credentials.SecretKey,
            res.Credentials.SessionToken));
    }

    public async Task SendCommandAsync(string vin, string command, string pin, string action)
    {
        ArgumentNullException.ThrowIfNull(_loginInfo);

        var (userUid, awsCredentials) = _loginInfo.Value;

        var data = new
        {
            pin = Convert.ToBase64String(Encoding.UTF8.GetBytes(pin))
        };

        var pinAuthResponse = await _httpClient
            .Request(_apiConfig.AuthUrl)
            .AppendPathSegments("v1", "accounts", userUid, "ignite", "pin", "authenticate")
            .WithHeaders(WithAwsDefaultParameter(_apiConfig.AuthApiKey))
            .AwsSign(awsCredentials, _apiConfig.AwsEndpoint, data)
            .PostJsonAsync(data)
            .ReceiveJson<FcaPinAuthResponse>();

        _logger.LogDebug(pinAuthResponse.Dump());

        var json = new
        {
            command,
            pinAuth = pinAuthResponse.Token
        };

        var commandResponse = await _httpClient
            .Request(_apiConfig.ApiUrl)
            .AppendPathSegments("v1", "accounts", userUid, "vehicles", vin, action)
            .WithHeaders(WithAwsDefaultParameter(_apiConfig.ApiKey))
            .AwsSign(awsCredentials, _apiConfig.AwsEndpoint, json)
            .PostJsonAsync(json)
            .ReceiveJson<FcaCommandResponse>();

        _logger.LogDebug(commandResponse.Dump());
    }

    public async Task<List<VehicleInfo>> FetchAsync()
    {
        ArgumentNullException.ThrowIfNull(_loginInfo);

        var result = new List<VehicleInfo>();

        var (userUid, awsCredentials) = _loginInfo.Value;

        var vehicleResponse = await _httpClient
            .Request(_apiConfig.ApiUrl)
            .AppendPathSegments("v4", "accounts", userUid, "vehicles")
            .SetQueryParam("stage", "ALL")
            .WithHeaders(WithAwsDefaultParameter(_apiConfig.ApiKey))
            .AwsSign(awsCredentials, _apiConfig.AwsEndpoint)
            .GetJsonAsync<VehicleResponse>();

        _logger.LogDebug(vehicleResponse.Dump());

        foreach (var vehicle in vehicleResponse.Vehicles)
        {
            var info = new VehicleInfo
            {
                Vehicle = vehicle,
                Details = await _httpClient
                    .Request(_apiConfig.ApiUrl)
                    .AppendPathSegments("v2", "accounts", userUid, "vehicles", vehicle.Vin, "status")
                    .WithHeaders(WithAwsDefaultParameter(_apiConfig.ApiKey))
                    .AwsSign(awsCredentials, _apiConfig.AwsEndpoint)
                    .GetJsonAsync<JsonObject>(),
                Location = await _httpClient
                    .Request(_apiConfig.ApiUrl)
                    .AppendPathSegments("v1", "accounts", userUid, "vehicles", vehicle.Vin, "location", "lastknown")
                    .WithHeaders(WithAwsDefaultParameter(_apiConfig.ApiKey))
                    .AwsSign(awsCredentials, _apiConfig.AwsEndpoint)
                    .GetJsonAsync<VehicleLocation>()
            };

            _logger.LogDebug(info.Details.Dump());
            _logger.LogDebug(info.Location.Dump());

            result.Add(info);
        }

        return result;
    }

    private Dictionary<string, object> WithAwsDefaultParameter(string apiKey, Dictionary<string, object>? parameters = null)
    {
        var dict = new Dictionary<string, object>
        {
            { "x-clientapp-name", "CWP" },
            { "x-clientapp-version", "1.0" },
            { "clientrequestid", Guid.NewGuid().ToString("N")[..16] },
            { "x-api-key", apiKey },
            { "locale", _apiConfig.Locale },
            { "x-originator-type", "web" },
        };

        foreach (var parameter in parameters ?? new())
            dict.Add(parameter.Key, parameter.Value);

        return dict;
    }

    private Dictionary<string, object> WithFiatDefaultParameter(Dictionary<string, object>? parameters = null)
    {
        var dict = new Dictionary<string, object>()
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
}