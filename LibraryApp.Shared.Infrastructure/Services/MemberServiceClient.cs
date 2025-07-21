using LibraryApp.Shared.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryApp.Shared.Infrastructure.Services
{
    /// <summary>
    /// HTTP client for communicating with the Member Service
    /// </summary>
    public class MemberServiceClient : IMemberServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MemberServiceClient> _logger;
        private readonly ServiceClientOptions _options;
        private readonly JsonSerializerOptions _jsonOptions;

        public string ServiceName => "MemberService";

        public MemberServiceClient(
            HttpClient httpClient, 
            ILogger<MemberServiceClient> logger,
            IOptions<ServiceClientOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Set base address if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(_options.MemberServiceBaseUrl);
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

        public async Task<MemberServiceResponse?> GetMemberAsync(int memberId, string correlationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug(
                    "Getting member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/members/{memberId}");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogDebug("Member {MemberId} not found in {ServiceName}", memberId, ServiceName);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var member = await response.Content.ReadFromJsonAsync<MemberServiceResponse>(_jsonOptions, cancellationToken);
                
                _logger.LogDebug(
                    "Successfully retrieved member {MemberId} from {ServiceName}", 
                    memberId, ServiceName);

                return member;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, 
                    "HTTP error getting member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, 
                    "Request timeout getting member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Unexpected error getting member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);
                throw;
            }
        }

        public async Task<MemberBorrowingEligibilityResponse> ValidateBorrowingEligibilityAsync(int memberId, string correlationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug(
                    "Validating borrowing eligibility for member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);

                using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/members/{memberId}/borrowing-eligibility");
                request.Headers.Add("X-Correlation-ID", correlationId);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var eligibility = await response.Content.ReadFromJsonAsync<MemberBorrowingEligibilityResponse>(_jsonOptions, cancellationToken);
                
                _logger.LogDebug(
                    "Successfully validated borrowing eligibility for member {MemberId} from {ServiceName}. Eligible: {IsEligible}", 
                    memberId, ServiceName, eligibility?.IsEligible ?? false);

                return eligibility ?? new MemberBorrowingEligibilityResponse
                {
                    IsEligible = false,
                    Reason = "Failed to validate eligibility"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error validating borrowing eligibility for member {MemberId} from {ServiceName} with correlation ID {CorrelationId}", 
                    memberId, ServiceName, correlationId);
                
                // Return safe default for business continuity
                return new MemberBorrowingEligibilityResponse
                {
                    IsEligible = false,
                    Reason = "Service unavailable - please try again later"
                };
            }
        }
    }
}