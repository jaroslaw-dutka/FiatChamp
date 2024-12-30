using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class VehicleService
{
    [JsonPropertyName("vehicleCapable")]
    public bool VehicleCapable { get; set; }

    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonPropertyName("serviceEnabled")]
    public bool ServiceEnabled { get; set; }
}