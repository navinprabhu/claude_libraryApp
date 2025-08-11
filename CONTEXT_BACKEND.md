# Backend Development Context (~400 tokens)

## Services Architecture
- Auth(5001): JWT tokens, user management
- Books(5002): CRUD, borrowing logic  
- Members(5003): User profiles, history
- Gateway(5000): Ocelot routing, rate limiting

## Development Pattern
```csharp
[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin", "Member")]  // Custom attribute
public class BooksController : ControllerBase
{
    private readonly IBookService _service;
    // Controller → Service → Repository pattern
}
```

## Key Folders
```
LibraryApp.{Service}/
├── Controllers/     # API endpoints
├── Data/           # DbContext, repositories  
├── Services/       # Business logic
└── Infrastructure/ # Health checks, middleware
```

## Development Commands
```bash
.\scripts\start-dev.ps1 -Detached  # All services
dotnet build                       # Build solution
dotnet test                        # Run tests
docker-compose up -d               # With containers
```

## Database: InMemory (dev), PostgreSQL (demo containers)
## Auth: JWT with roles, custom [AuthorizeRoles] attribute