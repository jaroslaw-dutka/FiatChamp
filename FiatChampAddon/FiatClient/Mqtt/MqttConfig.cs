using System.ComponentModel.DataAnnotations;

namespace FiatChamp.Mqtt;

public class MqttConfig
{
    [Required(AllowEmptyStrings = false)]
    public string Server { get; set; } = null!;

    [Range(1, 65536)]
    public int Port { get; set; } = 1883;

    public string User { get; set; } = "";

    public string Password { get; set; } = "";

    public bool UseTls { get; set; } = false;
}