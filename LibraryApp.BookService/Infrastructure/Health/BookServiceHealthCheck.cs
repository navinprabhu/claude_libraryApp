using LibraryApp.BookService.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.BookService.Infrastructure.Health
{
    public class BookServiceHealthCheck : IHealthCheck
    {
        private readonly BookDbContext _dbContext;
        private readonly ILogger<BookServiceHealthCheck> _logger;

        public BookServiceHealthCheck(BookDbContext dbContext, ILogger<BookServiceHealthCheck> logger)
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
                    ["service"] = "LibraryApp.BookService"
                };

                await _dbContext.Database.CanConnectAsync(cancellationToken);
                data["database"] = "Connected";

                var bookCount = await _dbContext.Books.CountAsync(cancellationToken);
                data["total_books"] = bookCount;

                var availableBooksCount = await _dbContext.Books.CountAsync(b => b.AvailableCopies > 0, cancellationToken);
                data["available_books"] = availableBooksCount;

                var activeBorrowingsCount = await _dbContext.BorrowingRecords.CountAsync(br => !br.IsReturned, cancellationToken);
                data["active_borrowings"] = activeBorrowingsCount;

                return HealthCheckResult.Healthy("Book service is healthy", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return HealthCheckResult.Unhealthy("Book service is unhealthy", ex);
            }
        }
    }

    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly BookDbContext _dbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(BookDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
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
}