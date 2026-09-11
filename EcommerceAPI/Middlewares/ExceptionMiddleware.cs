using EcommerceAPI.DTOs.Checkout;
using EcommerceAPI.DTOs.Error;
using EcommerceAPI.Exceptions;

namespace EcommerceAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CheckoutValidationException ex)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                await HandleExceptionAsync(
                    context,
                    ex.StatusCode,
                    ex.Message,
                    ex.Issues);
            }
            catch (ApiException ex)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }
                await HandleExceptionAsync(
                    context, 
                    ex.StatusCode, 
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unhandled exception occurred. TraceId: {TraceId}",
                    context.TraceIdentifier);

                if (context.Response.HasStarted)
                {
                    throw;
                }

                await HandleExceptionAsync(
                    context, 
                    StatusCodes.Status500InternalServerError, 
                    "Unexpected server error.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message, List<CheckoutIssueDto>? issues=null)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                StatusCode = statusCode,
                Message = message,
                TraceId = context.TraceIdentifier,
                Issues = issues
            });
        }
    }
}