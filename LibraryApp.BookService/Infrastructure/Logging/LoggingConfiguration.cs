using Serilog;
using Serilog.Events;

namespace LibraryApp.BookService.Infrastructure.Logging
{
    public static class LoggingConfiguration
    {
        public static void ConfigureSerilog(IConfiguration configuration, IHostEnvironment environment)
        {
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "LibraryApp.BookService")
                .Enrich.WithProperty("Environment", environment.EnvironmentName);

            if (environment.IsDevelopment())
            {
                loggerConfig
                    .MinimumLevel.Debug()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }
            else
            {
                loggerConfig
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggerConfig.WriteTo.Console();
            }

            Log.Logger = loggerConfig.CreateLogger();
        }

        public static void AddCorrelationIdLogging(this IServiceCollection services)
        {
            // No need to register middleware as a service - it's added in pipeline
        }
    }

    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);
            
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method
            }))
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                await _next(context);
            }
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault();
            
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            return correlationId;
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