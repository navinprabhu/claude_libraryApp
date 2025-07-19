using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.BookService.Services
{
    public interface IBookService
    {
        Task<ApiResponse<IEnumerable<BookDto>>> GetAllBooksAsync();
        Task<ApiResponse<BookDto>> GetBookByIdAsync(int id);
        Task<ApiResponse<BookDto>> GetBookByISBNAsync(string isbn);
        Task<ApiResponse<PagedResult<BookDto>>> GetBooksPagedAsync(int page, int pageSize, string? searchTerm = null);
        Task<ApiResponse<IEnumerable<BookDto>>> SearchBooksByTitleAsync(string title);
        Task<ApiResponse<IEnumerable<BookDto>>> SearchBooksByAuthorAsync(string author);
        Task<ApiResponse<IEnumerable<BookDto>>> GetBooksByGenreAsync(string genre);
        Task<ApiResponse<IEnumerable<BookDto>>> GetAvailableBooksAsync();
        Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto createBookDto, string createdBy);
        Task<ApiResponse<BookDto>> UpdateBookAsync(int id, UpdateBookDto updateBookDto, string updatedBy);
        Task<ApiResponse<bool>> DeleteBookAsync(int id, string deletedBy);
        Task<ApiResponse<bool>> CheckAvailabilityAsync(int bookId);
        Task<ApiResponse<IEnumerable<BookDto>>> GetOverdueBooksAsync();
    }
}