using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Vehicle
{
    public string RegStatus { get; set; }
    public string Color { get; set; }
    public long Year { get; set; }
    public string TsoBodyCode { get; set; }
    public bool NavEnabledHu { get; set; }
    public string Language { get; set; }
    public string CustomerRegStatus { get; set; }
    public string Radio { get; set; }
    public string ActivationSource { get; set; }
    public string? Nickname { get; set; }
    public string Vin { get; set; }
    public string Company { get; set; }
    public string Model { get; set; }
    public string ModelDescription { get; set; }
    public long TcuType { get; set; }
    public string Make { get; set; }
    public string BrandCode { get; set; }
    public string SoldRegion { get; set; }
    [JsonIgnore] public JObject Details { get; set; }
    [JsonIgnore] public VehicleLocation Location { get; set; }
}