using System.ComponentModel.DataAnnotations;

namespace FiatChamp.Mqtt;

public class MqttConfig
{
    public string Server { get; set; }

    public int Port { get; set; }
    
    public bool UseTls { get; set; }
    
    public string ClientId { get; set; }

    public string User { get; set; }

    public string Password { get; set; }
}