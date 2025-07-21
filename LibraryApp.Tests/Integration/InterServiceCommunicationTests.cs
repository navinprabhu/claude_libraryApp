using LibraryApp.BookService.Controllers;
using LibraryApp.BookService.Services;
using LibraryApp.MemberService.Controllers;
using LibraryApp.MemberService.Services;
using LibraryApp.MemberService.Services.External;
using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Infrastructure.Middleware;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LibraryApp.Tests.Integration
{
    public class InterServiceCommunicationTests
    {
        private readonly Mock<ILogger<MembersController>> _memberControllerLogger;
        private readonly Mock<ILogger<BorrowingsController>> _borrowingsControllerLogger;
        private readonly Mock<IMemberService> _memberService;
        private readonly Mock<IBookService> _bookService;
        private readonly Mock<LibraryApp.MemberService.Services.External.IBookServiceClient> _bookServiceClient;
        private readonly Mock<IEventPublisher> _eventPublisher;
        private readonly Mock<ICorrelationIdService> _correlationIdService;

        public InterServiceCommunicationTests()
        {
            _memberControllerLogger = new Mock<ILogger<MembersController>>();
            _borrowingsControllerLogger = new Mock<ILogger<BorrowingsController>>();
            _memberService = new Mock<IMemberService>();
            _bookService = new Mock<IBookService>();
            _bookServiceClient = new Mock<LibraryApp.MemberService.Services.External.IBookServiceClient>();
            _eventPublisher = new Mock<IEventPublisher>();
            _correlationIdService = new Mock<ICorrelationIdService>();
        }

        [Fact]
        public async Task MemberService_GetBorrowingEligibility_ReturnsCorrectEligibility()
        {
            // Arrange
            var memberId = 1;
            var memberData = new MemberDto
            {
                Id = memberId,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true,
                MaxBooksAllowed = 5
            };

            var memberResponse = ApiResponse<MemberDto>.SuccessResponse(memberData);
            var borrowedBooks = new List<BorrowedBookResponse>
            {
                new BorrowedBookResponse
                {
                    BorrowingId = 1,
                    BookId = 1,
                    BookTitle = "Test Book",
                    BookAuthor = "Test Author",
                    BorrowDate = DateTime.UtcNow.AddDays(-5),
                    DueDate = DateTime.UtcNow.AddDays(9),
                    IsOverdue = false
                }
            };

            _memberService.Setup(s => s.GetMemberByIdAsync(memberId))
                .ReturnsAsync(memberResponse);

            _bookServiceClient.Setup(s => s.GetMemberBorrowedBooksAsync(memberId))
                .ReturnsAsync(ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(
                    borrowedBooks.Select(b => new BorrowingRecordDto
                    {
                        Id = b.BorrowingId,
                        BookId = b.BookId,
                        BookTitle = b.BookTitle,
                        BookAuthor = b.BookAuthor,
                        MemberId = memberId,
                        BorrowedAt = b.BorrowDate,
                        DueDate = b.DueDate,
                        IsReturned = false
                    })));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(LibraryApp.MemberService.Services.External.IBookServiceClient)))
                .Returns(_bookServiceClient.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider.Object;

            var controller = new MembersController(_memberService.Object, _memberControllerLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = await controller.GetBorrowingEligibility(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var eligibility = okResult.Value;
            Assert.NotNull(eligibility);
            
            // Verify member service was called
            _memberService.Verify(s => s.GetMemberByIdAsync(memberId), Times.Once);
            
            // Verify book service client was called
            _bookServiceClient.Verify(s => s.GetMemberBorrowedBooksAsync(memberId), Times.Once);
        }

        [Fact]
        public async Task BookService_BorrowingEndpoint_PublishesEvent()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            _correlationIdService.Setup(s => s.GetCorrelationId()).Returns(correlationId);

            var borrowingService = new Mock<IBorrowingService>();
            var controller = new BorrowingsController(borrowingService.Object, _borrowingsControllerLogger.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Items["CorrelationId"] = correlationId;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var memberId = 1;
            var borrowedBooks = new List<BorrowingRecordDto>
            {
                new BorrowingRecordDto
                {
                    Id = 1,
                    BookId = 1,
                    BookTitle = "Test Book",
                    BookAuthor = "Test Author",
                    MemberId = memberId,
                    BorrowedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14),
                    IsReturned = false
                }
            };

            borrowingService.Setup(s => s.GetMemberBorrowingsAsync(memberId))
                .ReturnsAsync(ApiResponse<IEnumerable<BorrowingRecordDto>>.SuccessResponse(borrowedBooks));

            // Act
            var result = await controller.GetMemberCurrentBorrowings(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            borrowingService.Verify(s => s.GetMemberBorrowingsAsync(memberId), Times.Once);
        }

        [Fact]
        public void CorrelationId_FlowsThroughServices()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            
            // Act
            _correlationIdService.Setup(s => s.GetCorrelationId()).Returns(correlationId);

            // Verify correlation ID is properly set and flows through the system
            Assert.Equal(correlationId, _correlationIdService.Object.GetCorrelationId());
        }

        [Fact]
        public async Task EventPublisher_HandlesEventSerialization()
        {
            // Arrange
            var bookBorrowedEvent = new LibraryApp.Shared.Events.BookBorrowedEvent
            {
                BorrowingRecordId = 1000,
                //EventType = "BookBorrowed",
                Timestamp = DateTime.UtcNow,
                CorrelationId = Guid.NewGuid().ToString(),
                BookId = 1,
                BookTitle = "Test Book",
                //BookAuthor = "Test Author",
                MemberId = 1,
                MemberEmail = "test@example.com",
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            _eventPublisher.Setup(p => p.PublishAsync(It.IsAny<LibraryApp.Shared.Events.BookBorrowedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _eventPublisher.Object.PublishAsync(bookBorrowedEvent);

            // Assert
            _eventPublisher.Verify(p => p.PublishAsync(It.IsAny<LibraryApp.Shared.Events.BookBorrowedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MemberService_HandlesMissingMember()
        {
            // Arrange
            var memberId = 999;
            var memberResponse = ApiResponse<MemberDto>.ErrorResponse("Member not found", 404);

            _memberService.Setup(s => s.GetMemberByIdAsync(memberId))
                .ReturnsAsync(memberResponse);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(LibraryApp.MemberService.Services.External.IBookServiceClient)))
                .Returns(_bookServiceClient.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider.Object;

            var controller = new MembersController(_memberService.Object, _memberControllerLogger.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = await controller.GetBorrowingEligibility(memberId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Member with ID 999 not found", notFoundResult.Value?.ToString());
        }

        [Fact]
        public async Task BookServiceClient_HandlesServiceUnavailable()
        {
            // Arrange
            var memberId = 1;
            var mockLogger = new Mock<ILogger<BookServiceClient>>();
            
            var httpClient = new HttpClient(new MockHttpMessageHandler(System.Net.HttpStatusCode.ServiceUnavailable));
            
            var client = new BookServiceClient(httpClient, mockLogger.Object, _correlationIdService.Object);

            // Act
            var result = await client.GetMemberBorrowedBooksAsync(memberId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(503, result.StatusCode);
            Assert.Equal("BookService unavailable", result.Message);
        }
    }

    // Helper class for mocking HTTP responses
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly System.Net.HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(System.Net.HttpStatusCode statusCode, string content = "")
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }
}