using CoordinateSharp;
using Newtonsoft.Json;

namespace FiatChamp.Ha.Model;

public class HaRestApiZone
{
    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("radius")]
    public long Radius { get; set; }

    [JsonProperty("passive")]
    public bool Passive { get; set; }

    [JsonProperty("icon")]
    public string? Icon { get; set; }

    [JsonProperty("friendly_name")]
    public string FriendlyName { get; set; } = null!;

    [JsonIgnore]
    public Coordinate Coordinate => new(Latitude, Longitude);
}