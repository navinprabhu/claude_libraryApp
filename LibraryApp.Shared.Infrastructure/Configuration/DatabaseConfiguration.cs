using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.Shared.Infrastructure.Configuration
{
    public static class DatabaseConfiguration
    {
        public static IServiceCollection AddDatabaseConfiguration<TContext>(
            this IServiceCollection services, 
            IConfiguration configuration,
            string connectionStringName = "DefaultConnection") 
            where TContext : DbContext
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            
            services.AddDbContext<TContext>(options =>
            {
                options.UseSqlServer(connectionString);
                
                // Add logging in development
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            });

            return services;
        }

        public static IServiceCollection AddDatabaseHealthCheck<TContext>(
            this IServiceCollection services,
            string healthCheckName = "database")
            where TContext : DbContext
        {
            services.AddHealthChecks()
                .AddDbContextCheck<TContext>(healthCheckName);

            return services;
        }
    }
}