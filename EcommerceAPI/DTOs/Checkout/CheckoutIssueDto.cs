namespace EcommerceAPI.DTOs.Checkout
{
    public class CheckoutIssueDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
