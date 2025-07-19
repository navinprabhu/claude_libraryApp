using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Infrastructure.Repositories;
using LibraryApp.Shared.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.BookService.Data.Repositories
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        private new readonly BookDbContext _context;

        public BookRepository(BookDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Book?> GetByISBNAsync(string isbn)
        {
            return await _context.Books
                .Include(b => b.BorrowingRecords)
                .FirstOrDefaultAsync(b => b.ISBN == isbn && b.IsActive);
        }

        public async Task<IEnumerable<Book>> GetByTitleAsync(string title)
        {
            return await _context.Books
                .Where(b => b.Title.Contains(title) && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByAuthorAsync(string author)
        {
            return await _context.Books
                .Where(b => b.Author.Contains(author) && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetByGenreAsync(string genre)
        {
            return await _context.Books
                .Where(b => b.Genre != null && b.Genre.Contains(genre) && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _context.Books
                .Where(b => b.AvailableCopies > 0 && b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<PagedResult<Book>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            var query = _context.Books.Where(b => b.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => 
                    b.Title.Contains(searchTerm) ||
                    b.Author.Contains(searchTerm) ||
                    b.ISBN.Contains(searchTerm) ||
                    (b.Genre != null && b.Genre.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Book>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<bool> IsAvailableAsync(int bookId)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == bookId && b.IsActive);
            
            return book != null && book.AvailableCopies > 0;
        }

        public async Task<bool> HasAvailableCopiesAsync(int bookId)
        {
            return await IsAvailableAsync(bookId);
        }

        public async Task UpdateAvailableCopiesAsync(int bookId, int changeAmount)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book != null)
            {
                book.AvailableCopies += changeAmount;
                book.UpdatedAt = DateTime.UtcNow;
                
                if (book.AvailableCopies < 0)
                    book.AvailableCopies = 0;
                
                if (book.AvailableCopies > book.TotalCopies)
                    book.AvailableCopies = book.TotalCopies;

                book.Status = book.AvailableCopies > 0 
                    ? LibraryApp.Shared.Models.Enums.BookStatus.Available 
                    : LibraryApp.Shared.Models.Enums.BookStatus.Borrowed;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Book>> GetOverdueBooksAsync()
        {
            var currentDate = DateTime.UtcNow;
            
            return await _context.Books
                .Include(b => b.BorrowingRecords)
                .Where(b => b.BorrowingRecords.Any(br => 
                    !br.IsReturned && br.DueDate < currentDate))
                .ToListAsync();
        }

        public async Task<bool> ISBNExistsAsync(string isbn)
        {
            return await _context.Books
                .AnyAsync(b => b.ISBN == isbn);
        }

        public override async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.BorrowingRecords)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        }

        public override async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books
                .Where(b => b.IsActive)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Book> CreateAsync(Book book)
        {
            return await AddAsync(book);
        }

        public new async Task<Book> UpdateAsync(Book entity)
        {
            await base.UpdateAsync(entity);
            return entity;
        }
    }
}