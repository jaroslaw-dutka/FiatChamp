namespace FiatChamp.Mqtt;

public interface IMqttClient
{
    Task ConnectAsync();
    Task SubAsync(string topic, Func<string, Task> callback);
    Task PubAsync(string topic, string payload);
}