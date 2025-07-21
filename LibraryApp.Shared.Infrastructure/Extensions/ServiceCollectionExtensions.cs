using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Infrastructure.Middleware;
using LibraryApp.Shared.Infrastructure.Repositories;
using LibraryApp.Shared.Infrastructure.Services;
using LibraryApp.Shared.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace LibraryApp.Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Note: BaseRepository<T> is abstract and should not be registered directly.
            // Each service should register their own concrete repository implementations.
            // Example: services.AddScoped<IUserRepository, UserRepository>();

            // Add correlation ID service
            services.AddSingleton<ICorrelationIdService, CorrelationIdService>();

            // Add event publisher
            services.AddSingleton<IEventPublisher, InMemoryEventPublisher>();

            // Add telemetry and metrics
            services.AddSingleton<ServiceCallMetrics>();
            services.AddSingleton<IServiceCallMetrics, ServiceCallMetricsService>();

            // Configure service client options
            services.Configure<ServiceClientOptions>(configuration.GetSection(ServiceClientOptions.SectionName));

            return services;
        }

        public static IServiceCollection AddServiceClients(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceClientOptions = new ServiceClientOptions();
            configuration.GetSection(ServiceClientOptions.SectionName).Bind(serviceClientOptions);

            // Configure HTTP clients with Polly retry policies
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    serviceClientOptions.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Log retry attempts - can be extended with proper logger injection
                        Console.WriteLine(
                            "Retry {0} for {1} after {2}ms due to: {3}",
                            retryCount,
                            context.OperationKey,
                            timespan.TotalMilliseconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    serviceClientOptions.CircuitBreakerFailureThreshold,
                    TimeSpan.FromSeconds(serviceClientOptions.CircuitBreakerDurationSeconds),
                    onBreak: (result, duration) =>
                    {
                        // Log circuit breaker opened
                    },
                    onReset: () =>
                    {
                        // Log circuit breaker closed
                    });

            // Register Book Service Client
            services.AddHttpClient<IBookServiceClient, BookServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceClientOptions.BookServiceBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(serviceClientOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "LibraryApp-ServiceClient/1.0");
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

            // Register Member Service Client
            services.AddHttpClient<IMemberServiceClient, MemberServiceClient>(client =>
            {
                client.BaseAddress = new Uri(serviceClientOptions.MemberServiceBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(serviceClientOptions.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("User-Agent", "LibraryApp-ServiceClient/1.0");
            })
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

            return services;
        }

        public static IServiceCollection AddCustomCors(this IServiceCollection services, string policyName = "DefaultCorsPolicy")
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}