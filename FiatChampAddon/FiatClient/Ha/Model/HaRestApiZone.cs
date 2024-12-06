using CoordinateSharp;
using Newtonsoft.Json;

namespace FiatChamp.Ha.Model;

public class HaRestApiZone
{
    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public long Radius { get; set; }

    public bool Passive { get; set; }

    public string? Icon { get; set; }

    [JsonProperty("friendly_name")]
    public string FriendlyName { get; set; } = null!;

    [JsonIgnore]
    public Coordinate Coordinate => new Coordinate(Latitude, Longitude);
}