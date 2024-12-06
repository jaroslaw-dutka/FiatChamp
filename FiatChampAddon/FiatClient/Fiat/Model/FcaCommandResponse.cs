public class FcaCommandResponse
{
    public string Command { get; set; }
    public Guid CorrelationId { get; set; }
    public string ResponseStatus { get; set; }
    public long StatusTimestamp { get; set; }
    public long AsyncRespTimeout { get; set; }
}