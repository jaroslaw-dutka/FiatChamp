public class FiatCommand
{
    public bool IsDangerous { get; set; }
    public required string Message { get; init; }
    public string Action { get; init; } = "remote";
}