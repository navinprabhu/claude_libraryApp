# CI Pipeline Patterns

## Pipeline Structure (.github/workflows/ci.yml)

```yaml
jobs:
  changes          # Detect what changed, set build matrix
  ↓
  build-and-test   # Build .NET, run unit tests (depends: changes)  
  ↓
  security-scan    # .NET security analysis (depends: changes)
  ↓
  docker-build     # Build Docker images (depends: build-and-test)
  ↓  
  integration-tests # Test with Docker Compose (depends: docker-build)
  ↓
  ci-summary       # Final status check (depends: all above)
```

## Matrix Build Strategy
- **Detect changes**: Auth, Book, Member, Gateway services
- **Build only changed services** + dependencies
- **Shared changes trigger all services**

## Testing Pipeline
```yaml
Unit Tests:
- Run with: dotnet test --logger trx --collect:"XPlat Code Coverage"
- Filter: No filter (all unit tests)
- Results: TestResults/*.trx

Integration Tests:  
- Run with: dotnet test --filter "Category=Integration"
- Requires: [Trait("Category", "Integration")] on test classes
- Results: IntegrationTestResults/*.trx
- Dependencies: Docker services must be healthy
```

## Docker Health Checks
Services must pass health checks before integration tests:

```yaml
Database Health Checks:
- auth-db, book-db, member-db, redis
- Check: Container status and pg_isready/redis-cli ping

Service Health Checks:
- Auth Service: curl http://localhost:5001/health
- Book Service: curl http://localhost:5002/health  
- Member Service: curl http://localhost:5003/health
- API Gateway: curl http://localhost:5000/health/simple  # Special endpoint!
```

## Critical Health Check Pattern
**API Gateway uses `/health/simple` not `/health`**:
- `/health/simple`: Custom middleware, bypasses Ocelot routing
- `/health`: Standard ASP.NET health checks, processed by Ocelot

```csharp
// In API Gateway Program.cs - BEFORE Ocelot
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health/simple" && context.Request.Method == "GET")
    {
        context.Response.ContentType = "application/json";
        var response = new { status = "healthy", timestamp = DateTime.UtcNow };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        return;
    }
    await next();
});
```

## Container Naming Convention
CI scripts use specific container name mapping:
```bash
auth → libraryapp-auth-service
book → libraryapp-book-service  
member → libraryapp-member-service
gateway → libraryapp-api-gateway
```

## Security Scanning
- **Dependency vulnerability scan**: `dotnet list package --vulnerable`
- **Code pattern analysis**: Check for hardcoded secrets, SQL injection
- **Docker image scanning**: Trivy with table output (not SARIF)
- **CodeQL**: Separate workflow, runs on schedule/main branch only

## Common CI Failures & Fixes

### "No file matches IntegrationTestResults/*.trx"
**Fix**: Add `[Trait("Category", "Integration")]` to integration test classes

### "Container unhealthy" 
**Fix**: Check health check endpoints, increase timeouts in docker-compose.yml

### "Gateway service failed to become healthy"
**Fix**: Ensure CI uses `/health/simple` endpoint for API Gateway

### Build failures
**Fix**: Run `dotnet build` locally first, check for compilation errors

## Pipeline Configuration
```yaml
Environment Variables:
- DOTNET_VERSION: '8.0.x'
- ASPNETCORE_ENVIRONMENT: Development (for Docker services)

Timeouts:
- Build jobs: 10 minutes
- Integration tests: 15 minutes  
- Docker health checks: 240 seconds startup + 100 seconds checks

Cache Strategy:
- NuGet packages: ~/.nuget/packages
- Docker layers: /tmp/.buildx-cache
```

## Job Dependencies
Critical sequencing to prevent race conditions:
```yaml
integration-tests:
  needs: [changes, build-and-test, docker-build]  # Must wait for Docker builds!
```