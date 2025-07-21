using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Infrastructure.Telemetry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryApp.Shared.Infrastructure.Services
{
    /// <summary>
    /// HTTP client for communicating with the Book Service
    /// </summary>
    public class BookServiceClient : IBookServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookServiceClient> _logger;
        private readonly ServiceClientOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IServiceCallMetrics? _metrics;

        public string ServiceName => "BookService";

        public BookServiceClient(
            HttpClient httpClient, 
            ILogger<BookServiceClient> logger,
            IOptions<ServiceClientOptions> options,
            IServiceCallMetrics? metrics = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _metrics = metrics;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Set base address if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_options.BookServiceBaseUrl);
            }
        }

        public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Performing health check for {ServiceName}", ServiceName);
                
                var response = await _httpClient.GetAsync("/health", cancellationToken);
                var isHealthy = response.IsSuccessStatusCode;
                
                _logger.LogDebug(
                    "Health check for {ServiceName} completed with status {StatusCode}", 
                    ServiceName, 
                    response.StatusCode);
                
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for {ServiceName}", ServiceName);
                return false;
            }
        }

        public async Task<BookServiceResponse?> GetBookAsync(int bookId, string correlationId, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var operation = "GetBook";
            bool success = false;
            
            try
            {
                _logger.LogDebug(
                    "Getting book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);

                _metrics?.RecordServiceCall(ServiceName, operation, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Book {BookId} not found in {ServiceName}", bookId, ServiceName);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var book = await response.Content.ReadFromJsonAsync<BookServiceResponse>(_jsonOptions, cancellationToken);
                
                _logger.LogDebug(
                    "Successfully retrieved book {BookId} from {ServiceName}", 
                    bookId, ServiceName);

                success = true;
                return book;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, 
                    "HTTP error getting book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, 
                    "Request timeout getting book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Unexpected error getting book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metrics?.RecordServiceCallDuration(ServiceName, operation, stopwatch.Elapsed.TotalMilliseconds, success, correlationId);
            }
        }

        public async Task<BookBorrowingStatusResponse?> GetBorrowingStatusAsync(int bookId, string correlationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug(
                    "Getting borrowing status for book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/books/{bookId}/borrowing-status");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Borrowing status for book {BookId} not found in {ServiceName}", bookId, ServiceName);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var status = await response.Content.ReadFromJsonAsync<BookBorrowingStatusResponse>(_jsonOptions, cancellationToken);
                
                _logger.LogDebug(
                    "Successfully retrieved borrowing status for book {BookId} from {ServiceName}", 
                    bookId, ServiceName);

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error getting borrowing status for book {BookId} from {ServiceName} with correlation ID {CorrelationId}", 
                    bookId, ServiceName, correlationId);
                throw;
            }
        }

        public async Task<IEnumerable<BorrowedBookResponse>> GetMemberBorrowedBooksAsync(int memberId, string correlationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug(
                    "Getting borrowed books for member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/borrowing/member/{memberId}/current");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var borrowedBooks = await response.Content.ReadFromJsonAsync<IEnumerable<BorrowedBookResponse>>(_jsonOptions, cancellationToken);
                
                _logger.LogDebug(
                    "Successfully retrieved {Count} borrowed books for member {MemberId} from {ServiceName}", 
                    borrowedBooks?.Count() ?? 0, memberId, ServiceName);

                return borrowedBooks ?? Enumerable.Empty<BorrowedBookResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error getting borrowed books for member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);
                throw;
            }
        }
    }

    /// <summary>
    /// Configuration options for service clients
    /// </summary>
    public class ServiceClientOptions
    {
        public const string SectionName = "ServiceClients";

        public string BookServiceBaseUrl { get; set; } = "http://localhost:5002";
        public string MemberServiceBaseUrl { get; set; } = "http://localhost:5003";
        public string AuthServiceBaseUrl { get; set; } = "http://localhost:5001";
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        public int CircuitBreakerDurationSeconds { get; set; } = 60;
    }
}