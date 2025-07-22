# LibraryApp - Microservices Development Context

This file contains essential context and information for working with the LibraryApp microservices project. It ensures consistent development patterns and provides quick reference for common operations.

## Project Overview

**LibraryApp** is a modern .NET 8 microservices-based library management system with:
- **4 Core Services**: Authentication, Book Management, Member Management, API Gateway
- **Docker-first architecture** with PostgreSQL databases and Redis cache
- **JWT-based authentication** with role-based access control
- **Ocelot API Gateway** for service orchestration and rate limiting
- **Comprehensive CI/CD pipeline** with GitHub Actions

## Architecture & Service Structure

### Services & Ports
```
API Gateway (Ocelot):     5000  → http://localhost:5000
Auth Service:             5001  → http://localhost:5001
Book Service:             5002  → http://localhost:5002
Member Service:           5003  → http://localhost:5003

Databases:
Auth Database:            5432  → localhost:5432
Book Database:            5433  → localhost:5433  
Member Database:          5434  → localhost:5434
Redis Cache:              6379  → localhost:6379
```

### Service Dependencies
```
API Gateway → All Services (routing)
Book Service → Auth Service (JWT validation)
Member Service → Auth Service (JWT validation)
Member Service → Book Service (borrowing info)
All Services → Redis (caching)
```

### Solution Structure
```
LibraryApp.sln
├── LibraryApp.AuthService/         # JWT authentication & user management
├── LibraryApp.BookService/         # Book CRUD & borrowing workflows
├── LibraryApp.MemberService/       # Member management & borrowing history
├── LibraryApp.ApiGateway/          # Ocelot gateway with rate limiting
├── LibraryApp.Shared.Models/       # Common DTOs & entities
├── LibraryApp.Shared.Infrastructure/ # Common utilities & services
├── LibraryApp.Shared.Events/       # Event models for inter-service communication
└── LibraryApp.Tests/              # Unit & integration tests
```

## Development Workflows

### Environment Setup
```powershell
# Initial setup (run once)
.\scripts\setup-local-dev.ps1

# Build all services
.\scripts\build-all.ps1

# Start development environment
.\scripts\start-dev.ps1 -Detached

# View logs
.\scripts\logs.ps1

# Stop all services
.\scripts\stop-all.ps1
```

### Common Development Commands
```powershell
# .NET Build & Test
dotnet build                                    # Build all projects
dotnet test                                     # Run all tests
dotnet test --collect:"XPlat Code Coverage"    # Run tests with coverage
dotnet run --project LibraryApp.AuthService    # Run specific service

# Docker Development
docker-compose up -d                            # Start all services detached
docker-compose down                             # Stop all services
docker-compose logs -f auth-service             # Follow logs for specific service
docker-compose build                            # Rebuild all images
docker-compose build --no-cache                 # Clean rebuild

# Database Operations
docker exec -it libraryapp-auth-db psql -U auth_user -d AuthDatabase
docker exec -it libraryapp-book-db psql -U book_user -d BookDatabase
docker exec -it libraryapp-member-db psql -U member_user -d MemberDatabase
```

## Key Technologies & Patterns

### Framework Stack
- **.NET 8.0**: Core framework with minimal APIs
- **Entity Framework Core**: PostgreSQL data access with migrations
- **AutoMapper**: DTO mapping with `MappingProfile` classes
- **Serilog**: Structured logging with file/console outputs
- **xUnit + Moq**: Unit testing framework
- **Polly**: Resilience patterns for HTTP calls

### Authentication & Authorization
- **JWT Bearer tokens** with HS256 signing
- **Role-based access**: `Admin`, `Member` roles
- **Custom attributes**: `AuthorizeRolesAttribute` for endpoints
- **Token validation middleware**: `JwtAuthenticationMiddleware`

### Inter-Service Communication
- **HTTP clients** with `HttpClientFactory`
- **Service discovery**: Configuration-based URLs
- **Resilience patterns**: Circuit breaker, retry policies
- **Correlation IDs**: Request tracing across services

### Database Patterns
- **Database-per-service**: Each service has dedicated PostgreSQL database
- **Repository pattern**: `IRepository<T>` with `BaseRepository<T>`
- **Unit of Work**: DbContext-based transaction management
- **Migrations**: EF Core code-first migrations

## Configuration Management

### Environment Variables
```bash
# Database connections
AUTH_DB_NAME=AuthDatabase
AUTH_DB_USER=auth_user
AUTH_DB_PASSWORD=auth_password123

# JWT Settings
JWT_SECRET_KEY=YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789
JWT_ISSUER=LibraryApp.AuthService
JWT_AUDIENCE=LibraryApp.ApiClients
JWT_EXPIRY_MINUTES=60

# Redis
REDIS_PASSWORD=redis_password123

# Service URLs (internal Docker network)
ServiceUrls__AuthService=http://auth-service:5001
ServiceUrls__BookService=http://book-service:5002
ServiceUrls__MemberService=http://member-service:5003
```

### Ocelot Gateway Configuration
```json
Routes:
- /api/auth/{everything} → auth-service:5001
- /api/books/{everything} → book-service:5002  
- /api/members/{everything} → member-service:5003

Rate Limits:
- Auth: 100 requests/minute
- Books: 200 requests/minute
- Members: 150 requests/minute

Health Checks:
- /health/services → Aggregated health from all services
```

## API Endpoints & Usage

### Authentication Service (5001)
```http
POST /api/auth/login          # Get JWT token
POST /api/auth/validate       # Validate token
POST /api/auth/refresh        # Refresh token
GET  /api/auth/userinfo       # Get user details
```

