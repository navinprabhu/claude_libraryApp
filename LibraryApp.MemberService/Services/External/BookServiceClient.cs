using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;
using Polly;
using Polly.Extensions.Http;
using System.Text.Json;

namespace LibraryApp.MemberService.Services.External
{
    public class BookServiceClient : IBookServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookServiceClient> _logger;
        private readonly ICorrelationIdService _correlationIdService;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookServiceClient(
            HttpClient httpClient, 
            ILogger<BookServiceClient> logger,
            ICorrelationIdService correlationIdService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _correlationIdService = correlationIdService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowedBooksAsync(int memberId)
        {
            var correlationId = _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString();
            
            try
            {
                _logger.LogInformation("Calling BookService to get borrowed books for member: {MemberId} with correlation ID: {CorrelationId}", 
                    memberId, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/borrowings/member/{memberId}/current");
                request.Headers.Add("X-Correlation-ID", correlationId);
                
                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var borrowedBooks = JsonSerializer.Deserialize<IEnumerable<BorrowedBookResponse>>(content, _jsonOptions);
                    
                    // Convert to BorrowingRecordDto format
                    var borrowingRecords = borrowedBooks?.Select(b => new BorrowingRecordDto
                    {
                        Id = b.BorrowingId,
                        BookId = b.BookId,
                        BookTitle = b.BookTitle,
                        BookAuthor = b.BookAuthor,
                        MemberId = memberId,
                        BorrowedAt = b.BorrowDate,
                        DueDate = b.DueDate,
                        IsReturned = false,
                        ReturnedAt = null
                    }) ?? Enumerable.Empty<BorrowingRecordDto>();

                    return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowingRecords);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(
                        new List<BorrowingRecordDto>(), "No borrowed books found");
                }
                else
                {
                    _logger.LogWarning("BookService returned error status: {StatusCode} for correlation ID: {CorrelationId}", 
                        response.StatusCode, correlationId);
                    return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService unavailable", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BookService for member borrowed books: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService unavailable", 503);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling BookService for member borrowed books: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService timeout", 504);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling BookService for member borrowed books: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowed books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BorrowingRecordDto>>> GetMemberBorrowingHistoryAsync(int memberId)
        {
            try
            {
                _logger.LogInformation("Calling BookService to get borrowing history for member: {MemberId}", memberId);
                
                var response = await _httpClient.GetAsync($"/api/borrowings/history/{memberId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<BorrowingRecordDto>>>(content, _jsonOptions);
                    return apiResponse ?? ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Invalid response from BookService", 500);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(
                        new List<BorrowingRecordDto>(), "No borrowing history found");
                }
                else
                {
                    _logger.LogWarning("BookService returned error status: {StatusCode}", response.StatusCode);
                    return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService unavailable", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BookService for member history: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService unavailable", 503);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling BookService for member history: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("BookService timeout", 504);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling BookService for member history: {MemberId}", memberId);
                return ApiResponse<IEnumerable<BorrowingRecordDto>>.ErrorResponse("Failed to retrieve borrowing history", 500);
            }
        }

        public async Task<ApiResponse<int>> GetMemberActiveBorrowingCountAsync(int memberId)
        {
            try
            {
                _logger.LogInformation("Calling BookService to get active borrowing count for member: {MemberId}", memberId);
                
                var response = await _httpClient.GetAsync($"/api/borrowings/member/{memberId}/active-count");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<int>>(content, _jsonOptions);
                    return apiResponse ?? ApiResponse<int>.ErrorResponse("Invalid response from BookService", 500);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return ApiResponse<int>.SuccessResponse(0, "No active borrowings found");
                }
                else
                {
                    _logger.LogWarning("BookService returned error status: {StatusCode}", response.StatusCode);
                    return ApiResponse<int>.ErrorResponse("BookService unavailable", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BookService for active borrowing count: {MemberId}", memberId);
                return ApiResponse<int>.ErrorResponse("BookService unavailable", 503);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling BookService for active borrowing count: {MemberId}", memberId);
                return ApiResponse<int>.ErrorResponse("BookService timeout", 504);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling BookService for active borrowing count: {MemberId}", memberId);
                return ApiResponse<int>.ErrorResponse("Failed to retrieve active borrowing count", 500);
            }
        }

        public async Task<ApiResponse<bool>> CanMemberBorrowAsync(int memberId, int maxBooks)
        {
            try
            {
                _logger.LogInformation("Calling BookService to check if member can borrow: {MemberId}", memberId);
                
                var response = await _httpClient.GetAsync($"/api/borrowings/member/{memberId}/can-borrow?maxBooks={maxBooks}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    return apiResponse ?? ApiResponse<bool>.ErrorResponse("Invalid response from BookService", 500);
                }
                else
                {
                    _logger.LogWarning("BookService returned error status: {StatusCode}", response.StatusCode);
                    return ApiResponse<bool>.ErrorResponse("BookService unavailable", (int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling BookService for can member borrow: {MemberId}", memberId);
                return ApiResponse<bool>.ErrorResponse("BookService unavailable", 503);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout calling BookService for can member borrow: {MemberId}", memberId);
                return ApiResponse<bool>.ErrorResponse("BookService timeout", 504);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling BookService for can member borrow: {MemberId}", memberId);
                return ApiResponse<bool>.ErrorResponse("Failed to check borrowing eligibility", 500);
            }
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry attempt {retryCount} in {timespan.TotalMilliseconds}ms");
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (exception, duration) =>
                    {
                        // Log circuit breaker opening
                    },
                    onReset: () =>
                    {
                        // Log circuit breaker closing
                    });
        }
    }
}