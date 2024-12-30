using System.Text.Json.Serialization;
using CoordinateSharp;

namespace FiatChamp.Ha.Model;

public class HaRestApiZone
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("radius")]
    public long Radius { get; set; }

    [JsonPropertyName("passive")]
    public bool Passive { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; set; } = null!;

    [JsonIgnore]
    public Coordinate Coordinate => new(Latitude, Longitude);
}