using FiatChamp.Fiat.Model;

namespace FiatChamp.Fiat;

public record FiatConfig
{
    public FcaBrand Brand { get; set; }

    public FcaRegion Region { get; set; }

    public string User { get; set; };

    public string Password { get; set; };

    public string? Pin { get; set; }
}