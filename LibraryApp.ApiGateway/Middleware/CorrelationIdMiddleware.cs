using System.Diagnostics;

namespace LibraryApp.ApiGateway.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);
            
            // Set correlation ID in response headers
            context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
            
            // Add correlation ID to logging context
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                { "CorrelationId", correlationId },
                { "RequestPath", context.Request.Path },
                { "RequestMethod", context.Request.Method }
            }))
            {
                // Set correlation ID in Activity for distributed tracing
                Activity.Current?.SetTag("correlationId", correlationId);
                
                // Store correlation ID in HttpContext for downstream access
                context.Items["CorrelationId"] = correlationId;
                
                _logger.LogInformation("Processing request with correlation ID: {CorrelationId}", correlationId);
                
                await _next(context);
                
                _logger.LogInformation("Completed request with correlation ID: {CorrelationId}", correlationId);
            }
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check if correlation ID already exists in request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) &&
                !string.IsNullOrEmpty(correlationId))
            {
                return correlationId.ToString();
            }

            // Generate new correlation ID if not present
            return Guid.NewGuid().ToString();
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}