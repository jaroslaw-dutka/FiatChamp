namespace FiatChamp.Ha;

public interface IHaClient
{
    IHaApiClient ApiClient { get; }
    IHaMqttClient MqttClient { get; }
}