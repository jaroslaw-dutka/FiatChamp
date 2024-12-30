using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model;

public class HaUnitSystem
{
    [JsonPropertyName("length")]
    public string Length { get; set; }

    [JsonPropertyName("accumulated_precipitation")]
    public string AccumulatedPrecipitation { get; set; }

    [JsonPropertyName("area")]
    public string Area { get; set; }

    [JsonPropertyName("mass")]
    public string Mass { get; set; }

    [JsonPropertyName("pressure")]
    public string Pressure { get; set; }

    [JsonPropertyName("temperature")]
    public string Temperature { get; set; }

    [JsonPropertyName("volume")]
    public string Volume { get; set; }

    [JsonPropertyName("wind_speed")]
    public string WindSpeed { get; set; }
}