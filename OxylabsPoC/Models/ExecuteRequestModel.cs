using System.ComponentModel.DataAnnotations;

namespace OxylabsPoC.Models
{
    public class ExecuteRequestModel
    {
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(GET|POST|PUT|DELETE|PATCH)$", ErrorMessage = "Invalid HTTP method")]
        public string Method { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AccountId must be a positive integer")]
        public int AccountId { get; set; }
    }
}