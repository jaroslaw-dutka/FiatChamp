namespace FiatChamp.Ha
{
    public class HaClient : IHaClient
    {
        public IHaApiClient ApiClient { get; }
        public IHaMqttClient MqttClient { get; }

        public HaClient(IHaApiClient apiClient, IHaMqttClient mqttClient)
        {
            ApiClient = apiClient;
            MqttClient = mqttClient;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await MqttClient.ConnectAsync();
        }
    }
}
