using System.Text.Json.Serialization;

namespace WebshareProxyServicePoC.Models
{
    public class ProxyReplacementRequest
    {
        [JsonPropertyName("to_replace")]
        public ToReplace ToReplace { get; set; } = new ToReplace();

        [JsonPropertyName("replace_with")]
        public List<ReplaceWith> ReplaceWith { get; set; } = new List<ReplaceWith>();

        [JsonPropertyName("dry_run")]
        public bool DryRun { get; set; }
    }

    public class ToReplace
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

        [JsonPropertyName("ip_ranges")]
        public List<string> IpRanges { get; set; } = new List<string>();
    }

    public class ReplaceWith
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;

        [JsonPropertyName("country_code")] public string CountryCode { get; set; } = string.Empty;
    }

    public class ProxyReplacementResponse
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("to_replace")]
        public ToReplace ToReplace { get; set; } = new ToReplace();

        [JsonPropertyName("replace_with")]
        public List<ReplaceWith> ReplaceWith { get; set; } = new List<ReplaceWith>();

        [JsonPropertyName("dry_run")]
        public bool DryRun { get; set; }
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("proxies_removed")]
        public int? ProxiesRemoved { get; set; }

        [JsonPropertyName("proxies_added")]
        public int? ProxiesAdded { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }
}