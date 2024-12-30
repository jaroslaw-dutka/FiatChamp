using System.Text;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using FiatChamp.Extensions;
using FiatChamp.Fiat.Model;
using FiatChamp.Http;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FiatChamp.Fiat;

public class FiatClient : IFiatClient
{
    private readonly FiatSettings _settings;

    private readonly string _loginApiKey = "3_mOx_J2dRgjXYCdyhchv3b5lhi54eBcdCTX4BI8MORqmZCoQWhA0mV2PTlptLGUQI";
    private readonly string _apiKey = "2wGyL6PHec9o1UeLPYpoYa1SkEWqeBur9bLsi24i";
    private readonly string _loginUrl = "https://loginmyuconnect.fiat.com";
    private readonly string _tokenUrl = "https://authz.sdpr-01.fcagcv.com/v2/cognito/identity/token";
    private readonly string _apiUrl = "https://channels.sdpr-01.fcagcv.com";
    private readonly string _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // for pin
    private readonly string _authUrl = "https://mfa.fcl-01.fcagcv.com"; // for pin
    private readonly string _locale = "de_de"; // for pin
    private readonly RegionEndpoint _awsEndpoint = RegionEndpoint.EUWest1;
    private readonly CookieJar _cookieJar = new();
    private readonly IFlurlClient _defaultHttpClient;

    private (string userUid, ImmutableCredentials awsCredentials)? _loginInfo = null;

    public FiatClient(IOptions<FiatSettings> config)
    {
        _settings = config.Value;

        if (_settings.Brand == FcaBrand.Ram)
        {
            _loginApiKey = "3_7YjzjoSb7dYtCP5-D6FhPsCciggJFvM14hNPvXN9OsIiV1ujDqa4fNltDJYnHawO";
            _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            _loginUrl = "https://login-us.ramtrucks.com";
            _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            _apiUrl = "https://channels.sdpr-02.fcagcv.com";
            _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            _authUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            _awsEndpoint = RegionEndpoint.USEast1;
            _locale = "en_us";
        }
        else if (_settings.Brand == FcaBrand.Dodge)
        {
            _loginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
            _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            _loginUrl = "https://login-us.dodge.com";
            _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            _apiUrl = "https://channels.sdpr-02.fcagcv.com";
            _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            _authUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            _awsEndpoint = RegionEndpoint.USEast1;
            _locale = "en_us";
        }
        else if (_settings.Brand == FcaBrand.Fiat && _settings.Region == FcaRegion.America)
        {
            _loginApiKey = "3_etlYkCXNEhz4_KJVYDqnK1CqxQjvJStJMawBohJU2ch3kp30b0QCJtLCzxJ93N-M";
            _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
            _loginUrl = "https://login-us.fiat.com";
            _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
            _apiUrl = "https://channels.sdpr-02.fcagcv.com";
            _authApiKey = "JWRYW7IYhW9v0RqDghQSx4UcRYRILNmc8zAuh5ys"; // UNKNOWN
            _authUrl = "https://mfa.fcl-01.fcagcv.com"; // UNKNOWN
            _awsEndpoint = RegionEndpoint.USEast1;
            _locale = "en_us";
        }
        else if (_settings.Brand == FcaBrand.Jeep)
        {
            if (_settings.Region == FcaRegion.Europe)
            {
                _loginApiKey = "3_ZvJpoiZQ4jT5ACwouBG5D1seGEntHGhlL0JYlZNtj95yERzqpH4fFyIewVMmmK7j";
                _loginUrl = "https://login.jeep.com";
            }
            else
            {
                _loginApiKey = "3_5qxvrevRPG7--nEXe6huWdVvF5kV7bmmJcyLdaTJ8A45XUYpaR398QNeHkd7EB1X";
                _apiKey = "OgNqp2eAv84oZvMrXPIzP8mR8a6d9bVm1aaH9LqU";
                _loginUrl = "https://login-us.jeep.com";
                _tokenUrl = "https://authz.sdpr-02.fcagcv.com/v2/cognito/identity/token";
                _apiUrl = "https://channels.sdpr-02.fcagcv.com";
                _authApiKey = "fNQO6NjR1N6W0E5A6sTzR3YY4JGbuPv48Nj9aZci";
                _authUrl = "https://mfa.fcl-02.fcagcv.com";
                _awsEndpoint = RegionEndpoint.USEast1;
                _locale = "en_us";
            }
        }

        _defaultHttpClient = new FlurlClient().Configure(settings => { settings.HttpClientFactory = new PollyHttpClientFactory(); });
    }

