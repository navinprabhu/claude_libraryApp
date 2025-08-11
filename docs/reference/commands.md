# Quick Reference: Commands

## Development Environment
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

## .NET Commands
```bash
# Build & Test
dotnet build                                    # Build all projects
dotnet test                                     # Run all tests  
dotnet test --collect:"XPlat Code Coverage"    # Run tests with coverage
dotnet run --project LibraryApp.AuthService    # Run specific service

# Specific test categories
dotnet test --filter "Category=Integration"    # Integration tests only
dotnet test LibraryApp.Tests/LibraryApp.Tests.csproj --logger trx # Generate trx files
```

## Docker Commands
```bash
# Development
docker-compose up -d                            # Start all services detached
docker-compose down                             # Stop all services
docker-compose logs -f auth-service             # Follow logs for specific service
docker-compose build                            # Rebuild all images
docker-compose build --no-cache                 # Clean rebuild

# Production (CI)
docker-compose -f docker-compose.yml up -d --build  # CI environment (no overrides)

# Health checks
docker-compose ps                               # Show service status
docker logs libraryapp-auth-service             # View service logs
```

## Database Commands  
```bash
# Connect to databases
docker exec -it libraryapp-auth-db psql -U auth_user -d AuthDatabase
docker exec -it libraryapp-book-db psql -U book_user -d BookDatabase  
docker exec -it libraryapp-member-db psql -U member_user -d MemberDatabase

# Query users table
docker exec -it libraryapp-auth-db psql -U auth_user -d AuthDatabase -c "SELECT id, username, email, role FROM users LIMIT 5;"
```

## Health Check Commands
```bash
# Individual service health
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Books
curl http://localhost:5003/health  # Members
curl http://localhost:5000/health/simple  # Gateway (Docker)
curl http://localhost:5000/health         # Gateway (full)

# Database health  
docker exec libraryapp-auth-db pg_isready -U auth_user -d AuthDatabase
docker exec libraryapp-redis redis-cli ping
```

## Git & CI Commands
```bash
# Git workflow
git add .
git commit -m "Your message"
git push

# Check CI status (if gh CLI configured)
gh run list --limit 5
gh run view [run-id] --log

# Local CI simulation
.\scripts\build-all.ps1
docker-compose -f docker-compose.yml up -d --build
# Wait for health checks, then:
dotnet test --filter "Category=Integration"
```

## Troubleshooting Commands
```bash
# Check ports
netstat -an | grep 500[0-3]    # Check if ports 5000-5003 are in use
netstat -an | grep 543[2-4]    # Check database ports 5432-5434

# Clean up Docker
docker system prune -a         # Clean up all Docker resources
docker-compose down -v         # Stop and remove volumes
docker container ls -a         # List all containers
docker image ls                # List all images

# Check container logs
docker logs libraryapp-auth-service --tail 50
docker logs libraryapp-api-gateway --tail 50

# Resource usage
docker stats                   # Live resource usage
df -h                         # Disk space
free -m                       # Memory usage
```

## API Testing Commands
```bash
# Login and get token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Use token for authenticated requests  
TOKEN="your-jwt-token-here"
curl -X GET http://localhost:5000/api/books \
  -H "Authorization: Bearer $TOKEN"

# Create a book (Admin only)
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title": "Sample Book", "author": "Author Name", "isbn": "1234567890"}'
```

## Performance & Monitoring  
```bash
# Monitor logs in real-time
.\scripts\logs.ps1

# Check service resource usage
docker stats libraryapp-auth-service
docker stats libraryapp-book-service  
docker stats libraryapp-member-service
docker stats libraryapp-api-gateway

# Database connection counts
docker exec -it libraryapp-auth-db psql -U auth_user -d AuthDatabase -c "SELECT count(*) FROM pg_stat_activity;"
```

## Configuration Commands
```bash
# View current configuration
docker exec libraryapp-auth-service printenv | grep -E "(JWT_|DB_|REDIS_)"
docker exec libraryapp-api-gateway printenv | grep SERVICE_URLS

# Test Redis connection  
docker exec libraryapp-redis redis-cli ping
docker exec libraryapp-redis redis-cli info server

# Check service discovery
docker exec libraryapp-book-service nslookup auth-service
docker exec libraryapp-member-service nslookup book-service
```