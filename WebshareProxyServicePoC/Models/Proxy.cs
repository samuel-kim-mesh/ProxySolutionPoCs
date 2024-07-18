using System.Text.Json.Serialization;

namespace WebshareProxyServicePoC.Models
{
    public class Proxy
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [JsonPropertyName("proxy_address")]
        public string ProxyAddress { get; set; } = string.Empty;
        public int Port { get; set; }
        public bool Valid { get; set; }
        [JsonPropertyName("last_verification")]
        public DateTime LastVerification { get; set; }
        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;
        [JsonPropertyName("city_name")]
        public string CityName { get; set; } = string.Empty;
        [JsonPropertyName("asn_name")]
        public string AsnName { get; set; } = string.Empty;
        [JsonPropertyName("asn_number")]
        public int AsnNumber { get; set; }
        [JsonPropertyName("high_country_confidence")]
        public bool HighCountryConfidence { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}