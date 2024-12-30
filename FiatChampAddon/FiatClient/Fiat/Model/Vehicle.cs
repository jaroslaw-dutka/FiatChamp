using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class Vehicle
{
    [JsonPropertyName("color")]
    public string Color { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("tsoBodyCode")]
    public string TsoBodyCode { get; set; }

    [JsonPropertyName("navEnabledHU")]
    public bool NavEnabledHU { get; set; }

    [JsonPropertyName("isCompanyCar")]
    public bool IsCompanyCar { get; set; }

    [JsonPropertyName("radio")]
    public string Radio { get; set; }

    [JsonPropertyName("vin")]
    public string Vin { get; set; }

    [JsonPropertyName("company")]
    public string Company { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("tcuType")]
    public string TcuType { get; set; }

    [JsonPropertyName("make")]
    public string Make { get; set; }

    [JsonPropertyName("brandCode")]
    public string BrandCode { get; set; }

    [JsonPropertyName("soldRegion")]
    public string SoldRegion { get; set; }

    [JsonPropertyName("svla")]
    public VehicleSvla Svla { get; set; }

    [JsonPropertyName("market")]
    public string Market { get; set; }

    [JsonPropertyName("modelDescription")]
    public string ModelDescription { get; set; }

    [JsonPropertyName("fuelType")]
    public string FuelType { get; set; }

    [JsonPropertyName("tsoModelYear")]
    public string TsoModelYear { get; set; }

    [JsonPropertyName("sdp")]
    public string Sdp { get; set; }

    [JsonPropertyName("subMake")]
    public string SubMake { get; set; }

    [JsonPropertyName("regStatus")]
    public string RegStatus { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("customerRegStatus")]
    public string CustomerRegStatus { get; set; }

    [JsonPropertyName("activationSource")]
    public string ActivationSource { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("regTimestamp")]
    public long RegTimestamp { get; set; }

    [JsonPropertyName("services")]
    public List<VehicleService> Services { get; set; }

    [JsonPropertyName("enrollmentStatus")]
    public string EnrollmentStatus { get; set; }

    [JsonPropertyName("channelFeatures")]
    public List<object> ChannelFeatures { get; set; }

    [JsonPropertyName("privacyMode")]
    public string PrivacyMode { get; set; }

    [JsonPropertyName("tc")]
    public Dictionary<string, VehicleTC> TermsAndConditions { get; set; }

    [JsonPropertyName("cslProvider")]
    public string CslProvider { get; set; }
}