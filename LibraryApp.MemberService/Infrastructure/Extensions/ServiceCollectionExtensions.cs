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
            services.AddDbContext<MemberDbContext>(options =>
                options.UseInMemoryDatabase("MemberServiceDb"));

            // Add repositories
            services.AddScoped<IMemberRepository, MemberRepository>();

            // Add business services
            services.AddScoped<IMemberService, Services.MemberService>();

            // Add AutoMapper
            services.AddAutoMapper(typeof(MemberMappingProfile));

            // Add HTTP clients with Polly policies
            AddHttpClients(services, configuration);

            // Add health checks
            services.AddHealthChecks()
                .AddCheck<MemberServiceHealthCheck>("memberservice")
                .AddCheck<DatabaseHealthCheck>("database")
                .AddCheck<BookServiceHealthCheck>("bookservice");

            return services;
        }

        private static void AddHttpClients(IServiceCollection services, IConfiguration configuration)
        {
            var bookServiceUrl = configuration.GetValue<string>("ExternalServices:BookService:BaseUrl") 
                ?? "http://localhost:5002";

            services.AddHttpClient<IBookServiceClient, BookServiceClient>(client =>
            {
                client.BaseAddress = new Uri(bookServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "MemberService/1.0");
            });

            // Register HttpClient for BookService health checks
            services.AddHttpClient<BookServiceHealthCheck>(client =>
            {
                client.BaseAddress = new Uri(bookServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        }
    }
}