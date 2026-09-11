using EcommerceAPI.DTOs.Checkout;

namespace EcommerceAPI.Exceptions
{
    public class CheckoutValidationException:ApiException
    {
        public List<CheckoutIssueDto> Issues { get; }
        public CheckoutValidationException(List<CheckoutIssueDto> issues):
            base(StatusCodes.Status400BadRequest, "Checkout validation failed.")
        {
            Issues = issues;
        }
    }
}
