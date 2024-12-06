using FiatChamp.Fiat.Model;

namespace FiatChamp.Fiat;

public record FiatConfig
{
    public FcaBrand Brand { get; set; }

    public FcaRegion Region { get; set; } = FcaRegion.Europe;

    public string User { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Pin { get; set; }
}