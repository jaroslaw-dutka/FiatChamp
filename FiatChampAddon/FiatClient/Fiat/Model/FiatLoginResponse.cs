using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model;

public class FiatLoginResponse : FiatResponse
{
    [JsonPropertyName("registeredTimestamp")]
    public int RegisteredTimestamp { get; set; }

    [JsonPropertyName("UID")]
    public string UID { get; set; }

    [JsonPropertyName("UIDSignature")]
    public string UIDSignature { get; set; }

    [JsonPropertyName("signatureTimestamp")]
    public string SignatureTimestamp { get; set; }

    [JsonPropertyName("created")]
    public DateTime Created { get; set; }

    [JsonPropertyName("createdTimestamp")]
    public int CreatedTimestamp { get; set; }

    [JsonPropertyName("data")]
    public FiatAuthData Data { get; set; }

    [JsonPropertyName("subscriptions")]
    public FiatSubscriptions Subscriptions { get; set; }

    [JsonPropertyName("preferences")]
    public Dictionary<string, FiatPreferences> Preferences { get; set; }

    [JsonPropertyName("emails")]
    public FiatEmails Emails { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("isRegistered")]
    public bool IsRegistered { get; set; }

    [JsonPropertyName("isVerified")]
    public bool IsVerified { get; set; }

    [JsonPropertyName("lastLogin")]
    public DateTime LastLogin { get; set; }

    [JsonPropertyName("lastLoginTimestamp")]
    public int LastLoginTimestamp { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }

    [JsonPropertyName("lastUpdatedTimestamp")]
    public long LastUpdatedTimestamp { get; set; }

    [JsonPropertyName("loginProvider")]
    public string LoginProvider { get; set; }

    [JsonPropertyName("oldestDataUpdated")]
    public DateTime OldestDataUpdated { get; set; }

    [JsonPropertyName("oldestDataUpdatedTimestamp")]
    public long OldestDataUpdatedTimestamp { get; set; }

    [JsonPropertyName("profile")]
    public FiatProfile Profile { get; set; }

    [JsonPropertyName("registered")]
    public DateTime Registered { get; set; }

    [JsonPropertyName("socialProviders")]
    public string SocialProviders { get; set; }

    [JsonPropertyName("verified")]
    public DateTime Verified { get; set; }

    [JsonPropertyName("verifiedTimestamp")]
    public long VerifiedTimestamp { get; set; }

    [JsonPropertyName("newUser")]
    public bool NewUser { get; set; }

    [JsonPropertyName("sessionInfo")]
    public FiatSessionInfo SessionInfo { get; set; }
}