public class Battery
{
    public long StateOfCharge { get; set; }
    public string ChargingLevel { get; set; }
    public bool PlugInStatus { get; set; }
    public long TimeToFullyChargeL3 { get; set; }
    public long TimeToFullyChargeL2 { get; set; }
    public string ChargingStatus { get; set; }
    public long TotalRange { get; set; }
    public DistanceToEmpty DistanceToEmpty { get; set; }
}