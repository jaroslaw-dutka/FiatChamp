using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class VehicleLocation
{
    [JsonPropertyName("timeStamp")]
    public long TimeStamp { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("altitude")]
    public double Altitude { get; set; }

    [JsonPropertyName("bearing")]
    public int Bearing { get; set; }

    [JsonPropertyName("isLocationApprox")]
    public bool IsLocationApprox { get; set; }
}