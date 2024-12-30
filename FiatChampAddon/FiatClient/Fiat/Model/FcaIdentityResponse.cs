using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FcaIdentityResponse : IFiatResponse
{
    [JsonPropertyName("IdentityId")]
    public string IdentityId { get; set; }

    [JsonPropertyName("Token")]
    public string Token { get; set; }

    public bool CheckForError() => string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(IdentityId);

    public void ThrowOnError(string message)
    {
        if (CheckForError())
            throw new Exception(message);
    }
}