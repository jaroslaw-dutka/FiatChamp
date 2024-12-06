using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FiatChamp.Ha;

public class HaRestApiEntityState
{
    [JsonProperty("entity_id")] 
    public string EntityId { get; set; } = null!;

    public string State { get; set; } = null!;

    public JObject Attributes { get; set; } = new();

    public T AttrTo<T>()
    {
        return Attributes.ToObject<T>();
    }
}