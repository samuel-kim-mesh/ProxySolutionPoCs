namespace OxylabsPoC.Models
{
    public class ExecuteRequestResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Content { get; set; } = string.Empty;
    }
}