### Book Service (5002)
```http
GET    /api/books             # List all books
GET    /api/books/{id}        # Get book details
POST   /api/books             # Create book [Admin]
PUT    /api/books/{id}        # Update book [Admin]  
DELETE /api/books/{id}        # Delete book [Admin]
POST   /api/books/{id}/borrow # Borrow book
POST   /api/books/{id}/return # Return book
GET    /api/books/{id}/history # Borrowing history
```

### Member Service (5003)
```http
GET  /api/members               # List members [Admin]
GET  /api/members/{id}          # Get member details
POST /api/members               # Register member
PUT  /api/members/{id}          # Update member
GET  /api/members/{id}/borrowed-books  # Current borrowed books
GET  /api/members/{id}/history         # Borrowing history
```

### Example API Usage
```bash
# Login to get token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Use token for authenticated requests
curl -X GET http://localhost:5000/api/books \
  -H "Authorization: Bearer {your-jwt-token}"

# Create a book (Admin only)
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer {admin-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{"title": "Sample Book", "author": "Author Name", "isbn": "1234567890"}'
```

## Testing Strategy

### Unit Tests
```powershell
# Run all tests
dotnet test

# Run specific service tests
dotnet test LibraryApp.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests
- **TestContainers**: Spin up PostgreSQL for integration tests
- **WebApplicationFactory**: In-memory testing with real HTTP calls
- **Fixture pattern**: Shared test data and setup

### Health Checks
```bash
# Individual service health
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Books  
curl http://localhost:5003/health  # Members

# Gateway health checks
curl http://localhost:5000/health/gateway   # Gateway only
curl http://localhost:5000/health/services  # All services aggregated
```

## Troubleshooting & Debugging

### Common Issues

**Docker connectivity problems:**
```powershell
docker context use default          # Switch to default context
docker-compose down --remove-orphans # Clean up containers
.\scripts\build-all.ps1 -Clean      # Clean rebuild
```

**Database connection issues:**
```powershell
# Check if databases are running
docker-compose ps

# Connect to database manually
docker exec -it libraryapp-auth-db psql -U auth_user -d AuthDatabase

# Reset database volumes
docker-compose down -v
docker-compose up -d
```

**Service startup issues:**
```powershell
# View service logs
docker-compose logs auth-service
docker-compose logs book-service
docker-compose logs member-service

# Check health endpoints
curl http://localhost:5000/health/services
```

### Logging & Monitoring
- **Structured logging** with Serilog in JSON format
- **Correlation IDs** for request tracing across services  
- **Log files** stored in `logs/` directory per service
- **Health checks** available at `/health` endpoints

## Code Patterns & Standards

### Controller Patterns
```csharp
[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin", "Member")]  // Custom authorization attribute
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;
    // Constructor injection pattern
}
```

### Service Layer Pattern
```csharp
public interface IBookService
{
    Task<BookDto> GetByIdAsync(int id);
    Task<PagedResult<BookDto>> GetAllAsync(int page, int size);
    // Async/await pattern with DTOs
}
```

### Repository Pattern
```csharp
public class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(BookDbContext context) : base(context) { }
    // Inherits CRUD operations from BaseRepository
}
```

### Error Handling
```csharp
// Global exception handling middleware
public class GlobalExceptionHandlingMiddleware
{
    // Catches exceptions and returns standardized ApiResponse<T>
}

// Standardized API responses
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}
```

## Development Best Practices

### When Adding New Features:
1. **Add to appropriate service** based on domain boundaries
2. **Create DTOs** in `LibraryApp.Shared.Models` for inter-service communication
3. **Add repository methods** following existing patterns
4. **Implement service layer** with business logic
5. **Add controller endpoints** with proper authorization
6. **Write unit tests** for new functionality
7. **Update API documentation** and health checks

### When Modifying Database Schema:
1. **Add EF Core migration**: `dotnet ef migrations add MigrationName`
2. **Update seed data** in `scripts/db-seed/` if needed
3. **Test with clean database**: `docker-compose down -v && docker-compose up -d`

### When Adding Dependencies Between Services:
1. **Add service client** in `LibraryApp.Shared.Infrastructure`
2. **Configure HTTP client** with Polly resilience policies
3. **Add correlation ID** passing for request tracing
4. **Handle service unavailability** gracefully

## Important File Locations

### Configuration Files
- `docker-compose.yml` - Main Docker orchestration
- `docker-compose.override.yml` - Development overrides
- `LibraryApp.ApiGateway/Configuration/ocelot.json` - Gateway routing
- `appsettings.Development.json` - Per-service development settings

### Scripts
- `scripts/build-all.ps1` - Build all Docker images
- `scripts/start-dev.ps1` - Start development environment  
- `scripts/setup-local-dev.ps1` - Initial environment setup
- `scripts/logs.ps1` - View aggregated logs
- `scripts/clean.ps1` - Clean up containers and volumes

### Database Initialization
- `scripts/db-init/*/01-init-*-db.sql` - Database schema creation
- `scripts/db-seed/*/02-seed-*-data.sql` - Initial data seeding

---

## Quick Reference Commands

```powershell
# Start everything
.\scripts\start-dev.ps1 -Detached -Build

# View logs  
.\scripts\logs.ps1

# Run tests
dotnet test

# Clean restart
docker-compose down -v && .\scripts\build-all.ps1 -Clean && .\scripts\start-dev.ps1 -Detached

# Health check
curl http://localhost:5000/health/services
```

This context file should provide all necessary information for effective development and maintenance of the LibraryApp microservices system.