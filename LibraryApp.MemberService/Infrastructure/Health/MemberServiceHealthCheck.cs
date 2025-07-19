using LibraryApp.MemberService.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.MemberService.Infrastructure.Health
{
    public class MemberServiceHealthCheck : IHealthCheck
    {
        private readonly MemberDbContext _dbContext;
        private readonly ILogger<MemberServiceHealthCheck> _logger;

        public MemberServiceHealthCheck(MemberDbContext dbContext, ILogger<MemberServiceHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    ["timestamp"] = DateTimeOffset.UtcNow,
                    ["service"] = "LibraryApp.MemberService"
                };

                await _dbContext.Database.CanConnectAsync(cancellationToken);
                data["database"] = "Connected";

                var memberCount = await _dbContext.Members.CountAsync(cancellationToken);
                data["total_members"] = memberCount;

                var activeMembersCount = await _dbContext.Members.CountAsync(m => m.IsActive, cancellationToken);
                data["active_members"] = activeMembersCount;

                return HealthCheckResult.Healthy("Member service is healthy", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return HealthCheckResult.Unhealthy("Member service is unhealthy", ex);
            }
        }
    }

    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly MemberDbContext _dbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(MemberDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                var data = new Dictionary<string, object>
                {
                    ["timestamp"] = DateTimeOffset.UtcNow,
                    ["database_type"] = "InMemory",
                    ["connection_status"] = "Connected"
                };

                return HealthCheckResult.Healthy("Database is healthy", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
            }
        }
    }

    public class BookServiceHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookServiceHealthCheck> _logger;

        public BookServiceHealthCheck(HttpClient httpClient, ILogger<BookServiceHealthCheck> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("/health", cancellationToken);
                
                var data = new Dictionary<string, object>
                {
                    ["timestamp"] = DateTimeOffset.UtcNow,
                    ["service"] = "BookService",
                    ["status_code"] = (int)response.StatusCode
                };

                if (response.IsSuccessStatusCode)
                {
                    data["status"] = "Healthy";
                    return HealthCheckResult.Healthy("BookService is healthy", data);
                }
                else
                {
                    data["status"] = "Unhealthy";
                    return HealthCheckResult.Unhealthy("BookService is unhealthy", null, data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BookService health check failed");
                return HealthCheckResult.Unhealthy("BookService is unavailable", ex);
            }
        }
    }
}