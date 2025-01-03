using System.Text.Json;
using System.Text.Json.Serialization;
using FiatChamp.Ha.Entities;
using FiatChamp.Ha.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace FiatChamp.Ha
{
    public class HaMqttClient : IHaMqttClient
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly Dictionary<string, IHaSetEntity> _setEntities = new();
        private readonly ILogger<HaMqttClient> _logger;
        private readonly HaMqttSettings _settings;
        private IManagedMqttClient _client;

        public HaMqttClient(ILogger<HaMqttClient> logger, IOptions<HaMqttSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            var builder = new MqttClientOptionsBuilder()
                .WithCleanSession()
                .WithClientId(_settings.ClientId)
                .WithTcpServer(_settings.Server, _settings.Port);

            if (string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
                _logger.LogWarning("Mqtt User/Password is EMPTY.");
            else
                builder.WithCredentials(_settings.User, _settings.Password);

            if (_settings.UseTls)
                builder.WithTlsOptions(_ => { });

            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(builder.Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            await _client.StartAsync(options);

            _client.ConnectedAsync += args =>
            {
                _logger.LogInformation("Connected to HomeAssistant MQTT: " + args.ConnectResult.ReasonString);
                return Task.CompletedTask;
            };

            _client.ConnectingFailedAsync += args =>
            {
                _logger.LogInformation("Failed to connect to HomeAssistant MQTT: " + args.ConnectResult?.ReasonString);
                return Task.CompletedTask;
            };

            _client.DisconnectedAsync += args =>
            {
                _logger.LogInformation("Disconnected from HomeAssistant MQTT" + args.ReasonString);
                return Task.CompletedTask;
            };

            _client.ApplicationMessageReceivedAsync += async args =>
            {
                var msg = args.ApplicationMessage;
                var payload = msg.ConvertPayloadToString();

                _logger.LogDebug("MQTT: {topic} - {payload}", msg.Topic, payload);

                if (_setEntities.TryGetValue(msg.Topic, out var command))
                {
                    await command.OnSetAsync(payload);
                    await PublishAsync(command);
                }
            };

            await _client.SubscribeAsync(GetTopic("+", "+", HaMqttTopic.Set));
        }

        public async Task AnnounceAsync(IHaEntity entity)
        {
            var interfaces = entity.GetType().GetInterfaces();
            var announcement = new HaAnnouncement
            {
                Device = entity.Device,
                Name = entity.Name,
                UniqueId = entity.Id,
                Platform = "mqtt",
                UnitOfMeasurement = entity.UnitOfMeasurement,
                DeviceClass = entity.DeviceClass,
                Icon = entity.Icon,
                StateTopic = interfaces.Contains(typeof(IHaStateEntity)) ? GetTopic(entity, HaMqttTopic.State) : null,
                AttributesTopic = interfaces.Contains(typeof(IHaAttributesEntity)) ? GetTopic(entity, HaMqttTopic.Attributes) : null,
                CommandTopic = interfaces.Contains(typeof(IHaSetEntity)) ? GetTopic(entity, HaMqttTopic.Set) : null,
            };
            var json = JsonSerializer.Serialize(announcement, SerializerOptions);
            await _client.EnqueueAsync(GetTopic(entity, HaMqttTopic.Config), json, retain: true);
        }

        public async Task PublishAsync(IHaEntity entity)
        {
            if (entity is IHaStateEntity stateEntity)
                await _client.EnqueueAsync(GetTopic(entity, HaMqttTopic.State), stateEntity.State, retain: true);
            if (entity is IHaAttributesEntity attributesEntity)
                await _client.EnqueueAsync(GetTopic(entity, HaMqttTopic.Attributes), attributesEntity.SerializedAttributes, retain: true);
        }

        public void Subscribe(IHaSetEntity entity) => 
            _setEntities.Add(GetTopic(entity, HaMqttTopic.Set), entity);

        private string GetTopic(IHaEntity entity, HaMqttTopic topic) =>
            GetTopic(entity.Type, entity.Id, topic);

        private string GetTopic(string type, string id, HaMqttTopic topic) =>
            $"homeassistant/{type}/{id}/{topic.ToString().ToLower()}";
    }
}
