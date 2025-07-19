using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Interfaces;
using LibraryApp.Shared.Models.Common;

namespace LibraryApp.BookService.Data.Repositories
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<Book?> GetByISBNAsync(string isbn);
        Task<IEnumerable<Book>> GetByTitleAsync(string title);
        Task<IEnumerable<Book>> GetByAuthorAsync(string author);
        Task<IEnumerable<Book>> GetByGenreAsync(string genre);
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
        Task<PagedResult<Book>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
        Task<bool> IsAvailableAsync(int bookId);
        Task<bool> HasAvailableCopiesAsync(int bookId);
        Task UpdateAvailableCopiesAsync(int bookId, int changeAmount);
        Task<IEnumerable<Book>> GetOverdueBooksAsync();
        Task<bool> ISBNExistsAsync(string isbn);
        Task<Book> CreateAsync(Book book);
        new Task<Book> UpdateAsync(Book book);
    }
}