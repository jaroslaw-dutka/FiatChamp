namespace FiatChamp.Ha;

public interface IHaClient
{
    IHaApiClient ApiClient { get; }
    IHaMqttClient MqttClient { get; }

    Task ConnectAsync(CancellationToken cancellationToken);
}