namespace FiatChamp;

public record AppConfig
{
    public bool Debug { get; set; }

    public bool FakeApi { get; set; }
    
    public int RefreshInterval { get; set; }

    public string CarUnknownLocation { get; set; }

    public int StartDelaySeconds { get; set; }

    public bool AutoRefreshLocation { get; set; }

    public bool AutoRefreshBattery { get; set; }

    public bool EnableDangerousCommands { get; set; }

    public bool ConvertKmToMiles { get; set; }
}