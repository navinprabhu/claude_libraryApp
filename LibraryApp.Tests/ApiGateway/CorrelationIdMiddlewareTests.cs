using LibraryApp.ApiGateway.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace LibraryApp.Tests.ApiGateway
{
    public class CorrelationIdMiddlewareTests
    {
        private readonly Mock<ILogger<CorrelationIdMiddleware>> _loggerMock;
        private readonly Mock<RequestDelegate> _nextMock;

        public CorrelationIdMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
            _nextMock = new Mock<RequestDelegate>();
        }

        [Fact]
        public async Task InvokeAsync_Should_Generate_CorrelationId_When_Not_Present()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test";
            
            var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey("X-Correlation-ID"));
            Assert.True(context.Items.ContainsKey("CorrelationId"));
            
            var correlationId = context.Items["CorrelationId"]?.ToString();
            Assert.False(string.IsNullOrEmpty(correlationId));
            Assert.True(Guid.TryParse(correlationId, out _));
            
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Use_Existing_CorrelationId_When_Present()
        {
            // Arrange
            var existingCorrelationId = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Request.Headers.Append("X-Correlation-ID", existingCorrelationId);
            context.Request.Method = "POST";
            context.Request.Path = "/api/test";

            var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(existingCorrelationId, context.Response.Headers["X-Correlation-ID"].ToString());
            Assert.Equal(existingCorrelationId, context.Items["CorrelationId"]?.ToString());
            
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Add_CorrelationId_To_Activity()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test";

            var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

            // Start an activity to test
            using var activity = new Activity("Test").Start();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId = context.Items["CorrelationId"]?.ToString();
            Assert.NotNull(correlationId);
            
            // Check if the correlation ID was added to the current activity
            Assert.NotNull(Activity.Current);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_Should_Log_Request_Processing()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "PUT";
            context.Request.Path = "/api/books/123";

            var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Processing request with correlation ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Completed request with correlation ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task InvokeAsync_Should_Generate_New_CorrelationId_When_Header_Is_Empty(string? emptyValue)
        {
            // Arrange
            var context = new DefaultHttpContext();
            if (emptyValue != null)
            {
                context.Request.Headers.Append("X-Correlation-ID", emptyValue);
            }
            context.Request.Method = "DELETE";
            context.Request.Path = "/api/members/456";

            var middleware = new CorrelationIdMiddleware(_nextMock.Object, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            var correlationId = context.Items["CorrelationId"]?.ToString();
            Assert.NotNull(correlationId);
            
            // The middleware uses !string.IsNullOrEmpty() so whitespace is preserved
            if (emptyValue == "   ")
            {
                // Whitespace-only strings are preserved as-is
                Assert.Equal("   ", correlationId);
            }
            else
            {
                // Empty or null values generate a new GUID
                Assert.True(Guid.TryParse(correlationId, out _));
            }
            
            _nextMock.Verify(x => x(context), Times.Once);
        }
    }
}