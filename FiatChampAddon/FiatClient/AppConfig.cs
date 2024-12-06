namespace FiatChamp;

public record AppConfig
{
    public int RefreshInterval { get; set; } = 15;

    public string CarUnknownLocation { get; set; } = "away";

    public int StartDelaySeconds { get; set; } = 1;

    public bool AutoRefreshLocation { get; set; } = false;

    public bool AutoRefreshBattery { get; set; } = false;

    public bool EnableDangerousCommands { get; set; } = false;

    public bool ConvertKmToMiles { get; set; } = false;

    public bool DevMode { get; set; } = false;

    public bool UseFakeApi { get; set; } = false;

    public bool Debug { get; set; } = false;
}