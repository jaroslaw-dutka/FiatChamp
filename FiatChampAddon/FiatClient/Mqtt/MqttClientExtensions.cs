using System.Text.Json;
using System.Text.Json.Serialization;

namespace FiatChamp.Mqtt;

public static class MqttClientExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task PubJsonAsync<T>(this IMqttClient client, string topic, T payload) => 
        await client.PubAsync(topic, JsonSerializer.Serialize(payload, SerializerOptions));
}