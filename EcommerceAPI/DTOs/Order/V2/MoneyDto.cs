namespace EcommerceAPI.DTOs.Order.V2
{
    public class MoneyDto
    {
        public decimal Value { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
