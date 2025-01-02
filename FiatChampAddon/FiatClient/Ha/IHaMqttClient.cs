namespace FiatChamp.Ha;

public interface IHaMqttClient
{
    Task ConnectAsync();
    Task SubscribeAsync(string topic, Func<string, Task> callback);
    Task PublishAsync(string topic, string payload);
    Task PublishJsonAsync<T>(string topic, T payload);
}