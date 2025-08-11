# Health Check Patterns

## Health Check Architecture

### Self-Contained Health Checks (Recommended)
```csharp
public class MemberServiceHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Self-contained check - no external dependencies
            var data = new Dictionary<string, object>
            {
                ["timestamp"] = DateTimeOffset.UtcNow,
                ["service"] = "LibraryApp.MemberService",
                ["database"] = "InMemory",  // InMemory is always "healthy"
                ["status"] = "Ready"
            };
            return Task.FromResult(HealthCheckResult.Healthy("Member service is healthy", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Member service is unhealthy", ex));
        }
    }
}
```

### ❌ Avoid Circular Dependencies  
```csharp
// DON'T DO THIS - causes cascading failures
public class MemberServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(...)
    {
        // This can fail if BookService is starting up
        var bookServiceHealth = await _bookServiceClient.GetHealthAsync();
        return bookServiceHealth.IsHealthy ? Healthy() : Unhealthy();
    }
}
```

## Docker Health Check Configuration

### Service Health Checks (docker-compose.yml)
```yaml
services:
  auth-service:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 45s      # Time between checks
      timeout: 20s       # Max time for single check
      retries: 6         # Number of retries before marking unhealthy
      start_period: 180s # Grace period during startup
      
  api-gateway:
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health/simple"]  # Special!
      interval: 45s
      timeout: 20s
      retries: 6
      start_period: 360s  # Longer startup - depends on other services
```

### Database Health Checks
```yaml
  auth-db:
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U auth_user -d AuthDatabase"]
      interval: 30s
      timeout: 15s
      retries: 5
      start_period: 120s
      
  redis:
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 30s
      timeout: 15s
      retries: 5
      start_period: 60s
```

## API Gateway Special Case

### The Ocelot Routing Problem
API Gateway has two health endpoints:
- `/health`: Standard ASP.NET health checks (processed by Ocelot)
- `/health/simple`: Custom middleware (bypasses Ocelot)

### Why `/health/simple` Exists
```csharp
// Problem: Ocelot tries to route /health through its configuration
// Solution: Handle /health/simple BEFORE Ocelot middleware

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health/simple" && context.Request.Method == "GET")
    {
        // Bypass Ocelot entirely
        context.Response.ContentType = "application/json";
        var response = new { status = "healthy", timestamp = DateTime.UtcNow };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        return; // Don't call next() - short circuit
    }
    await next();
});

// ... later in pipeline
await app.UseOcelot(); // This won't see /health/simple requests
```

## Health Check Endpoints

### Individual Service Health
```http
GET http://localhost:5001/health  # Auth Service
GET http://localhost:5002/health  # Book Service  
GET http://localhost:5003/health  # Member Service
GET http://localhost:5000/health/simple  # API Gateway (Docker)
GET http://localhost:5000/health  # API Gateway (full health checks)
```

### Response Format
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "AuthService": {
      "data": {
        "timestamp": "2023-07-20T10:30:00Z",
        "service": "LibraryApp.AuthService",  
        "database": "InMemory",
        "status": "Ready"
      },
      "duration": "00:00:00.0012345",
      "status": "Healthy"
    }
  }
}
```

## Health Check Registration Pattern
```csharp
// In Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<AuthServiceHealthCheck>("AuthService")
    .AddCheck<DatabaseHealthCheck>("Database");

// Configure endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## CI/CD Health Check Integration
```yaml
# Wait for all services to be healthy before running tests
- name: Check service health
  run: |
    for service in auth book member gateway; do
      endpoint=$(case $service in
        gateway) echo "/health/simple" ;;  # Special case!
        *) echo "/health" ;;
      esac)
      
      for i in {1..10}; do
        if curl -f http://localhost:${port}${endpoint}; then
          echo "✅ $service healthy"
          break
        fi
        sleep 10
      done
    done
```

## Troubleshooting Health Checks

### Service Won't Start
1. Check Docker logs: `docker logs libraryapp-{service-name}`
2. Check dependency services are healthy first
3. Verify health check endpoint responds manually
4. Check for port conflicts or binding issues

### Health Check Timeouts  
1. Increase `start_period` for slow-starting services
2. Check if service is doing heavy initialization
3. Verify database migrations aren't blocking startup

### Intermittent Failures
1. Check resource constraints (CPU, memory)
2. Look for circular dependency health checks
3. Verify network connectivity between containers