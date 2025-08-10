using LibraryApp.MemberService.Data;
using LibraryApp.MemberService.Data.Repositories;
using LibraryApp.MemberService.Infrastructure.Health;
using LibraryApp.MemberService.Infrastructure.Mapping;
using LibraryApp.MemberService.Services;
using LibraryApp.MemberService.Services.External;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.MemberService.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMemberServiceDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDbContext<MemberDbContext>(options =>
                    options.UseNpgsql(connectionString));
            }
            else
            {
                // Fallback to InMemory for development/testing
                services.AddDbContext<MemberDbContext>(options =>
                    options.UseInMemoryDatabase("MemberServiceDb"));
            }

            // Add repositories
            services.AddScoped<IMemberRepository, MemberRepository>();

            // Add business services
            services.AddScoped<IMemberService, Services.MemberService>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(MemberMappingProfile));

            // Add HTTP clients with Polly policies
            AddHttpClients(services, configuration);

            // Add health checks (removed external service dependency to avoid circular health check issues)
            services.AddHealthChecks()
                .AddCheck<MemberServiceHealthCheck>("memberservice")
                .AddCheck<DatabaseHealthCheck>("database");

            return services;
        }

        private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            var bookServiceUrl = configuration.GetValue<string>("ServiceUrls:BookService") 
                ?? configuration.GetValue<string>("ExternalServices:BookService:BaseUrl") 
                ?? "http://localhost:5002";

            services.AddHttpClient<IBookServiceClient, BookServiceClient>(client =>
            {
                client.BaseAddress = new Uri(bookServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MemberService/1.0");
            });
        }
    }
}