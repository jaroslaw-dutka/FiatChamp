using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model;

public class HaLocation
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("source_type")]
    public string SourceType { get; set; }

    [JsonPropertyName("gps_accuracy")]
    public int GpsAccuracy { get; set; }
}