using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

public class Vehicle
{
    public string RegStatus { get; set; }
    public string Color { get; set; }
    public int Year { get; set; }
    public string TsoBodyCode { get; set; }
    public bool NavEnabledHu { get; set; }
    public string Language { get; set; }
    public string CustomerRegStatus { get; set; }
    public string Radio { get; set; }
    public string ActivationSource { get; set; }
    public string? Nickname { get; set; }
    public string Vin { get; set; }
    public string Company { get; set; }
    public int Model { get; set; }
    public string ModelDescription { get; set; }
    public int TcuType { get; set; }
    public string Make { get; set; }
    public string BrandCode { get; set; }
    public string SoldRegion { get; set; }
    [JsonIgnore] 
    public JsonNode Details { get; set; }
    [JsonIgnore] 
    public VehicleLocation Location { get; set; }
}