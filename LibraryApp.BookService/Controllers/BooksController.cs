using LibraryApp.BookService.Infrastructure.Authorization;
using LibraryApp.BookService.Services;
using LibraryApp.Shared.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LibraryApp.BookService.Models.Entities;

namespace LibraryApp.BookService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var result = await _bookService.GetAllBooksAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetBooksPagedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _bookService.GetBooksPagedAsync(page, pageSize, searchTerm);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var result = await _bookService.GetBookByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("isbn/{isbn}")]
        public async Task<IActionResult> GetBookByISBN(string isbn)
        {
            var result = await _bookService.GetBookByISBNAsync(isbn);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("search/title")]
        public async Task<IActionResult> SearchBooksByTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest(new { error = "Title parameter is required" });
            }

            var result = await _bookService.SearchBooksByTitleAsync(title);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("search/author")]
        public async Task<IActionResult> SearchBooksByAuthor([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author))
            {
                return BadRequest(new { error = "Author parameter is required" });
            }

            var result = await _bookService.SearchBooksByAuthorAsync(author);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("genre/{genre}")]
        public async Task<IActionResult> GetBooksByGenre(string genre)
        {
            var result = await _bookService.GetBooksByGenreAsync(genre);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            var result = await _bookService.GetAvailableBooksAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("overdue")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> GetOverdueBooks()
        {
            var result = await _bookService.GetOverdueBooksAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:int}/availability")]
        public async Task<IActionResult> CheckBookAvailability(int id)
        {
            var result = await _bookService.CheckAvailabilityAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookDto createBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBy = GetCurrentUsername();
            var result = await _bookService.CreateBookAsync(createBookDto, createdBy);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(nameof(GetBookById), new { id = result.Data.Id }, result);
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:int}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookDto updateBookDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedBy = GetCurrentUsername();
            var result = await _bookService.UpdateBookAsync(id, updateBookDto, updatedBy);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:int}")]
        [AuthorizeRoles(UserRoles.Admin)]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var deletedBy = GetCurrentUsername();
            var result = await _bookService.DeleteBookAsync(id, deletedBy);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets borrowing status for a specific book - used by other services
        /// </summary>
        [HttpGet("{id:int}/borrowing-status")]
        [AllowAnonymous] // Allow inter-service calls without authentication
        public async Task<IActionResult> GetBorrowingStatus(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (!book.Success || book.Data == null)
                {
                    return NotFound($"Book with ID {id} not found");
                }

                // Get current borrowings for this book
                var borrowingService = HttpContext.RequestServices.GetRequiredService<IBorrowingService>();
                var currentBorrowings = await borrowingService.GetBookBorrowingsAsync(id);

                var borrowingStatus = new
                {
                    BookId = id,
                    TotalCopies = book.Data.TotalCopies,
                    AvailableCopies = book.Data.AvailableCopies,
                    BorrowedCopies = book.Data.TotalCopies - book.Data.AvailableCopies,
                    IsAvailable = book.Data.AvailableCopies > 0,
                    CurrentBorrowings = currentBorrowings.Data?.Select(b => new
                    {
                        BorrowingId = b.Id,
                        MemberId = b.MemberId,
                        MemberEmail = b.MemberEmail,
                        BorrowDate = b.BorrowedAt,
                        DueDate = b.DueDate,
                        IsOverdue = b.DueDate < DateTime.UtcNow && !b.IsReturned
                    }) ?? Enumerable.Empty<object>()
                };

                return Ok(borrowingStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting borrowing status for book {BookId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Member";
        }
    }
}