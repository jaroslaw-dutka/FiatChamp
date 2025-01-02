using System.Security.Cryptography;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using FiatChamp.Fiat.Entities;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using Amazon.Util;
using FiatChamp.Aws;

namespace FiatChamp.Fiat;

public class FiatClient : IFiatClient
{
    private readonly ILogger<FiatClient> _logger;
    private readonly IFiatApiClient _apiClient;
    private readonly FiatApiConfig _apiConfig;
    private readonly AmazonCognitoIdentityClient _cognitoClient;

    private FiatSession? _fiatSession;
    
    public FiatClient(ILogger<FiatClient> logger, IFiatApiConfigProvider configProvider, IFiatApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
        _apiConfig = configProvider.Get();
        _cognitoClient = new AmazonCognitoIdentityClient(new AnonymousAWSCredentials(), _apiConfig.AwsEndpoint);
    }

    public async Task LoginAndKeepSessionAliveAsync(CancellationToken cancellationToken)
    {
        if (_fiatSession is not null)
            return;

        await Login();

        _ = Task.Run(async () =>
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));

            while (await timer.WaitForNextTickAsync(cancellationToken))
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
        }, cancellationToken);
    }

    public async Task SendCommandAsync(string vin, string command, string pin, string action)
    {
        ArgumentNullException.ThrowIfNull(_fiatSession);
        
        var pinAuthResponse = await _apiClient.AuthenticatePin(_fiatSession, pin);
        var commandResponse = await _apiClient.SendCommand(_fiatSession, pinAuthResponse.Token, vin, action, command);
        commandResponse.ThrowOnError("Cannot execute command");
    }

    public async Task<List<VehicleInfo>> FetchAsync()
    {
        ArgumentNullException.ThrowIfNull(_fiatSession);

        var result = new List<VehicleInfo>();

        var vehicleResponse = await _apiClient.GetVehicles(_fiatSession);
        foreach (var vehicle in vehicleResponse.Vehicles)
        {
            result.Add(new VehicleInfo
            {
                Vehicle = vehicle,
                Details = await _apiClient.GetVehicleDetails(_fiatSession, vehicle.Vin),
                Location = await _apiClient.GetVehicleLocation(_fiatSession, vehicle.Vin)
            });
        }

        return result;
    }

    public async Task ConnectToMqtt()
    {
        var uri = new Uri("wss://ahwxpxjb5ckg1-ats.iot.eu-west-1.amazonaws.com:443/mqtt");

        var contentHash = AWSSDKUtils.ToHex(SHA256.HashData(ReadOnlySpan<byte>.Empty), true);
        var url = AwsSigner.SignQuery(_fiatSession.AwsCredentials, "GET", uri, DateTime.UtcNow, _apiConfig.AwsEndpoint.SystemName, "iotdata", contentHash);

        var builder = new MqttClientOptionsBuilder()
            .WithClientId("NGI1OTlmNTEtNGQyNC01NQ==")
            .WithWebSocketServer(builder =>
            {
                builder.WithUri(url);
                builder.WithRequestHeaders(new Dictionary<string, string>
                {
                    { "host", uri.Host }
                });
            })
            .WithTls()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(15))
            .WithCleanSession();

        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(builder.Build())
            .Build();

        var client = new MqttFactory().CreateManagedMqttClient();
        await client.StartAsync(options);

        client.ApplicationMessageReceivedAsync += async args =>
        {
            var msg = args.ApplicationMessage;
            var payload = msg.ConvertPayloadToString();
            _logger.LogInformation(msg.Topic + ": " + payload);
        };

        client.ConnectedAsync += args =>
        {
            _logger.LogInformation("Connection to mqtt succeeded: " + args.ConnectResult.ReasonString);
            return Task.CompletedTask;
        };

        client.ConnectingFailedAsync += args =>
        {
            _logger.LogInformation("Connection to mqtt failed: " + args.ConnectResult.ReasonString);
            return Task.CompletedTask;
        };

        client.DisconnectedAsync += args =>
        {
            _logger.LogInformation("Disconnected from mqtt" + args.ReasonString);
            return Task.CompletedTask;
        };

        await client.SubscribeAsync("channels/" + _fiatSession.UserId + "/+/notifications/updates");
    }

    private async Task Login()
    {
        var bootstrapResponse = await _apiClient.Bootstrap();
        bootstrapResponse.ThrowOnError("Login failed.");

        var loginResponse = await _apiClient.Login();
        loginResponse.ThrowOnError("Authentication failed.");

        var tokenResponse = await _apiClient.GetToken(loginResponse.SessionInfo.LoginToken);
        tokenResponse.ThrowOnError("Authentication failed.");

        var identityResponse = await _apiClient.GetIdentity(tokenResponse.IdToken);
        identityResponse.ThrowOnError("Identity failed.");

        var credentialsResponse = await _cognitoClient.GetCredentialsForIdentityAsync(identityResponse.IdentityId, new Dictionary<string, string>
        {
            { "cognito-identity.amazonaws.com", identityResponse.Token }
        });

        _fiatSession = new FiatSession
        {
            UserId = loginResponse.UID,
            AwsCredentials = new ImmutableCredentials(credentialsResponse.Credentials.AccessKeyId, credentialsResponse.Credentials.SecretKey, credentialsResponse.Credentials.SessionToken)
        };
    }
}