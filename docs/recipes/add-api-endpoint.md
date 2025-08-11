# Recipe: Add New API Endpoint

> **Context**: Load `atoms/api-conventions.md` + `atoms/service-patterns.md` + `atoms/authentication-flow.md`

## Step-by-Step Guide

### 1. Choose Target Service
Determine which service should handle the endpoint based on domain:
- **Auth operations** → `LibraryApp.AuthService`
- **Book/borrowing operations** → `LibraryApp.BookService`  
- **Member operations** → `LibraryApp.MemberService`

### 2. Create Request/Response Models

```csharp
// In Models/Requests/
public class CreateBookRequest
{
    [Required]
    public string Title { get; set; }
    
    [Required]
    public string Author { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{10}(\d{3})?$", ErrorMessage = "Invalid ISBN format")]
    public string ISBN { get; set; }
    
    public string Description { get; set; }
    public int CategoryId { get; set; }
}

// Response uses existing DTOs from LibraryApp.Shared.Models
// e.g., BookDto, MemberDto, etc.
```

### 3. Add Repository Method (if needed)

```csharp
// In Data/Repositories/IBookRepository.cs
public interface IBookRepository : IRepository<Book>
{
    Task<Book> GetByISBNAsync(string isbn);
    Task<IEnumerable<Book>> GetByAuthorAsync(string author);
    Task<bool> IsISBNUniqueAsync(string isbn);
}

// In Data/Repositories/BookRepository.cs
public class BookRepository : BaseRepository<Book>, IBookRepository
{
    public async Task<Book> GetByISBNAsync(string isbn)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
    }
    
    public async Task<bool> IsISBNUniqueAsync(string isbn)
    {
        return !await _context.Books.AnyAsync(b => b.ISBN == isbn);
    }
}
```

### 4. Add Service Method

```csharp
// In Services/IBookService.cs
public interface IBookService
{
    Task<BookDto> CreateBookAsync(CreateBookRequest request);
    Task<BookDto> GetBookByISBNAsync(string isbn);
}

// In Services/BookService.cs
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;

    public async Task<BookDto> CreateBookAsync(CreateBookRequest request)
    {
        // Validation
        if (!await _bookRepository.IsISBNUniqueAsync(request.ISBN))
        {
            throw new BusinessException("Book with this ISBN already exists");
        }

        // Create entity
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Status = BookStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        // Save
        var createdBook = await _bookRepository.AddAsync(book);
        await _bookRepository.SaveChangesAsync();

        // Return DTO
        return _mapper.Map<BookDto>(createdBook);
    }
}
```

### 5. Add Controller Endpoint

```csharp
// In Controllers/BooksController.cs
[HttpPost]
[AuthorizeRoles("Admin")]  // Only admins can create books
public async Task<ActionResult<ApiResponse<BookDto>>> CreateBook([FromBody] CreateBookRequest request)
{
    try
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        var book = await _bookService.CreateBookAsync(request);
        
        return CreatedAtAction(
            nameof(GetBook), 
            new { id = book.Id }, 
            new ApiResponse<BookDto>
            {
                Success = true,
                Data = book,
                Message = "Book created successfully"
            });
    }
    catch (BusinessException ex)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = ex.Message
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating book");
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred while creating the book"
        });
    }
}

[HttpGet("isbn/{isbn}")]
[AuthorizeRoles("Admin", "Member")]
public async Task<ActionResult<ApiResponse<BookDto>>> GetBookByISBN(string isbn)
{
    var book = await _bookService.GetBookByISBNAsync(isbn);
    if (book == null)
    {
        return NotFound(new ApiResponse<object>
        {
            Success = false,
            Message = "Book not found"
        });
    }

    return Ok(new ApiResponse<BookDto>
    {
        Success = true,
        Data = book,
        Message = "Book retrieved successfully"
    });
}
```

### 6. Add AutoMapper Configuration

```csharp
// In Infrastructure/Mapping/MappingProfile.cs
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateBookRequest, Book>();
        // Add other mappings as needed
    }
}
```

### 7. Register Dependencies (if needed)

```csharp
// In Program.cs (usually already configured)
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
```

### 8. Update API Gateway Routes (if needed)

Check `LibraryApp.ApiGateway/Configuration/ocelot.json`:
```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/books/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "book-service", "Port": 5002 }
      ],
      "UpstreamPathTemplate": "/api/books/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ]
}
```

### 9. Add Unit Tests

```csharp
// In LibraryApp.Tests/Services/BookServiceTests.cs
[Fact]
public async Task CreateBookAsync_WithValidRequest_ReturnsBookDto()
{
    // Arrange
    var request = new CreateBookRequest
    {
        Title = "Test Book",
        Author = "Test Author", 
        ISBN = "1234567890123"
    };
    
    _mockRepository.Setup(r => r.IsISBNUniqueAsync(request.ISBN))
                  .ReturnsAsync(true);
    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Book>()))
                  .ReturnsAsync(new Book { Id = 1, Title = request.Title });

    // Act
    var result = await _bookService.CreateBookAsync(request);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(request.Title, result.Title);
}

[Fact]  
public async Task CreateBookAsync_WithDuplicateISBN_ThrowsBusinessException()
{
    // Arrange
    var request = new CreateBookRequest { ISBN = "1234567890123" };
    _mockRepository.Setup(r => r.IsISBNUniqueAsync(request.ISBN))
                  .ReturnsAsync(false);

    // Act & Assert
    await Assert.ThrowsAsync<BusinessException>(() => 
        _bookService.CreateBookAsync(request));
}
```

### 10. Add Integration Tests

```csharp
// In LibraryApp.Tests/Integration/BookApiTests.cs
[Trait("Category", "Integration")]  // Important for CI!
public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateBook_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateBookRequest
        {
            Title = "Integration Test Book",
            Author = "Test Author",
            ISBN = "9876543210987"
        };
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/books", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### 11. Test the Endpoint

```bash
# Local testing
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Book",
    "author": "Test Author", 
    "isbn": "1234567890123",
    "description": "A test book"
  }'

# Test via API Gateway
curl -X GET http://localhost:5000/api/books/isbn/1234567890123 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Authorization Patterns

### Public Endpoints (No Auth Required)
```csharp
[HttpGet("public/search")]
[AllowAnonymous]
public async Task<ActionResult<ApiResponse<IEnumerable<BookDto>>>> SearchBooks(string query)
```

### Role-Based Access
```csharp
[AuthorizeRoles("Admin")]           // Admin only
[AuthorizeRoles("Admin", "Member")] // Admin or Member
[AuthorizeRoles("Member")]          // Member only
```

### User-Specific Access
```csharp
[HttpGet("my-borrowed-books")]
[AuthorizeRoles("Member")]
public async Task<ActionResult<ApiResponse<IEnumerable<BookDto>>>> GetMyBorrowedBooks()
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var books = await _bookService.GetBorrowedBooksByUserIdAsync(userId);
    // ...
}
```

## Common Patterns

### Pagination
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<PagedResult<BookDto>>>> GetBooks(
    [FromQuery] int page = 1,
    [FromQuery] int size = 10)
{
    var books = await _bookService.GetBooksAsync(page, size);
    return Ok(new ApiResponse<PagedResult<BookDto>>
    {
        Success = true,
        Data = books
    });
}
```

### Search/Filtering
```csharp
[HttpGet("search")]
public async Task<ActionResult<ApiResponse<IEnumerable<BookDto>>>> SearchBooks(
    [FromQuery] string title,
    [FromQuery] string author,
    [FromQuery] int? categoryId)
{
    var books = await _bookService.SearchBooksAsync(title, author, categoryId);
    // ...
}
```