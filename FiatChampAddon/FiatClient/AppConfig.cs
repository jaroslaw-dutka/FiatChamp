using System.ComponentModel.DataAnnotations;

namespace FiatChamp;

public record HaConfig
{

}

public record AppConfig
{
    [Range(1, 1440)] 
  public int RefreshInterval { get; set; } = 15;

  public string CarUnknownLocation { get; set; } = "away";

  [Required(AllowEmptyStrings = false)]
  public string SupervisorToken { get; set; } = null!;

  public string HomeAssistantUrl { get; set; } = "http://supervisor/core";

  public int StartDelaySeconds { get; set; } = 1; 

  public bool AutoRefreshLocation { get; set; } = false;

  public bool AutoRefreshBattery { get; set; } = false;

  public bool EnableDangerousCommands { get; set; } = false;

  public bool ConvertKmToMiles { get; set; } = false;

  public bool DevMode { get; set; } = false;

  public bool UseFakeApi { get; set; } = false;

  public bool Debug { get; set; } = false;
}