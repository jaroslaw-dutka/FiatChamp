using Newtonsoft.Json;

namespace FiatChamp.Ha.Model;

public class HaRestApiUnitSystem
{
    [JsonProperty("length")]
    public string Length { get; set; }

    [JsonProperty("mass")]
    public string Mass { get; set; }

    [JsonProperty("pressure")]
    public string Pressure { get; set; }

    [JsonProperty("temperature")]
    public string Temperature { get; set; }

    [JsonProperty("volume")]
    public string Volume { get; set; }

    [JsonProperty("wind_speed")]
    public string WindSpeed { get; set; }

    [JsonProperty("accumulated_precipitation")]
    public string AccumulatedPrecipitation { get; set; }
}