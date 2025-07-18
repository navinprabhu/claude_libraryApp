using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LibraryApp.Shared.Infrastructure.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";

            _logger.LogInformation(
                "Request started: {Method} {Path} - CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }
    }
}