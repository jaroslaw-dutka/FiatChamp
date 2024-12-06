namespace FiatChamp.Ha;

public record HaConfig
{
    public string Url { get; set; } = "http://supervisor/core";
    public string Token { get; set; } = null!;
}