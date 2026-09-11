
namespace EcommerceAPI.DTOs.Order.V2
{
    public class OrderDetailResponseDto
    {
        public int OrderId { get; set; }
        public MoneyDto TotalAmount { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDetailsResponseDto> OrderItems { get; set; } = new();
    }
}
