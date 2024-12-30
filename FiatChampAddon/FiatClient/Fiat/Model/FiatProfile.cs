using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatProfile
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("birthDay")]
    public int BirthDay { get; set; }

    [JsonPropertyName("birthMonth")]
    public int BirthMonth { get; set; }

    [JsonPropertyName("birthYear")]
    public int BirthYear { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}