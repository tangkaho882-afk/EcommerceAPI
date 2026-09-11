using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.DTOs.Order.V1
{
    public class OrderItemDetailsResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        [Column(TypeName="decimal(18,2")]
        public decimal SubTotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
