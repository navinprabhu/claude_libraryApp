# Recipe: Fix CI Pipeline Issues

> **Context**: Load `atoms/ci-pipeline.md` + `atoms/health-checks.md` + `reference/commands.md`

## Common CI Failures & Solutions

### 1. "No file matches IntegrationTestResults/*.trx"

**Symptoms:**
```
No test result files matching IntegrationTestResults/*.trx were found
```

**Root Cause:** Integration tests not properly tagged or discovered

**Fix:**
```csharp
// Add to integration test classes:
[Trait("Category", "Integration")]
public class InterServiceCommunicationTests
{
    // test methods...
}
```

**Verification:**
```bash
dotnet test --list-tests --filter "Category=Integration"
```

### 2. "Container [service-name] is unhealthy" 

**Symptoms:**
```
Container libraryapp-member-service is unhealthy
Container libraryapp-api-gateway is unhealthy
```

**Root Causes & Fixes:**

#### A. Service Health Check Issues
```bash
# Check container logs
docker logs libraryapp-member-service --tail 30

# Common issue: Circular health check dependencies
# Fix: Make health checks self-contained (see atoms/health-checks.md)
```

#### B. API Gateway Health Check Using Wrong Endpoint
```yaml
# CI script should use /health/simple for gateway
health_endpoint=$(case $service in
  gateway) echo "/health/simple" ;;  # bypasses Ocelot
  *) echo "/health" ;;
esac)
```

#### C. Health Check Timeouts Too Short
```yaml
# In docker-compose.yml, increase timeouts:
healthcheck:
  interval: 45s      # Time between checks  
  timeout: 20s       # Max time for single check
  retries: 6         # Number of retries
  start_period: 180s # Grace period during startup
```

### 3. "Gateway service failed to become healthy"

**Symptoms:**
```
Waiting for gateway service... (10/10)
❌ gateway service failed to become healthy
```

**Root Cause:** Ocelot routing conflict with health endpoint

**Fix:** Ensure `/health/simple` middleware is before Ocelot:
```csharp
// In LibraryApp.ApiGateway/Program.cs - BEFORE UseOcelot()
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

### 4. Build Failures

**Symptoms:**
```
error CS0246: The type or namespace name 'UseNpgsql' could not be found
```

**Root Cause:** Missing NuGet package or incorrect database configuration

**Fix:** Ensure services use InMemory databases consistently:
```csharp
services.AddDbContext<MemberDbContext>(options =>
    options.UseInMemoryDatabase("MemberServiceDb"));
```

### 5. Job Sequencing Issues

**Symptoms:**
```
Integration tests running before Docker build completes
```

**Fix:** Ensure proper job dependencies:
```yaml
integration-tests:
  needs: [changes, build-and-test, docker-build]  # Must wait for docker-build!
```

### 6. CodeQL/SARIF Processing Failures

**Symptoms:**
```
Code Scanning could not process the submitted SARIF file
```

**Fix:** Ensure CodeQL runs separately from main CI:
```yaml
# In .github/workflows/codeql.yml
on:
  schedule:
    - cron: '0 3 * * 1'  # Weekly, not on every push
  push:
    branches: [ main ]   # Only main branch
```

## Debugging Workflow

### 1. Identify Failure Point
```bash
# Check which job failed in GitHub Actions
# Look for the first failing step
```

### 2. Reproduce Locally
```bash
# Build locally first
.\scripts\build-all.ps1

# Test Docker health checks
docker-compose -f docker-compose.yml up -d --build
docker-compose ps  # Check service status

# Test integration tests
dotnet test --filter "Category=Integration"
```

### 3. Check Service Health Individually
```bash
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Books
curl http://localhost:5003/health  # Members
curl http://localhost:5000/health/simple  # Gateway

# Check logs if unhealthy
docker logs libraryapp-auth-service
docker logs libraryapp-api-gateway
```

### 4. Fix and Test Before Commit
```bash
# Always test locally before pushing
dotnet build
dotnet test
.\scripts\start-dev.ps1 -Detached
# Wait for health checks
dotnet test --filter "Category=Integration"
```

## Prevention Checklist

- [ ] Add `[Trait("Category", "Integration")]` to integration tests
- [ ] Use self-contained health checks (no external service dependencies)
- [ ] API Gateway uses `/health/simple` for Docker health checks
- [ ] Health check timeouts appropriate for CI environment (45s+ intervals)
- [ ] Job dependencies ensure proper sequencing
- [ ] Local testing before commit
- [ ] CodeQL runs on separate schedule to avoid conflicts

## Quick Fixes Reference

```bash
# Common quick fixes
git add . && git commit -m "Fix: Add integration test traits"
git add . && git commit -m "Fix: Increase health check timeouts" 
git add . && git commit -m "Fix: Use /health/simple for gateway health check"
git add . && git commit -m "Fix: Remove circular health check dependencies"
```