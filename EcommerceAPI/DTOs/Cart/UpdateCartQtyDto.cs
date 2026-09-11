using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs.Cart
{
    public class UpdateCartQtyDto
    {
        [Range(1,int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
