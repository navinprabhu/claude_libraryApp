using LibraryApp.Shared.Models.Common;
using System.Net;
using System.Text.Json;

namespace LibraryApp.MemberService.Infrastructure.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var apiResponse = exception switch
            {
                ArgumentException => ApiResponse<object>.ErrorResponse(exception.Message, (int)HttpStatusCode.BadRequest),
                KeyNotFoundException => ApiResponse<object>.ErrorResponse("Resource not found", (int)HttpStatusCode.NotFound),
                UnauthorizedAccessException => ApiResponse<object>.ErrorResponse("Unauthorized access", (int)HttpStatusCode.Unauthorized),
                InvalidOperationException => ApiResponse<object>.ErrorResponse(exception.Message, (int)HttpStatusCode.BadRequest),
                HttpRequestException => ApiResponse<object>.ErrorResponse("External service unavailable", (int)HttpStatusCode.ServiceUnavailable),
                TaskCanceledException => ApiResponse<object>.ErrorResponse("Request timeout", (int)HttpStatusCode.RequestTimeout),
                _ => ApiResponse<object>.ErrorResponse("An error occurred while processing your request", (int)HttpStatusCode.InternalServerError)
            };

            response.StatusCode = apiResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(apiResponse, options);
            await response.WriteAsync(jsonResponse);
        }
    }

    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}