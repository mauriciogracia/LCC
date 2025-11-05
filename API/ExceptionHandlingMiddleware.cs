using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace API
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILog _log;

        public ExceptionHandlingMiddleware(ILog log)
        {
            _log = log;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _log.error($"Unhandled exception: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}
