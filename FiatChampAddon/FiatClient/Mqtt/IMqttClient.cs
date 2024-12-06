namespace FiatChamp.Mqtt;

public interface IMqttClient
{
    Task Connect();
    Task Sub(string topic, Func<string, Task> callback);
    Task Pub(string topic, string payload);
}