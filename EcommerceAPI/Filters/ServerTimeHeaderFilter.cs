using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EcommerceAPI.Filters
{
    public class ServerTimeHeaderFilter:IResultFilter
    {
        
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var statusCode = context.Result switch
            {
                IStatusCodeActionResult result when result.StatusCode.HasValue => result.StatusCode.Value,

                OkResult or OkObjectResult => StatusCodes.Status200OK,

                _ => (int?)null
            };


            if (statusCode is (>= 200 and < 300))
            {
                context.HttpContext.Response.Headers["X-Server-Time"] = DateTime.UtcNow.ToString("o");
                //context.HttpContext.Response.Headers.Add("X-Server-Time", DateTime.UtcNow.ToString("o"));
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}
