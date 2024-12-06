using System.Text.Json;

namespace FiatChamp.Mqtt;

public static class MqttClientExtensions
{
    public static async Task PubJson<T>(this IMqttClient client, string topic, T payload) => 
        await client.Pub(topic, JsonSerializer.Serialize(payload));
}