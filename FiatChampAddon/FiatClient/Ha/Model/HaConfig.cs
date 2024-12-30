using System.Text.Json.Serialization;

namespace FiatChamp.Ha.Model
{
    public class HaConfig
    {
        [JsonPropertyName("allowlist_external_dirs")]
        public List<string> AllowlistExternalDirs { get; set; }

        [JsonPropertyName("allowlist_external_urls")]
        public List<object> AllowlistExternalUrls { get; set; }

        [JsonPropertyName("components")]
        public List<string> Components { get; set; }

        [JsonPropertyName("config_dir")]
        public string ConfigDir { get; set; }

        [JsonPropertyName("config_source")]
        public string ConfigSource { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("debug")]
        public bool Debug { get; set; }

        [JsonPropertyName("elevation")]
        public int Elevation { get; set; }

        [JsonPropertyName("external_url")]
        public object ExternalUrl { get; set; }

        [JsonPropertyName("internal_url")]
        public object InternalUrl { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("location_name")]
        public string LocationName { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("radius")]
        public int Radius { get; set; }

        [JsonPropertyName("recovery_mode")]
        public bool RecoveryMode { get; set; }

        [JsonPropertyName("safe_mode")]
        public bool SafeMode { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("time_zone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("unit_system")]
        public HaUnitSystem UnitSystem { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("whitelist_external_dirs")]
        public List<string> WhitelistExternalDirs { get; set; }
    }
}
