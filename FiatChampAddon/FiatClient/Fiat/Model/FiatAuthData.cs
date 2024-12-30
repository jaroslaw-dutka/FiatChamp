using System.Text.Json.Serialization;

namespace FiatChamp.Fiat.Model
{
    public class FiatAuthData
    {
        [JsonPropertyName("privacyFlag")]
        public string PrivacyFlag { get; set; }

        [JsonPropertyName("CVSID")]
        public Dictionary<string, string> CVSID { get; set; }

        [JsonPropertyName("fcaId")]
        public string FcaId { get; set; }

        [JsonPropertyName("GSDPisEmailVerified")]
        public bool GSDPisEmailVerified { get; set; }

        [JsonPropertyName("eventDone")]
        public bool EventDone { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("disclaimerCodeGSDP")]
        public string DisclaimerCodeGSDP { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        [JsonPropertyName("verificationDone")]
        public bool VerificationDone { get; set; }

        [JsonPropertyName("CustomerFirstID")]
        public string CustomerFirstID { get; set; }

        [JsonPropertyName("applications")]
        public Dictionary<string, bool> Applications { get; set; }

        [JsonPropertyName("marketUser")]
        public string MarketUser { get; set; }
    }
}
