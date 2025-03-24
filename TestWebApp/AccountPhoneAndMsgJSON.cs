using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TestWebApp
{
    public class AccountPhoneAndMsgJSON
    {
        [Required]
        [JsonPropertyName("AccountID")]
        public string? AccountID { get; set; }

        [Required]
        [JsonPropertyName("PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [Required]
        [JsonPropertyName("Message")]
        public string? Message { get; set; }
    }
}
