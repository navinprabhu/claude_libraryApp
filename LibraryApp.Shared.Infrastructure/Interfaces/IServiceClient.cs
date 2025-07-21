namespace LibraryApp.Shared.Infrastructure.Interfaces
{
    /// <summary>
    /// Base interface for service-to-service HTTP communication
    /// </summary>
    public interface IServiceClient
    {
        /// <summary>
        /// Gets the service name this client communicates with
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Performs a health check on the target service
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the service is healthy, false otherwise</returns>
        Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for HTTP communication with the Book Service
    /// </summary>
    public interface IBookServiceClient : IServiceClient
    {
        /// <summary>
        /// Gets book information by ID
        /// </summary>
        /// <param name="bookId">The book ID</param>
        /// <param name="correlationId">Correlation ID for request tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Book information or null if not found</returns>
        Task<BookServiceResponse?> GetBookAsync(int bookId, string correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets borrowing status for a specific book
        /// </summary>
        /// <param name="bookId">The book ID</param>
        /// <param name="correlationId">Correlation ID for request tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Borrowing status information</returns>
        Task<BookBorrowingStatusResponse?> GetBorrowingStatusAsync(int bookId, string correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all books borrowed by a specific member
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <param name="correlationId">Correlation ID for request tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of borrowed books</returns>
        Task<IEnumerable<BorrowedBookResponse>> GetMemberBorrowedBooksAsync(int memberId, string correlationId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for HTTP communication with the Member Service
    /// </summary>
    public interface IMemberServiceClient : IServiceClient
    {
        /// <summary>
        /// Gets member information by ID
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <param name="correlationId">Correlation ID for request tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Member information or null if not found</returns>
        Task<MemberServiceResponse?> GetMemberAsync(int memberId, string correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates if a member can borrow books
        /// </summary>
        /// <param name="memberId">The member ID</param>
        /// <param name="correlationId">Correlation ID for request tracking</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result</returns>
        Task<MemberBorrowingEligibilityResponse> ValidateBorrowingEligibilityAsync(int memberId, string correlationId, CancellationToken cancellationToken = default);
    }

    // Response models for service communication
    public class BookServiceResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class BookBorrowingStatusResponse
    {
        public int BookId { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int BorrowedCopies { get; set; }
        public bool IsAvailable { get; set; }
        public IEnumerable<CurrentBorrowing> CurrentBorrowings { get; set; } = Enumerable.Empty<CurrentBorrowing>();
    }

    public class CurrentBorrowing
    {
        public int BorrowingId { get; set; }
        public int MemberId { get; set; }
        public string MemberEmail { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class BorrowedBookResponse
    {
        public int BorrowingId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? ReturnDate { get; set; }
    }

    public class MemberServiceResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MembershipNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime MembershipDate { get; set; }
    }

    public class MemberBorrowingEligibilityResponse
    {
        public bool IsEligible { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int CurrentBorrowedCount { get; set; }
        public int MaxBorrowLimit { get; set; }
        public bool HasOverdueBooks { get; set; }
        public decimal OutstandingFees { get; set; }
    }
}