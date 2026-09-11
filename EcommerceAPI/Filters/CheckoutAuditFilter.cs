using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace EcommerceAPI.Filters
{
    public class CheckoutAuditFilter:IAsyncActionFilter
    {
        private readonly ILogger<CheckoutAuditFilter> _logger;
        public CheckoutAuditFilter(ILogger<CheckoutAuditFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Exception is null && executedContext.Result is IStatusCodeActionResult statusCodeActionResult)
            {
                var statusCode = statusCodeActionResult.StatusCode;
                if(statusCode is not (>= 200 and < 300))
                {
                    return;
                }

                var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var traceId = context.HttpContext.TraceIdentifier;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning(
                        "Checkout succeeded but UserId claim was missing. TraceId: {TraceId}",
                        traceId);

                    return;
                }

                WriteAudit(userId, traceId);
            }
        }
        private void WriteAudit(string userId, string traceId)
        {
            _logger.LogInformation(
                "User {UserId} completed checkout. TraceId: {TraceId}", 
                userId, 
                traceId);
        }
    }
}
