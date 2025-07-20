using LibraryApp.ApiGateway.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace LibraryApp.Tests.ApiGateway
{
    public class RateLimitingMiddlewareTests
    {
        private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;

        public RateLimitingMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
        }

        [Fact]
        public async Task InvokeAsync_Should_Continue_Pipeline_When_No_Rate_Limit_Exception()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Items["CorrelationId"] = Guid.NewGuid().ToString();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            Assert.NotEqual(429, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_Should_Handle_Rate_Limit_Exception()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Items["CorrelationId"] = correlationId;
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");
            context.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                    .ThrowsAsync(new Exception("rate limit exceeded"));

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(429, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read response body
            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

            Assert.Equal("Rate limit exceeded", response.GetProperty("error").GetString());
            Assert.Equal(correlationId, response.GetProperty("correlationId").GetString());
            Assert.Equal("60", response.GetProperty("retryAfter").GetString());
        }

        [Fact]
        public async Task InvokeAsync_Should_Log_Rate_Limit_Warning()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Items["CorrelationId"] = correlationId;
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.50");
            context.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                    .ThrowsAsync(new Exception("rate limit violation detected"));

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Rate limit exceeded")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Handle_Missing_CorrelationId()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("172.16.0.1");
            context.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                    .ThrowsAsync(new Exception("rate limit"));

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<JsonElement>(responseBody);

            Assert.Equal("unknown", response.GetProperty("correlationId").GetString());
            Assert.Equal(429, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("X-Forwarded-For", "203.0.113.1")]
        [InlineData("X-Real-IP", "198.51.100.1")]
        public async Task InvokeAsync_Should_Extract_Client_IP_From_Headers(string headerName, string ipAddress)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers[headerName] = ipAddress;
            context.Items["CorrelationId"] = Guid.NewGuid().ToString();
            context.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                    .ThrowsAsync(new Exception("rate limit test"));

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(ipAddress)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Rethrow_Non_RateLimit_Exceptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Items["CorrelationId"] = Guid.NewGuid().ToString();

            var customException = new InvalidOperationException("Some other error");
            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                    .ThrowsAsync(customException);

            var middleware = new RateLimitingMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => middleware.InvokeAsync(context));
            
            Assert.Same(customException, thrownException);
        }
    }
}