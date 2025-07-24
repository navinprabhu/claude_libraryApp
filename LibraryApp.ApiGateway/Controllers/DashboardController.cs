using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LibraryApp.ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DashboardController> _logger;
        private readonly IConfiguration _configuration;

        public DashboardController(IHttpClientFactory httpClientFactory, ILogger<DashboardController> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get overall dashboard statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
                _logger.LogInformation("Dashboard stats requested - CorrelationId: {CorrelationId}", correlationId);

                // Get service URLs from configuration
                var authServiceUrl = GetServiceUrl("AuthService", "http://localhost:5001");
                var bookServiceUrl = GetServiceUrl("BookService", "http://localhost:5002");
                var memberServiceUrl = GetServiceUrl("MemberService", "http://localhost:5003");

                // Create parallel tasks to fetch data from services
                var bookStatsTask = GetBookStatsAsync(bookServiceUrl, correlationId);
                var memberStatsTask = GetMemberStatsAsync(memberServiceUrl, correlationId);
                var overdueStatsTask = GetOverdueStatsAsync(bookServiceUrl, correlationId);

                // Execute all requests in parallel
                await Task.WhenAll(bookStatsTask, memberStatsTask, overdueStatsTask);

                var bookStats = await bookStatsTask;
                var memberStats = await memberStatsTask;
                var overdueStats = await overdueStatsTask;

                // Aggregate the results
                var dashboardStats = new
                {
                    totalBooks = bookStats.totalBooks,
                    availableBooks = bookStats.availableBooks,
                    booksBorrowed = bookStats.totalBooks - bookStats.availableBooks,
                    totalMembers = memberStats.totalMembers,
                    activeMembers = memberStats.activeMembers,
                    overdueBooks = overdueStats,
                    lastUpdated = DateTime.UtcNow
                };

                return Ok(new
                {
                    success = true,
                    data = dashboardStats,
                    correlationId = correlationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats");
                return StatusCode(500, new { error = "Failed to fetch dashboard statistics", message = ex.Message });
            }
        }

        /// <summary>
        /// Get recent transactions/borrowings
        /// </summary>
        [HttpGet("recent-transactions")]
        [Authorize]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 10)
        {
            try
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
                _logger.LogInformation("Recent transactions requested - CorrelationId: {CorrelationId}", correlationId);

                var bookServiceUrl = GetServiceUrl("BookService", "http://localhost:5002");
                
                // This would need to be implemented in BookService
                // For now, returning mock data structure
                var recentTransactions = new[]
                {
                    new
                    {
                        id = 1,
                        memberId = 1,
                        memberName = "John Doe",
                        bookId = 3,
                        bookTitle = "Sample Book",
                        action = "borrowed",
                        timestamp = DateTime.UtcNow.AddHours(-2)
                    },
                    new
                    {
                        id = 2,
                        memberId = 2,
                        memberName = "Jane Smith",
                        bookId = 1,
                        bookTitle = "Another Book",
                        action = "returned",
                        timestamp = DateTime.UtcNow.AddHours(-4)
                    }
                }.Take(limit);

                return Ok(new
                {
                    success = true,
                    data = recentTransactions,
                    correlationId = correlationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent transactions");
                return StatusCode(500, new { error = "Failed to fetch recent transactions", message = ex.Message });
            }
        }

        /// <summary>
        /// Get book categories with counts
        /// </summary>
        [HttpGet("book-categories")]
        [Authorize]
        public async Task<IActionResult> GetBookCategories()
        {
            try
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
                _logger.LogInformation("Book categories requested - CorrelationId: {CorrelationId}", correlationId);

                var bookServiceUrl = GetServiceUrl("BookService", "http://localhost:5002");
                
                // Get all books and group by genre
                var allBooksResponse = await _httpClient.GetAsync($"{bookServiceUrl}/api/books");
                if (!allBooksResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { error = "Failed to fetch books from book service" });
                }

                var booksContent = await allBooksResponse.Content.ReadAsStringAsync();
                var booksJson = JsonDocument.Parse(booksContent);
                
                // Mock categories for now - this would be computed from actual book data
                var categories = new[]
                {
                    new
                    {
                        category = "Fiction",
                        totalBooks = 10,
                        availableBooks = 7,
                        borrowedBooks = 3
                    },
                    new
                    {
                        category = "Non-Fiction",
                        totalBooks = 8,
                        availableBooks = 5,
                        borrowedBooks = 3
                    },
                    new
                    {
                        category = "Science",
                        totalBooks = 6,
                        availableBooks = 4,
                        borrowedBooks = 2
                    }
                };

                return Ok(new
                {
                    success = true,
                    data = categories,
                    correlationId = correlationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book categories");
                return StatusCode(500, new { error = "Failed to fetch book categories", message = ex.Message });
            }
        }

        /// <summary>
        /// Get top active members
        /// </summary>
        [HttpGet("top-members")]
        [Authorize]
        public async Task<IActionResult> GetTopMembers([FromQuery] int limit = 5)
        {
            try
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
                _logger.LogInformation("Top members requested - CorrelationId: {CorrelationId}", correlationId);

                var memberServiceUrl = GetServiceUrl("MemberService", "http://localhost:5003");
                
                // Mock top members for now - this would be computed from actual borrowing data
                var topMembers = new[]
                {
                    new
                    {
                        memberId = 1,
                        memberName = "Jane Smith",
                        email = "jane@example.com",
                        totalBorrowings = 15,
                        currentBorrowings = 2
                    },
                    new
                    {
                        memberId = 2,
                        memberName = "John Doe",
                        email = "john@example.com",
                        totalBorrowings = 12,
                        currentBorrowings = 1
                    }
                }.Take(limit);

                return Ok(new
                {
                    success = true,
                    data = topMembers,
                    correlationId = correlationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top members");
                return StatusCode(500, new { error = "Failed to fetch top members", message = ex.Message });
            }
        }

        /// <summary>
        /// Get system alerts
        /// </summary>
        [HttpGet("alerts")]
        [Authorize]
        public async Task<IActionResult> GetSystemAlerts()
        {
            try
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
                _logger.LogInformation("System alerts requested - CorrelationId: {CorrelationId}", correlationId);

                var bookServiceUrl = GetServiceUrl("BookService", "http://localhost:5002");
                
                // Get overdue books count
                var overdueStats = await GetOverdueStatsAsync(bookServiceUrl, correlationId);
                
                var alerts = new List<object>();

                if (overdueStats > 0)
                {
                    alerts.Add(new
                    {
                        id = 1,
                        type = "overdue",
                        message = $"{overdueStats} books are overdue",
                        severity = overdueStats > 5 ? "error" : "warning",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Add system health alert
                alerts.Add(new
                {
                    id = 2,
                    type = "system",
                    message = "All services are operational",
                    severity = "info",
                    timestamp = DateTime.UtcNow
                });

                return Ok(new
                {
                    success = true,
                    data = alerts,
                    correlationId = correlationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching system alerts");
                return StatusCode(500, new { error = "Failed to fetch system alerts", message = ex.Message });
            }
        }

        private async Task<(int totalBooks, int availableBooks)> GetBookStatsAsync(string bookServiceUrl, string correlationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{bookServiceUrl}/api/books");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch books stats from book service - CorrelationId: {CorrelationId}", correlationId);
                    return (0, 0);
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                
                // Mock calculation for now - would parse actual response
                return (totalBooks: 24, availableBooks: 18);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching book stats - CorrelationId: {CorrelationId}", correlationId);
                return (0, 0);
            }
        }

        private async Task<(int totalMembers, int activeMembers)> GetMemberStatsAsync(string memberServiceUrl, string correlationId)
        {
            try
            {
                // Mock for now - would call actual member service
                return (totalMembers: 15, activeMembers: 12);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching member stats - CorrelationId: {CorrelationId}", correlationId);
                return (0, 0);
            }
        }

        private async Task<int> GetOverdueStatsAsync(string bookServiceUrl, string correlationId)
        {
            try
            {
                // Mock for now - would call actual overdue books endpoint
                // Fixed tuple syntax error
                return 3;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching overdue stats - CorrelationId: {CorrelationId}", correlationId);
                return 0;
            }
        }

        private string GetServiceUrl(string serviceName, string defaultUrl)
        {
            return _configuration[$"ServiceUrls:{serviceName}"] ?? defaultUrl;
        }
    }
}