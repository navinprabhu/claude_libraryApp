# API Conventions

## Controller Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin", "Member")]  // Custom authorization
public class {Entity}Controller : ControllerBase
{
    private readonly I{Entity}Service _{entity}Service;
    private readonly ILogger<{Entity}Controller> _logger;
    
    // Constructor injection pattern
}
```

## Endpoint Conventions
- **GET** `/api/{entities}` - List entities (with pagination)
- **GET** `/api/{entities}/{id}` - Get single entity
- **POST** `/api/{entities}` - Create entity
- **PUT** `/api/{entities}/{id}` - Update entity  
- **DELETE** `/api/{entities}/{id}` - Delete entity

## Authentication & Authorization
- **JWT Bearer tokens** with HS256 signing
- **Custom attribute**: `[AuthorizeRoles("Admin", "Member")]`
- **Claims-based**: User ID, username, role in JWT
- **Middleware**: `JwtAuthenticationMiddleware` in each service

## Request/Response Patterns
### Successful Response
```csharp
return Ok(new ApiResponse<BookDto>
{
    Success = true,
    Data = bookDto,
    Message = "Book retrieved successfully"
});
```

### Error Response  
```csharp
return BadRequest(new ApiResponse<object>
{
    Success = false,
    Message = "Validation failed",
    Errors = ["Book title is required", "ISBN must be valid"]
});
```

### Pagination Pattern
```csharp
public async Task<ActionResult<ApiResponse<PagedResult<BookDto>>>> GetBooks(
    [FromQuery] int page = 1, 
    [FromQuery] int size = 10)
{
    var result = await _bookService.GetBooksAsync(page, size);
    return Ok(new ApiResponse<PagedResult<BookDto>>
    {
        Success = true,
        Data = result
    });
}
```

## Service Routes (via API Gateway)
```
Frontend → ApiGateway:5000/api/auth/* → AuthService:5001
Frontend → ApiGateway:5000/api/books/* → BookService:5002  
Frontend → ApiGateway:5000/api/members/* → MemberService:5003
```

## Inter-Service Communication
- **HTTP clients** with `HttpClientFactory`
- **Service URLs** via configuration: `ServiceUrls__AuthService=http://auth-service:5001`
- **Resilience patterns** with Polly (circuit breaker, retry)
- **Correlation IDs** for request tracing

## Validation Patterns
- **Data annotations** on request models
- **FluentValidation** for complex validation rules
- **Model state validation** in controllers
- **Business rule validation** in service layer

## Example: Complete CRUD Controller
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<PagedResult<BookDto>>>> GetBooks(
    [FromQuery] int page = 1, [FromQuery] int size = 10)
{
    var books = await _bookService.GetBooksAsync(page, size);
    return Ok(new ApiResponse<PagedResult<BookDto>>
    {
        Success = true,
        Data = books,
        Message = $"Retrieved {books.Items.Count} books"
    });
}

[HttpPost]
[AuthorizeRoles("Admin")]
public async Task<ActionResult<ApiResponse<BookDto>>> CreateBook([FromBody] CreateBookRequest request)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Validation failed",
            Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
        });
    }
    
    var book = await _bookService.CreateBookAsync(request);
    return CreatedAtAction(nameof(GetBook), new { id = book.Id }, 
        new ApiResponse<BookDto>
        {
            Success = true,
            Data = book,
            Message = "Book created successfully"
        });
}
```