using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatEmails
{
    [JsonPropertyName("verified")]
    public List<string> Verified { get; set; }

    [JsonPropertyName("unverified")]
    public List<string> Unverified { get; set; }
}