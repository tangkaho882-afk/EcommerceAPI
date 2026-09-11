using EcommerceAPI.Exceptions;
using System.Security.Claims;

namespace EcommerceAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetRequiredUserId(this ClaimsPrincipal user)
        {
            var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out int userId))
            {
                throw new InvalidUserClaimException("Authenticated user does not have a valid UserId claim.");
            }

            return userId;
        }
    }
}