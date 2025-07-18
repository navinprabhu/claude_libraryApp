using Microsoft.AspNetCore.Http;

namespace LibraryApp.Shared.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrGenerateCorrelationId(context);
            
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            await _next(context);
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                return correlationId.ToString();
            }

            return Guid.NewGuid().ToString();
        }
    }
}