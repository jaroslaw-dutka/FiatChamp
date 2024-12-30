using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model;

public class HaRestApiEntityState
{
    [JsonPropertyName("entity_id")]
    public string EntityId { get; set; } = null!;

    [JsonPropertyName("state")]
    public string State { get; set; } = null!;

    [JsonPropertyName("attributes")]
    public JsonObject Attributes { get; set; } = new();
}