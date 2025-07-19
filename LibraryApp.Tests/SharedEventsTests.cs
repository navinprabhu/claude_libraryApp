using LibraryApp.Shared.Events;

namespace LibraryApp.Tests
{
    public class SharedEventsTests
    {
        [Fact]
        public void BaseEvent_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var bookCreatedEvent = new BookCreatedEvent();

            // Assert
            Assert.NotEqual(Guid.Empty, bookCreatedEvent.EventId);
            Assert.True(bookCreatedEvent.Timestamp <= DateTime.UtcNow);
            Assert.True(bookCreatedEvent.Timestamp > DateTime.UtcNow.AddSeconds(-5)); // Recent timestamp
            Assert.Equal(string.Empty, bookCreatedEvent.CorrelationId);
        }

        [Fact]
        public void BookCreatedEvent_ShouldAcceptBookData()
        {
            // Arrange & Act
            var bookEvent = new BookCreatedEvent
            {
                BookId = 1,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                TotalCopies = 5,
                CorrelationId = "test-correlation-id"
            };

            // Assert
            Assert.Equal(1, bookEvent.BookId);
            Assert.Equal("Test Book", bookEvent.Title);
            Assert.Equal("Test Author", bookEvent.Author);
            Assert.Equal("1234567890", bookEvent.ISBN);
            Assert.Equal(5, bookEvent.TotalCopies);
            Assert.Equal("test-correlation-id", bookEvent.CorrelationId);
        }

        [Fact]
        public void MemberRegisteredEvent_ShouldAcceptMemberData()
        {
            // Arrange & Act
            var memberEvent = new MemberRegisteredEvent
            {
                MemberId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                MembershipNumber = "MEM001",
                CorrelationId = "test-correlation-id"
            };

            // Assert
            Assert.Equal(1, memberEvent.MemberId);
            Assert.Equal("John", memberEvent.FirstName);
            Assert.Equal("Doe", memberEvent.LastName);
            Assert.Equal("john.doe@example.com", memberEvent.Email);
            Assert.Equal("MEM001", memberEvent.MembershipNumber);
            Assert.Equal("test-correlation-id", memberEvent.CorrelationId);
        }

        [Fact]
        public void BookBorrowedEvent_ShouldAcceptBorrowingData()
        {
            // Arrange & Act
            var borrowEvent = new BookBorrowedEvent
            {
                BorrowingRecordId = 1,
                BookId = 1,
                MemberId = 1,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                BookTitle = "Test Book",
                MemberEmail = "test@example.com",
                CorrelationId = "test-correlation-id"
            };

            // Assert
            Assert.Equal(1, borrowEvent.BorrowingRecordId);
            Assert.Equal(1, borrowEvent.BookId);
            Assert.Equal(1, borrowEvent.MemberId);
            Assert.Equal("Test Book", borrowEvent.BookTitle);
            Assert.Equal("test@example.com", borrowEvent.MemberEmail);
            Assert.Equal("test-correlation-id", borrowEvent.CorrelationId);
            Assert.True(borrowEvent.DueDate > borrowEvent.BorrowDate);
        }

        [Fact]
        public void BookOverdueEvent_ShouldCalculateOverdueDays()
        {
            // Arrange & Act
            var overdueEvent = new BookOverdueEvent
            {
                BorrowingRecordId = 1,
                BookId = 1,
                MemberId = 1,
                DueDate = DateTime.UtcNow.AddDays(-5), // 5 days overdue
                DaysOverdue = 5,
                BookTitle = "Overdue Book",
                MemberEmail = "overdue@example.com"
            };

            // Assert
            Assert.Equal(1, overdueEvent.BorrowingRecordId);
            Assert.Equal(5, overdueEvent.DaysOverdue);
            Assert.True(overdueEvent.DueDate < DateTime.UtcNow);
            Assert.Equal("Overdue Book", overdueEvent.BookTitle);
            Assert.Equal("overdue@example.com", overdueEvent.MemberEmail);
        }

        [Fact]
        public void BookReturnedEvent_ShouldTrackReturnDetails()
        {
            // Arrange
            var dueDate = DateTime.UtcNow.AddDays(-1); // Was due yesterday
            var returnDate = DateTime.UtcNow; // Returned today

            // Act
            var returnEvent = new BookReturnedEvent
            {
                BorrowingRecordId = 1,
                BookId = 1,
                MemberId = 1,
                ReturnDate = returnDate,
                DueDate = dueDate,
                IsLate = true,
                LateFee = 5.00m,
                BookTitle = "Returned Book",
                MemberEmail = "member@example.com"
            };

            // Assert
            Assert.Equal(1, returnEvent.BorrowingRecordId);
            Assert.True(returnEvent.IsLate);
            Assert.Equal(5.00m, returnEvent.LateFee);
            Assert.True(returnEvent.ReturnDate > returnEvent.DueDate);
            Assert.Equal("Returned Book", returnEvent.BookTitle);
            Assert.Equal("member@example.com", returnEvent.MemberEmail);
        }
    }
}