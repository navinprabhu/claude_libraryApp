using AspNetCoreRateLimit;

namespace LibraryApp.ApiGateway.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (ex.Message.Contains("rate limit"))
            {
                _logger.LogWarning("Rate limit exceeded for {CorrelationId} from {ClientIp}", 
                    correlationId, GetClientIpAddress(context));
                
                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Rate limit exceeded",
                    message = "Too many requests. Please try again later.",
                    correlationId = correlationId,
                    retryAfter = "60"
                };
                
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                return;
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "unknown";
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}