using System.Diagnostics;
using System.Text;

namespace LibraryApp.ApiGateway.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";
            
            // Log request details
            await LogRequest(context, correlationId);
            
            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;
            
            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;
                
                await _next(context);
                
                stopwatch.Stop();
                
                // Log response details
                await LogResponse(context, correlationId, stopwatch.ElapsedMilliseconds);
                
                // Copy the captured response back to the original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request failed for {CorrelationId} after {ElapsedMs}ms", 
                    correlationId, stopwatch.ElapsedMilliseconds);
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpContext context, string correlationId)
        {
            var request = context.Request;
            
            var requestInfo = new
            {
                CorrelationId = correlationId,
                Method = request.Method,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                UserAgent = request.Headers.UserAgent.ToString(),
                RemoteIp = GetClientIpAddress(context),
                ContentType = request.ContentType,
                ContentLength = request.ContentLength,
                Headers = GetSafeHeaders(request.Headers)
            };
            
            _logger.LogInformation("Incoming request: {@RequestInfo}", requestInfo);
            
            // Log request body for POST/PUT requests (be careful with sensitive data)
            if (ShouldLogRequestBody(request))
            {
                var requestBody = await ReadRequestBody(request);
                if (!string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogDebug("Request body for {CorrelationId}: {RequestBody}", 
                        correlationId, SanitizeRequestBody(requestBody));
                }
            }
        }

        private async Task LogResponse(HttpContext context, string correlationId, long elapsedMs)
        {
            var response = context.Response;
            
            var responseInfo = new
            {
                CorrelationId = correlationId,
                StatusCode = response.StatusCode,
                ContentType = response.ContentType,
                ContentLength = response.Body.Length,
                ElapsedMilliseconds = elapsedMs,
                Headers = GetSafeHeaders(response.Headers)
            };
            
            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, "Outgoing response: {@ResponseInfo}", responseInfo);
            
            // Log response body for errors (be careful with sensitive data)
            if (response.StatusCode >= 400 && ShouldLogResponseBody(response))
            {
                var responseBody = await ReadResponseBody(context.Response);
                if (!string.IsNullOrEmpty(responseBody))
                {
                    _logger.LogWarning("Error response body for {CorrelationId}: {ResponseBody}", 
                        correlationId, responseBody);
                }
            }
        }

        private string GetClientIpAddress(HttpContext context)
        {
            var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = context.Connection.RemoteIpAddress?.ToString();
            }
            return ipAddress ?? "unknown";
        }

        private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
        {
            var safeHeaders = new Dictionary<string, string>();
            var sensitiveHeaders = new[] { "authorization", "cookie", "x-api-key" };
            
            foreach (var header in headers)
            {
                var key = header.Key.ToLowerInvariant();
                if (sensitiveHeaders.Contains(key))
                {
                    safeHeaders[header.Key] = "[REDACTED]";
                }
                else
                {
                    safeHeaders[header.Key] = header.Value.ToString();
                }
            }
            
            return safeHeaders;
        }

        private bool ShouldLogRequestBody(HttpRequest request)
        {
            return (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put || 
                    request.Method == HttpMethods.Patch) &&
                   request.ContentLength > 0 &&
                   request.ContentLength < 1024 * 10; // Only log bodies smaller than 10KB
        }

        private bool ShouldLogResponseBody(HttpResponse response)
        {
            return response.Body.Length > 0 && response.Body.Length < 1024 * 10; // Only log bodies smaller than 10KB
        }

        private async Task<string> ReadRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Position = 0;
            
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            
            return body;
        }

        private async Task<string> ReadResponseBody(HttpResponse response)
        {
            response.Body.Position = 0;
            
            using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            response.Body.Position = 0;
            
            return body;
        }

        private string SanitizeRequestBody(string requestBody)
        {
            // Remove sensitive information from request body
            // This is a simple implementation - in production, you might want more sophisticated sanitization
            var sensitiveFields = new[] { "password", "token", "secret", "key" };
            
            foreach (var field in sensitiveFields)
            {
                requestBody = System.Text.RegularExpressions.Regex.Replace(
                    requestBody, 
                    $@"""?{field}""?\s*:\s*""[^""]*""", 
                    $@"""{field}"": ""[REDACTED]""", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            return requestBody;
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}