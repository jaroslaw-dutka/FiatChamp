using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Ha.Model;

public class HaRestApiEntityState
{
    [JsonProperty("entity_id")]
    public string EntityId { get; set; } = null!;

    [JsonProperty("state")]
    public string State { get; set; } = null!;

    [JsonProperty("attributes")]
    public JObject Attributes { get; set; } = new();

    public T AttrTo<T>()
    {
        return Attributes.ToObject<T>();
    }
}