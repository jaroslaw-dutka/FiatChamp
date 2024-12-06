public class VehicleLocation
{
    public long TimeStamp { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double? Altitude { get; set; }
    public object? Bearing { get; set; }
    public bool? IsLocationApprox { get; set; }
}