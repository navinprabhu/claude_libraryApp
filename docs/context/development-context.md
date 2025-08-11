# Development Context for AI Agents

> **Purpose**: Complete context for feature development and code changes

## Load These Atoms:
- `atoms/service-patterns.md` - Service structure and patterns
- `atoms/api-conventions.md` - API design and conventions
- `atoms/authentication-flow.md` - JWT and security patterns
- `reference/commands.md` - Development commands  
- `reference/ports-and-services.md` - Service topology

## When to Use This Context:
- Adding new features or endpoints
- Modifying existing services  
- Implementing business logic
- Working with authentication/authorization
- Database and repository operations

## Key Development Patterns:

### Service Structure (All Services Follow This):
```
LibraryApp.{ServiceName}/
├── Controllers/        # API endpoints with [AuthorizeRoles]
├── Data/              # DbContext, repositories (InMemory databases)
├── Infrastructure/    # Health checks, middleware, mapping
├── Models/            # Entities and request models
├── Services/          # Business logic layer
└── Program.cs         # Service configuration
```

### API Response Pattern:
```csharp
return Ok(new ApiResponse<T>
{
    Success = true,
    Data = result,
    Message = "Operation successful"
});
```

### Authentication Pattern:
```csharp
[AuthorizeRoles("Admin", "Member")]  // Custom attribute
public async Task<ActionResult<ApiResponse<BookDto>>> GetBook(int id)
```

### Repository Pattern:
```csharp
public class BookRepository : BaseRepository<Book>, IBookRepository
{
    // Inherits CRUD operations, add specific methods
}
```

### Database Pattern:
- **InMemory databases** for all .NET services
- **PostgreSQL containers** for demo/infrastructure only  
- **One database per service** (microservices pattern)

## Service Dependencies:
```
AuthService: Standalone (issues JWT tokens)
BookService: → AuthService (validates JWT)
MemberService: → AuthService + BookService
ApiGateway: → All services (routes requests)
```

## Development Workflow:
```bash
# 1. Local development setup
.\scripts\start-dev.ps1 -Detached

# 2. Make code changes

# 3. Test locally
dotnet build
dotnet test

# 4. Test with Docker
docker-compose up -d --build

# 5. Integration tests
dotnet test --filter "Category=Integration"
```

## Recipe References:
- Add new endpoint: `recipes/add-api-endpoint.md`
- Add new service: `recipes/add-new-service.md`  
- Debug service communication: `recipes/debug-service-communication.md`

## Current Service Ports:
```
API Gateway: 5000    Auth Service: 5001
Book Service: 5002   Member Service: 5003
Auth DB: 5432        Book DB: 5433
Member DB: 5434      Redis: 6379
```

## Authentication Flow:
1. Client → `/api/auth/login` → Get JWT token
2. Client → API requests with `Authorization: Bearer {token}`
3. API Gateway validates JWT → Routes to service
4. Target service validates JWT → Processes request

## Common Request/Response Patterns:

### Create Resource:
```csharp
[HttpPost]
[AuthorizeRoles("Admin")]
public async Task<ActionResult<ApiResponse<BookDto>>> CreateBook([FromBody] CreateBookRequest request)
```

### Get Resources with Pagination:
```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<PagedResult<BookDto>>>> GetBooks(
    [FromQuery] int page = 1, [FromQuery] int size = 10)
```

### User-Specific Operations:
```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var userBooks = await _service.GetUserBooksAsync(userId);
```

## Testing Patterns:

### Unit Tests:
```csharp
[Fact]
public async Task CreateBook_ValidRequest_ReturnsBookDto()
```

### Integration Tests:
```csharp
[Trait("Category", "Integration")]  // Required for CI discovery!
public class BookApiIntegrationTests
```

## Configuration Patterns:
- Environment variables for sensitive data
- `appsettings.json` for structure  
- Docker environment variable injection
- Service URLs via configuration: `ServiceUrls__AuthService`