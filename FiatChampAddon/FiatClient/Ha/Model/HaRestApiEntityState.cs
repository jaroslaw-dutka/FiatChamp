using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Ha.Model;

public class HaRestApiEntityState
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = null!;

    [JsonPropertyName("state")]
    public string State { get; set; } = null!;

    [JsonPropertyName("attributes")]
    public JObject Attributes { get; set; } = new();

    public T AttrTo<T>()
    {
        return Attributes.ToObject<T>();
    }
}