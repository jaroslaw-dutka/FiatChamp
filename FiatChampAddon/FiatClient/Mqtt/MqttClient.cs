using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace FiatChamp.Mqtt;

public class MqttClient : IMqttClient
{
    private readonly ILogger<MqttClient> _logger;
    private readonly MqttSettings _settings;
    private readonly IManagedMqttClient _mqttClient;

    public MqttClient(ILogger<MqttClient> logger, IOptions<MqttSettings> config)
    {
        _logger = logger;
        _settings = config.Value;
        _mqttClient = new MqttFactory().CreateManagedMqttClient();
    }

    public async Task Connect()
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithCleanSession()
            .WithClientId(_settings.ClientId)
            .WithTcpServer(_settings.Server, _settings.Port);

        if (string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogWarning("Mqtt User/Password is EMPTY.");
        }
        else
        {
            mqttClientOptions.WithCredentials(_settings.User, _settings.Password);
        }

        if (_settings.UseTls)
        {
            mqttClientOptions.WithTls();
        }

        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(mqttClientOptions.Build())
            .Build();

        await _mqttClient.StartAsync(options);

        _mqttClient.ConnectedAsync += async args => { _logger.LogInformation("Mqtt connection successful"); };

        _mqttClient.ConnectingFailedAsync += async args => { _logger.LogInformation("Mqtt connection failed: {0}", args.Exception); };
    }

    public async Task Sub(string topic, Func<string, Task> callback)
    {
        _mqttClient.ApplicationMessageReceivedAsync += async args =>
        {
            var msg = args.ApplicationMessage;

            if (msg.Topic == topic)
            {
                try
                {
                    await callback(msg.ConvertPayloadToString());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to process Mqtt payload.");
                }
            }
        };

        await _mqttClient.SubscribeAsync(topic);
    }

    public async Task Pub(string topic, string payload)
    {
        await _mqttClient.EnqueueAsync(topic, payload, retain: true);
    }
}