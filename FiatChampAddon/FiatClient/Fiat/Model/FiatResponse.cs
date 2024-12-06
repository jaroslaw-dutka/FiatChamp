public class FiatResponse
{
  public string CallId { get; set; }
  public long ErrorCode { get; set; }
  public string ErrorDetails { get; set; }
  public string ErrorMessage { get; set; }
  public long ApiVersion { get; set; }
  public long StatusCode { get; set; }
  public string StatusReason { get; set; }
  public DateTimeOffset Time { get; set; }

  public bool CheckForError()
  {
    return StatusCode != 200;
  }

  public void ThrowOnError(string message)
  {
    if (CheckForError())
    {
      throw new Exception(message + $" {this.ErrorCode} {this.StatusReason} {this.ErrorMessage}");
    }
  }
}