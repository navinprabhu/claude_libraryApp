using LibraryApp.Shared.Models.DTOs;
using LibraryApp.Shared.Models.Enums;
using LibraryApp.Shared.Models.Common;

namespace LibraryApp.Tests
{
    public class SharedModelsTests
    {
        [Fact]
        public void BookDto_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var book = new BookDto();

            // Assert
            Assert.Equal(0, book.Id);
            Assert.Equal(string.Empty, book.Title);
            Assert.Equal(string.Empty, book.Author);
            Assert.Equal(string.Empty, book.ISBN);
            Assert.Equal(BookStatus.Available, book.Status);
        }

        [Fact]
        public void CreateBookDto_ShouldAcceptValidData()
        {
            // Arrange & Act
            var createBook = new CreateBookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                TotalCopies = 5,
                Category = "Fiction",
                Description = "A test book"
            };

            // Assert
            Assert.Equal("Test Book", createBook.Title);
            Assert.Equal("Test Author", createBook.Author);
            Assert.Equal("1234567890", createBook.ISBN);
            Assert.Equal(5, createBook.TotalCopies);
            Assert.Equal("Fiction", createBook.Category);
            Assert.Equal("A test book", createBook.Description);
        }

        [Fact]
        public void ApiResponse_SuccessResponse_ShouldReturnCorrectValues()
        {
            // Arrange
            var testData = "Test Data";
            var message = "Success";

            // Act
            var response = ApiResponse<string>.SuccessResponse(testData, message);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(testData, response.Data);
            Assert.Equal(message, response.Message);
            Assert.Equal(200, response.StatusCode);
            Assert.Empty(response.Errors);
        }

        [Fact]
        public void ApiResponse_ErrorResponse_ShouldReturnCorrectValues()
        {
            // Arrange
            var message = "Error occurred";
            var statusCode = 400;
            var errors = new List<string> { "Validation error 1", "Validation error 2" };

            // Act
            var response = ApiResponse<string>.ErrorResponse(message, statusCode, errors);

            // Assert
            Assert.False(response.Success);
            Assert.Null(response.Data);
            Assert.Equal(message, response.Message);
            Assert.Equal(statusCode, response.StatusCode);
            Assert.Equal(2, response.Errors.Count);
            Assert.Contains("Validation error 1", response.Errors);
            Assert.Contains("Validation error 2", response.Errors);
        }

        [Fact]
        public void PagedResult_ShouldCalculatePropertiesCorrectly()
        {
            // Arrange
            var items = new List<string> { "Item1", "Item2", "Item3" };
            var totalCount = 10;
            var pageNumber = 2;
            var pageSize = 3;

            // Act
            var pagedResult = new PagedResult<string>(items, totalCount, pageNumber, pageSize);

            // Assert
            Assert.Equal(items, pagedResult.Items);
            Assert.Equal(totalCount, pagedResult.TotalCount);
            Assert.Equal(pageNumber, pagedResult.PageNumber);
            Assert.Equal(pageSize, pagedResult.PageSize);
            Assert.Equal(4, pagedResult.TotalPages); // Ceiling(10/3) = 4
            Assert.True(pagedResult.HasNextPage);
            Assert.True(pagedResult.HasPreviousPage);
        }

        [Fact]
        public void BookStatus_ShouldHaveCorrectValues()
        {
            // Act & Assert
            Assert.Equal(0, (int)BookStatus.Available);
            Assert.Equal(1, (int)BookStatus.Borrowed);
        }

        [Fact]
        public void MemberDto_ShouldInitializeWithDefaults()
        {
            // Arrange & Act
            var member = new MemberDto();

            // Assert
            Assert.Equal(0, member.Id);
            Assert.Equal(string.Empty, member.FirstName);
            Assert.Equal(string.Empty, member.LastName);
            Assert.Equal(string.Empty, member.Email);
            Assert.Equal(string.Empty, member.Phone);
            Assert.False(member.IsActive);
            Assert.Equal(string.Empty, member.MembershipNumber);
        }

        [Fact]
        public void BorrowRequestDto_ShouldHaveDefaultBorrowDuration()
        {
            // Arrange & Act
            var borrowRequest = new BorrowRequestDto
            {
                BookId = 1,
                MemberId = 1
            };

            // Assert
            Assert.Equal(1, borrowRequest.BookId);
            Assert.Equal(1, borrowRequest.MemberId);
            Assert.Equal(14, borrowRequest.BorrowDurationDays); // Default value
        }
    }
}