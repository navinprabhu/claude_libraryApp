# Service Patterns

## Service Structure Pattern
All LibraryApp services follow consistent structure:

```
LibraryApp.{ServiceName}/
├── Controllers/           # API endpoints
├── Data/                 # DbContext and repositories  
├── Infrastructure/       # Cross-cutting concerns
│   ├── Authorization/    # Custom auth attributes
│   ├── Health/          # Health check implementations
│   ├── Mapping/         # AutoMapper profiles
│   └── Middleware/      # Custom middleware
├── Models/              # Entities and DTOs
│   ├── Entities/        # Database entities
│   └── Requests/        # API request models
├── Services/            # Business logic
└── Program.cs           # Service configuration

```

## Service Dependencies
- **AuthService**: Standalone (JWT issuer)
- **BookService**: → AuthService (JWT validation)
- **MemberService**: → AuthService + BookService
- **ApiGateway**: → All services (routing)

## Database Pattern
- **One database per service** (database-per-service pattern)
- **InMemory databases** for .NET services (.UseInMemoryDatabase())
- **PostgreSQL containers** for demo/infrastructure only
- **Repository pattern**: `IRepository<T>` with `BaseRepository<T>`

## Health Check Pattern
```csharp
public class {ServiceName}HealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Self-contained check - no external service dependencies
        // For InMemory databases, just verify context availability
        var data = new Dictionary<string, object>
        {
            ["timestamp"] = DateTimeOffset.UtcNow,
            ["service"] = "LibraryApp.{ServiceName}",
            ["database"] = "InMemory",
            ["status"] = "Ready"
        };
        return Task.FromResult(HealthCheckResult.Healthy("{ServiceName} is healthy", data));
    }
}
```

## Service Registration Pattern
```csharp
// Program.cs
builder.Services.AddDbContext<{ServiceName}DbContext>(options =>
    options.UseInMemoryDatabase("{ServiceName}Db"));
    
builder.Services.AddScoped<I{ServiceName}Repository, {ServiceName}Repository>();
builder.Services.AddScoped<I{ServiceName}Service, {ServiceName}Service>();
builder.Services.AddHealthChecks()
    .AddCheck<{ServiceName}HealthCheck>("{ServiceName}");
```

## API Response Pattern
All services use standardized `ApiResponse<T>`:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}
```

## Error Handling Pattern
Global exception middleware in each service:
```csharp
public class GlobalExceptionHandlingMiddleware
{
    // Returns standardized ApiResponse<T> for all errors
    // Logs with correlation IDs
    // Maps different exception types to appropriate HTTP status codes
}
```