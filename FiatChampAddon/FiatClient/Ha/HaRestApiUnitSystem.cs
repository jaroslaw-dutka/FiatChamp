using Newtonsoft.Json;
using Serilog;

namespace FiatChamp.Ha;

public class HaRestApiUnitSystem
{
    public string Length { get; set; }
    public string Mass { get; set; }
    public string Pressure { get; set; }
    public string Temperature { get; set; }
    public string Volume { get; set; }
    [JsonProperty("wind_speed")] public string WindSpeed { get; set; }

    [JsonProperty("accumulated_precipitation")]
    public string AccumulatedPrecipitation { get; set; }
}