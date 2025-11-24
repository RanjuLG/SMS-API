using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using Serilog;

namespace SMS.Middleware
{
    /// <summary>
    /// Global error handling middleware that catches all unhandled exceptions
    /// and logs them using Serilog
    /// </summary>
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception with Serilog
            Log.Error(exception, "An unhandled exception occurred while processing request {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            // Also log additional context information
            _logger.LogError(exception,
                "Error processing {Method} {Path}. User: {User}, IP: {IP}",
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous",
                context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Prepare error response
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Message = "An error occurred while processing your request.",
                Details = exception.Message
            };

            switch (exception)
            {
                case ApplicationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = exception.Message;
                    break;
                
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = "Resource not found.";
                    break;
                
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "Unauthorized access.";
                    break;
                
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = exception.Message;
                    break;
                
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "An internal server error occurred.";
                    // Don't expose internal error details in production
                    if (context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false)
                    {
                        errorResponse.Details = exception.ToString();
                    }
                    else
                    {
                        errorResponse.Details = "Please contact support if the problem persists.";
                    }
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse);
            await response.WriteAsync(result);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
