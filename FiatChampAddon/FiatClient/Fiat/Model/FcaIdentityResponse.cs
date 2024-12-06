public class FcaIdentityResponse : FiatBaseResponse
{
    public string IdentityId { get; set; }
    public string Token { get; set; }

    public override bool CheckForError()
    {
        return string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(IdentityId);
    }

    public override void ThrowOnError(string message)
    {
        if (CheckForError())
        {
            throw new Exception(message);
        }
    }
}