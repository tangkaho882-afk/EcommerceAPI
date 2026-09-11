using EcommerceAPI.DTOs.Checkout;
using System.Text.Json.Serialization;

namespace EcommerceAPI.DTOs.Error
{
    public class ErrorResponseDto
    {
        public int StatusCode { get; init; }
        public string Message { get; init; } = string.Empty;
        public string TraceId { get; init; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; init; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CheckoutIssueDto>? Issues { get; init; }
    }
}