    public async Task LoginAndKeepSessionAlive()
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
                    Log.Information("REFRESH SESSION");
                    await Login();
                }
                catch (Exception e)
                {

                    Log.Error("ERROR WHILE REFRESH SESSION");
                    Log.Debug("{0}", e);
                }
            }
        });
    }

    private async Task Login()
    {
        var loginResponse = await _loginUrl
            .WithClient(_defaultHttpClient)
            .AppendPathSegment("accounts.webSdkBootstrap")
            .SetQueryParam("apiKey", _loginApiKey)
            .WithCookies(_cookieJar)
            .GetJsonAsync<FiatLoginResponse>();

        Log.Debug("{0}", loginResponse.Dump());

        loginResponse.ThrowOnError("Login failed.");

        var authResponse = await _loginUrl
            .WithClient(_defaultHttpClient)
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
            .ReceiveJson<FiatAuthResponse>();

        Log.Debug("{0}", authResponse.Dump());

        authResponse.ThrowOnError("Authentication failed.");

        var jwtResponse = await _loginUrl
            .WithClient(_defaultHttpClient)
            .AppendPathSegment("accounts.getJWT")
            .SetQueryParams(
                WithFiatDefaultParameter(new()
                {
                    { "fields", "profile.firstName,profile.lastName,profile.email,country,locale,data.disclaimerCodeGSDP" },
                    { "login_token", authResponse.SessionInfo.LoginToken }
                }))
            .WithCookies(_cookieJar)
            .GetJsonAsync<FiatJwtResponse>();

        Log.Debug("{0}", jwtResponse.Dump());

        jwtResponse.ThrowOnError("Authentication failed.");

        var identityResponse = await _tokenUrl
            .WithClient(_defaultHttpClient)
            .WithHeader("content-type", "application/json")
            .WithHeaders(WithAwsDefaultParameter(_apiKey))
            .PostJsonAsync(new
            {
                gigya_token = jwtResponse.IdToken,
            })
            .ReceiveJson<FcaIdentityResponse>();

        Log.Debug("{0}", identityResponse.Dump());

        identityResponse.ThrowOnError("Identity failed.");

        var client = new AmazonCognitoIdentityClient(new AnonymousAWSCredentials(), _awsEndpoint);

        var res = await client.GetCredentialsForIdentityAsync(identityResponse.IdentityId,
            new Dictionary<string, string>()
            {
                { "cognito-identity.amazonaws.com", identityResponse.Token }
            });

        _loginInfo = (authResponse.UID, new ImmutableCredentials(res.Credentials.AccessKeyId,
            res.Credentials.SecretKey,
            res.Credentials.SessionToken));
    }

    private Dictionary<string, object> WithAwsDefaultParameter(string apiKey, Dictionary<string, object>? parameters = null)
    {
        var dict = new Dictionary<string, object>()
        {
            { "x-clientapp-name", "CWP" },
            { "x-clientapp-version", "1.0" },
            { "clientrequestid", Guid.NewGuid().ToString("N")[..16] },
            { "x-api-key", apiKey },
            { "locale", _locale },
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
            { "APIKey", _loginApiKey },
        };

        foreach (var parameter in parameters ?? new())
            dict.Add(parameter.Key, parameter.Value);

        return dict;
    }

    public async Task SendCommand(string vin, string command, string pin, string action)
    {
        ArgumentNullException.ThrowIfNull(_loginInfo);

        var (userUid, awsCredentials) = _loginInfo.Value;

        var data = new
        {
            pin = Convert.ToBase64String(Encoding.UTF8.GetBytes(pin))
        };

        var pinAuthResponse = await _authUrl
            .AppendPathSegments("v1", "accounts", userUid, "ignite", "pin", "authenticate")
            .WithHeaders(WithAwsDefaultParameter(_authApiKey))
            .AwsSign(awsCredentials, _awsEndpoint, data)
            .PostJsonAsync(data)
            .ReceiveJson<FcaPinAuthResponse>();

        Log.Debug("{0}", pinAuthResponse.Dump());

        var json = new
        {
            command,
            pinAuth = pinAuthResponse.Token
        };

        var commandResponse = await _apiUrl
            .AppendPathSegments("v1", "accounts", userUid, "vehicles", vin, action)
            .WithHeaders(WithAwsDefaultParameter(_apiKey))
            .AwsSign(awsCredentials, _awsEndpoint, json)
            .PostJsonAsync(json)
            .ReceiveJson<FcaCommandResponse>();

        Log.Debug("{0}", commandResponse.Dump());
    }

    public async Task<Vehicle[]> Fetch()
    {
        ArgumentNullException.ThrowIfNull(_loginInfo);

        var (userUid, awsCredentials) = _loginInfo.Value;

        var vehicleResponse = await _apiUrl
            .WithClient(_defaultHttpClient)
            .AppendPathSegments("v4", "accounts", userUid, "vehicles")
            .SetQueryParam("stage", "ALL")
            .WithHeaders(WithAwsDefaultParameter(_apiKey))
            .AwsSign(awsCredentials, _awsEndpoint)
            .GetJsonAsync<VehicleResponse>();

        Log.Debug("{0}", vehicleResponse.Dump());

        foreach (var vehicle in vehicleResponse.Vehicles)
        {
            var vehicleDetails = await _apiUrl
                .WithClient(_defaultHttpClient)
                .AppendPathSegments("v2", "accounts", userUid, "vehicles", vehicle.Vin, "status")
                .WithHeaders(WithAwsDefaultParameter(_apiKey))
                .AwsSign(awsCredentials, _awsEndpoint)
                .GetJsonAsync<JObject>();

            Log.Debug("{0}", vehicleDetails.Dump());

            vehicle.Details = vehicleDetails;

            var vehicleLocation = await _apiUrl
                .WithClient(_defaultHttpClient)
                .AppendPathSegments("v1", "accounts", userUid, "vehicles", vehicle.Vin, "location", "lastknown")
                .WithHeaders(WithAwsDefaultParameter(_apiKey))
                .AwsSign(awsCredentials, _awsEndpoint)
                .GetJsonAsync<VehicleLocation>();

            vehicle.Location = vehicleLocation;

            Log.Debug("{0}", vehicleLocation.Dump());
        }

        return vehicleResponse.Vehicles;
    }
}