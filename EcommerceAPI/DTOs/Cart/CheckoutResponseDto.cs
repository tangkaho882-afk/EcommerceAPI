using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.DTOs.Cart
{
    public class CheckoutResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int OrderId { get; set;}
        public decimal TotalAmount { get; set; }
    }
}
