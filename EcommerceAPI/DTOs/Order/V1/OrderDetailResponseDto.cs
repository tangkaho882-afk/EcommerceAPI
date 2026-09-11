
namespace EcommerceAPI.DTOs.Order.V1
{
    public class OrderDetailResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDetailsResponseDto> OrderItems { get; set; } = new();
    }
}
