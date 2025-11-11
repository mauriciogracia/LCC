using System.Text.Json;
using Domain.Interfaces;

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
                _log.error($"Unhandled exception: {ex}");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                object errorResponse;

                /*
                if (_env.IsDevelopment())
                {
                */
                    // Detailed info for dev/debug
                    errorResponse = new
                    {
                        Success = false,
                        Error = new
                        {
                            Message = "An unexpected error occurred.",
                            Detail = ex.Message,
                            Type = ex.GetType().Name,
                            Path = context.Request.Path,
                            TraceId = context.TraceIdentifier,
                            StackTrace = ex.StackTrace
                        }
                    };
                /*
                }
                else
                {
                
                    // Minimal info for production
                    errorResponse = new
                    {
                        Success = false,
                        Error = new
                        {
                            Message = "An unexpected error occurred. Please contact support.",
                            TraceId = context.TraceIdentifier
                        }
                    };
                }
                */

                var json = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
