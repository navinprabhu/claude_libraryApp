using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.Shared.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            
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