# CI/CD Context for AI Agents

> **Purpose**: Complete context for CI/CD troubleshooting and pipeline management

## Load These Atoms:
- `atoms/ci-pipeline.md` - Pipeline structure and patterns
- `atoms/health-checks.md` - Health check implementation and debugging  
- `reference/commands.md` - Quick command reference

## When to Use This Context:
- CI/CD pipeline failures
- GitHub Actions debugging
- Docker health check issues
- Integration test failures
- Build/deployment problems

## Key Concepts Summary:

### Pipeline Flow
```
changes → build-and-test → docker-build → integration-tests → ci-summary
```

### Critical Success Factors:
1. **Integration tests must have `[Trait("Category", "Integration")]`**
2. **API Gateway health check uses `/health/simple` endpoint**
3. **Job dependencies prevent race conditions** 
4. **Health check timeouts appropriate for CI environment**
5. **Container names match CI script expectations**

### Most Common Failures:
1. Integration test discovery (`No file matches *.trx`)
2. Service health check failures (container unhealthy)
3. API Gateway Ocelot routing conflicts
4. Job sequencing issues (tests run before Docker build)
5. CodeQL/SARIF processing conflicts

### Quick Debugging Commands:
```bash
# Check service health locally
curl http://localhost:5000/health/simple  # Gateway
curl http://localhost:5001/health         # Auth
curl http://localhost:5002/health         # Books  
curl http://localhost:5003/health         # Members

# Check container status
docker-compose ps
docker logs libraryapp-api-gateway --tail 30

# Test integration tests locally
dotnet test --filter "Category=Integration" --list-tests
dotnet test --filter "Category=Integration" --logger trx
```

### Environment Variables:
- `ASPNETCORE_ENVIRONMENT=Development` (for Docker services)
- `DOTNET_VERSION=8.0.x`
- All JWT, database, and Redis configuration

### Recipe References:
- For pipeline fixes: `recipes/fix-ci-pipeline.md`
- For service issues: `recipes/debug-service-communication.md`
- For local testing: `reference/commands.md`

## Container Health Check Matrix:
```
Database Containers:
├── auth-db: pg_isready check (30s interval, 120s start period)
├── book-db: pg_isready check (30s interval, 120s start period)  
├── member-db: pg_isready check (30s interval, 120s start period)
└── redis: redis-cli ping (30s interval, 60s start period)

Service Containers:
├── auth-service: /health (45s interval, 180s start period)
├── book-service: /health (45s interval, 240s start period) 
├── member-service: /health (45s interval, 300s start period)
└── api-gateway: /health/simple (45s interval, 360s start period)
```

## Critical File Locations:
- CI Pipeline: `.github/workflows/ci.yml`
- Docker Compose: `docker-compose.yml` 
- Health Checks: `LibraryApp.{Service}/Infrastructure/Health/`
- API Gateway Config: `LibraryApp.ApiGateway/Program.cs`
- Integration Tests: `LibraryApp.Tests/Integration/`