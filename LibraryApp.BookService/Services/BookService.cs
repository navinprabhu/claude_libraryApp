using AutoMapper;
using LibraryApp.BookService.Data.Repositories;
using LibraryApp.BookService.Models.Entities;
using LibraryApp.Shared.Models.Common;
using LibraryApp.Shared.Models.DTOs;

namespace LibraryApp.BookService.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BookService> _logger;

        public BookService(
            IBookRepository bookRepository,
            IMapper mapper,
            ILogger<BookService> logger)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetAllBooksAsync()
        {
            try
            {
                var books = await _bookRepository.GetAllAsync();
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all books");
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to retrieve books", 500);
            }
        }

        public async Task<ApiResponse<BookDto>> GetBookByIdAsync(int id)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book not found", 404);
                }

                var bookDto = _mapper.Map<BookDto>(book);
                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ID: {BookId}", id);
                return ApiResponse<BookDto>.ErrorResponse("Failed to retrieve book", 500);
            }
        }

        public async Task<ApiResponse<BookDto>> GetBookByISBNAsync(string isbn)
        {
            try
            {
                var book = await _bookRepository.GetByISBNAsync(isbn);
                if (book == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book not found", 404);
                }

                var bookDto = _mapper.Map<BookDto>(book);
                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book with ISBN: {ISBN}", isbn);
                return ApiResponse<BookDto>.ErrorResponse("Failed to retrieve book", 500);
            }
        }

        public async Task<ApiResponse<PagedResult<BookDto>>> GetBooksPagedAsync(int page, int pageSize, string? searchTerm = null)
        {
            try
            {
                var pagedBooks = await _bookRepository.GetPagedAsync(page, pageSize, searchTerm);
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(pagedBooks.Items);
                
                var pagedResult = new PagedResult<BookDto>
                {
                    Items = bookDtos.ToList(),
                    TotalCount = pagedBooks.TotalCount,
                    PageNumber = pagedBooks.PageNumber,
                    PageSize = pagedBooks.PageSize
                };

                return ApiResponse<PagedResult<BookDto>>.SuccessResponse(pagedResult, "Books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged books");
                return ApiResponse<PagedResult<BookDto>>.ErrorResponse("Failed to retrieve books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> SearchBooksByTitleAsync(string title)
        {
            try
            {
                var books = await _bookRepository.GetByTitleAsync(title);
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books by title: {Title}", title);
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to search books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> SearchBooksByAuthorAsync(string author)
        {
            try
            {
                var books = await _bookRepository.GetByAuthorAsync(author);
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books by author: {Author}", author);
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to search books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetBooksByGenreAsync(string genre)
        {
            try
            {
                var books = await _bookRepository.GetByGenreAsync(genre);
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by genre: {Genre}", genre);
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to retrieve books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetAvailableBooksAsync()
        {
            try
            {
                var books = await _bookRepository.GetAvailableBooksAsync();
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Available books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available books");
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to retrieve available books", 500);
            }
        }

        public async Task<ApiResponse<BookDto>> CreateBookAsync(CreateBookDto createBookDto, string createdBy)
        {
            try
            {
                if (await _bookRepository.ISBNExistsAsync(createBookDto.ISBN))
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book with this ISBN already exists", 400);
                }

                var book = _mapper.Map<Book>(createBookDto);
                book.CreatedBy = createdBy;
                book.CreatedAt = DateTime.UtcNow;
                book.IsActive = true;
                book.AvailableCopies = book.TotalCopies;

                var createdBook = await _bookRepository.CreateAsync(book);
                var bookDto = _mapper.Map<BookDto>(createdBook);

                _logger.LogInformation("Book created successfully: {BookId} by {CreatedBy}", createdBook.Id, createdBy);
                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book: {Title}", createBookDto.Title);
                return ApiResponse<BookDto>.ErrorResponse("Failed to create book", 500);
            }
        }

        public async Task<ApiResponse<BookDto>> UpdateBookAsync(int id, UpdateBookDto updateBookDto, string updatedBy)
        {
            try
            {
                var existingBook = await _bookRepository.GetByIdAsync(id);
                if (existingBook == null)
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book not found", 404);
                }

                if (updateBookDto.ISBN != existingBook.ISBN && await _bookRepository.ISBNExistsAsync(updateBookDto.ISBN))
                {
                    return ApiResponse<BookDto>.ErrorResponse("Book with this ISBN already exists", 400);
                }

                _mapper.Map(updateBookDto, existingBook);
                existingBook.UpdatedBy = updatedBy;
                existingBook.UpdatedAt = DateTime.UtcNow;

                if (updateBookDto.TotalCopies.HasValue)
                {
                    var difference = updateBookDto.TotalCopies.Value - existingBook.TotalCopies;
                    existingBook.AvailableCopies = Math.Max(0, existingBook.AvailableCopies + difference);
                }

                await _bookRepository.UpdateAsync(existingBook);
                var bookDto = _mapper.Map<BookDto>(existingBook);

                _logger.LogInformation("Book updated successfully: {BookId} by {UpdatedBy}", id, updatedBy);
                return ApiResponse<BookDto>.SuccessResponse(bookDto, "Book updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book: {BookId}", id);
                return ApiResponse<BookDto>.ErrorResponse("Failed to update book", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteBookAsync(int id, string deletedBy)
        {
            try
            {
                var book = await _bookRepository.GetByIdAsync(id);
                if (book == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Book not found", 404);
                }

                if (book.BorrowingRecords.Any(br => !br.IsReturned))
                {
                    return ApiResponse<bool>.ErrorResponse("Cannot delete book with active borrowings", 400);
                }

                book.IsActive = false;
                book.UpdatedBy = deletedBy;
                book.UpdatedAt = DateTime.UtcNow;

                await _bookRepository.UpdateAsync(book);

                _logger.LogInformation("Book deleted successfully: {BookId} by {DeletedBy}", id, deletedBy);
                return ApiResponse<bool>.SuccessResponse(true, "Book deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book: {BookId}", id);
                return ApiResponse<bool>.ErrorResponse("Failed to delete book", 500);
            }
        }

        public async Task<ApiResponse<bool>> CheckAvailabilityAsync(int bookId)
        {
            try
            {
                var isAvailable = await _bookRepository.IsAvailableAsync(bookId);
                return ApiResponse<bool>.SuccessResponse(isAvailable, 
                    isAvailable ? "Book is available" : "Book is not available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking book availability: {BookId}", bookId);
                return ApiResponse<bool>.ErrorResponse("Failed to check book availability", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<BookDto>>> GetOverdueBooksAsync()
        {
            try
            {
                var books = await _bookRepository.GetOverdueBooksAsync();
                var bookDtos = _mapper.Map<IEnumerable<BookDto>>(books);
                return ApiResponse<IEnumerable<BookDto>>.SuccessResponse(bookDtos, "Overdue books retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue books");
                return ApiResponse<IEnumerable<BookDto>>.ErrorResponse("Failed to retrieve overdue books", 500);
            }
        }

        public async Task<ApiResponse<IEnumerable<object>>> GetBookCategoriesAsync()
        {
            try
            {
                var books = await _bookRepository.GetAllAsync();
                
                var categories = books
                    .GroupBy(b => b.Genre ?? "Uncategorized")
                    .Select(g => new
                    {
                        category = g.Key,
                        totalBooks = g.Count(),
                        availableBooks = g.Count(b => b.AvailableCopies > 0),
                        borrowedBooks = g.Count(b => b.AvailableCopies < b.TotalCopies)
                    })
                    .OrderByDescending(c => c.totalBooks)
                    .ToList();

                return ApiResponse<IEnumerable<object>>.SuccessResponse(categories, "Book categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book categories");
                return ApiResponse<IEnumerable<object>>.ErrorResponse("Failed to retrieve book categories", 500);
            }
        }

        public async Task<ApiResponse<object>> GetBookStatisticsAsync()
        {
            try
            {
                var books = await _bookRepository.GetAllAsync();
                var overdueBooks = await _bookRepository.GetOverdueBooksAsync();
                
                var statistics = new
                {
                    totalBooks = books.Count(),
                    availableBooks = books.Count(b => b.AvailableCopies > 0),
                    borrowedBooks = books.Sum(b => b.TotalCopies - b.AvailableCopies),
                    overdueBooks = overdueBooks.Count(),
                    totalCopies = books.Sum(b => b.TotalCopies),
                    availableCopies = books.Sum(b => b.AvailableCopies),
                    mostPopularGenre = books
                        .GroupBy(b => b.Genre ?? "Uncategorized")
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "N/A",
                    averageCopiesPerBook = books.Any() ? books.Average(b => b.TotalCopies) : 0
                };

                return ApiResponse<object>.SuccessResponse(statistics, "Book statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book statistics");
                return ApiResponse<object>.ErrorResponse("Failed to retrieve book statistics", 500);
            }
        }
    }
}