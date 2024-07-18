namespace OxylabsPoC.Models
{
    // when we get enterprise
    public class ProxyList
    {
        public string Uuid { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public DateTime UpdatedTimestamp { get; set; }
        public int IpsCount { get; set; }
        public string Href { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;  // Add this line
    }
}