namespace WebshareProxyServicePoC.Models
{
    public class ProxyListResponse
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string? Previous { get; set; }
        public List<Proxy> Results { get; set; } = new List<Proxy>();
    }


}