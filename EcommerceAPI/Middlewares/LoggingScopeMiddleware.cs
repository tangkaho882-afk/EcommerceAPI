using System.Security.Claims;

namespace EcommerceAPI.Middlewares
{
    public class LoggingScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingScopeMiddleware> _logger;

        public LoggingScopeMiddleware(
            RequestDelegate next,
            ILogger<LoggingScopeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (_logger.BeginScope("RequestScope RequestId: {RequestId}, UserId: {UserId}",
                context.TraceIdentifier,
                userId))
            {
                await _next(context);
            }
        }
    }
}