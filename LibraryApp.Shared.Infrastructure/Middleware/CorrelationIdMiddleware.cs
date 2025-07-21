using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LibraryApp.Shared.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to handle correlation ID tracking across service calls
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get or generate correlation ID
            var correlationId = GetOrGenerateCorrelationId(context);

            // Add correlation ID to response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            // Add correlation ID to logging scope
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            });

            // Store correlation ID in HttpContext for access by other middleware/services
            context.Items["CorrelationId"] = correlationId;

            _logger.LogDebug(
                "Processing request {Method} {Path} with correlation ID {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            try
            {
                await _next(context);

                _logger.LogDebug(
                    "Completed request {Method} {Path} with correlation ID {CorrelationId} - Status: {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing request {Method} {Path} with correlation ID {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    correlationId);
                throw;
            }
        }

        private string GetOrGenerateCorrelationId(HttpContext context)
        {
            // Check if correlation ID is provided in request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdHeader) &&
                !string.IsNullOrEmpty(correlationIdHeader.FirstOrDefault()))
            {
                var existingCorrelationId = correlationIdHeader.FirstOrDefault();
                _logger.LogDebug("Using existing correlation ID: {CorrelationId}", existingCorrelationId);
                return existingCorrelationId!;
            }

            // Generate new correlation ID
            var newCorrelationId = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated new correlation ID: {CorrelationId}", newCorrelationId);
            return newCorrelationId;
        }
    }

    /// <summary>
    /// Helper service to access correlation ID from the current HTTP context
    /// </summary>
    public interface ICorrelationIdService
    {
        /// <summary>
        /// Gets the current correlation ID for the request
        /// </summary>
        /// <returns>The correlation ID or null if not available</returns>
        string? GetCorrelationId();
    }

    /// <summary>
    /// Implementation of correlation ID service
    /// </summary>
    public class CorrelationIdService : ICorrelationIdService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string? GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue("CorrelationId", out var correlationId) == true)
            {
                return correlationId?.ToString();
            }

            return null;
        }
    }
}