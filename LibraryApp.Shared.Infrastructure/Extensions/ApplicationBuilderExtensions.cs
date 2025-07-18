using LibraryApp.Shared.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LibraryApp.Shared.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            
            return app;
        }
    }
}