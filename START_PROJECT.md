# How to Run LibraryApp Microservices Project

## Prerequisites
✅ .NET 8.0 SDK installed  
✅ Ports 5000-5003 available  
✅ No running dotnet processes (check with `tasklist | findstr dotnet`)

## Current Project Status
- ✅ Complete CI/CD GitHub Actions workflows
- ✅ API Gateway with Ocelot configuration
- ✅ Authentication service with JWT
- ✅ Book management service  
- ✅ Member management service
- ✅ Inter-service communication
- ✅ Prometheus monitoring setup
- ✅ Comprehensive documentation

## Quick Start Instructions

### IMPORTANT: Fix Compilation Issues First
Before running, execute these commands to fix compilation issues:
```bash
# Kill any running dotnet processes
powershell "Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue"

# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Option 1: Run All Services (Recommended)
Open 4 separate command prompt windows:

```bash
# Terminal 1 - Auth Service
cd LibraryApp.AuthService
dotnet run --urls="http://localhost:5001"

# Terminal 2 - Book Service  
cd LibraryApp.BookService
dotnet run --urls="http://localhost:5002"

# Terminal 3 - Member Service
cd LibraryApp.MemberService
dotnet run --urls="http://localhost:5003"

# Terminal 4 - API Gateway
cd LibraryApp.ApiGateway
dotnet run --urls="http://localhost:5000"
```

### Option 2: Test Individual Services
```bash
# Test Auth Service Only
cd LibraryApp.AuthService && dotnet run

# Test Book Service Only  
cd LibraryApp.BookService && dotnet run

# Test Member Service Only
cd LibraryApp.MemberService && dotnet run
```

## Service Architecture
```
API Gateway (Port 5000) 
    ├── Auth Service (Port 5001)
    ├── Book Service (Port 5002) 
    └── Member Service (Port 5003)
```

## API Access Points

### Direct Service Access
- **Auth API**: http://localhost:5001 (Swagger UI available)
- **Book API**: http://localhost:5002 (Swagger UI available)  
- **Member API**: http://localhost:5003 (Swagger UI available)

### Via API Gateway
- **Gateway**: http://localhost:5000
- **Auth via Gateway**: http://localhost:5000/api/auth/*
- **Books via Gateway**: http://localhost:5000/api/books/*
- **Members via Gateway**: http://localhost:5000/api/members/*

## Health Check Endpoints
```
http://localhost:5000/health        # Gateway health
http://localhost:5001/health        # Auth service health
http://localhost:5002/health        # Book service health
http://localhost:5003/health        # Member service health
http://localhost:5000/health/services  # Aggregated health check
```

## Authentication Flow
1. **Register**: `POST /api/auth/register`
   ```json
   {
     "email": "user@example.com",
     "password": "SecurePassword123!",
     "username": "testuser"
   }
   ```

2. **Login**: `POST /api/auth/login`
   ```json
   {
     "email": "user@example.com", 
     "password": "SecurePassword123!"
   }
   ```

3. **Use JWT**: Add header `Authorization: Bearer {token}`

## Sample API Testing Workflow

### 1. Start Services and Verify Health
```bash
curl http://localhost:5000/health/gateway
curl http://localhost:5000/health/services
```

### 2. Register and Login
```bash
# Register user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!","username":"testuser"}'

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123!"}'
```

### 3. Create Member and Books (with JWT token)
```bash
# Create member
curl -X POST http://localhost:5000/api/members \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com"}'

# Create book
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Book","author":"Test Author","isbn":"1234567890"}'
```

## Development Features

### Monitoring
- **Prometheus metrics**: Available on each service
- **Correlation ID tracking**: X-Correlation-ID header
- **Request/response logging**: Structured logging with Serilog

### Security
- **JWT Authentication**: RS256/HS256 support
- **Rate limiting**: Configurable per service
- **CORS**: Configurable origins

### DevOps Ready
- **GitHub Actions CI/CD**: Complete workflows in `.github/workflows/`
- **Docker support**: Multi-stage builds
- **Kubernetes manifests**: In `k8s/` directory
- **Monitoring stack**: Prometheus + Grafana configs

## Known Issues & Fixes

### Issue: Build failures with locked files
**Solution**: Kill dotnet processes before building
```bash
powershell "Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue"
dotnet clean
dotnet build
```

### Issue: Port already in use (e.g., "address already in use")
**Error**: `Failed to bind to address http://[::1]:5000: address already in use`

**Solutions**:

**Option A - Kill processes using the ports:**
```bash
# Find processes using ports 5000-5003
netstat -ano | findstr ":5000\|:5001\|:5002\|:5003"

# Kill specific processes (replace PID with actual process ID)
powershell "Stop-Process -Id <PID> -Force -ErrorAction SilentlyContinue"

# Or kill all dotnet processes
powershell "Stop-Process -Name dotnet -Force -ErrorAction SilentlyContinue"
```

**Option B - Use different ports:**
```bash
# API Gateway on port 6000
cd LibraryApp.ApiGateway
dotnet run --urls="http://localhost:6000"

# Auth Service on port 6001
cd LibraryApp.AuthService  
dotnet run --urls="http://localhost:6001"

# Book Service on port 6002
cd LibraryApp.BookService
dotnet run --urls="http://localhost:6002"

# Member Service on port 6003
cd LibraryApp.MemberService
dotnet run --urls="http://localhost:6003"
```

**Option C - Update Ocelot configuration for new ports:**
If using different ports, update `LibraryApp.ApiGateway/Configuration/ocelot.json` to match the new port numbers.

### Issue: Docker connectivity issues
**Solution**: Use native .NET approach as documented above

### Issue: Missing packages
**Solution**: 
```bash
dotnet restore
# If issues persist, check NuGet sources:
dotnet nuget list source
```

## Development Tips

1. **Hot Reload**: Use `dotnet watch run` instead of `dotnet run`
2. **Debug Mode**: Set `ASPNETCORE_ENVIRONMENT=Development`  
3. **Logs**: Check console output for correlation IDs and structured logs
4. **Testing**: Use Swagger UI for interactive API testing
5. **Database**: All services use in-memory databases (data resets on restart)

## Project Structure
```
├── LibraryApp.AuthService/          # JWT authentication & user management
├── LibraryApp.BookService/          # Book catalog & borrowing logic
├── LibraryApp.MemberService/        # Member management & profiles  
├── LibraryApp.ApiGateway/           # Ocelot API Gateway
├── LibraryApp.Shared.*/             # Shared libraries & models
├── .github/workflows/               # CI/CD pipelines
├── k8s/                            # Kubernetes deployment manifests
├── docs/                           # Architecture & deployment docs
└── START_PROJECT.md                # This file
```

## Next Steps
1. Start all services as documented above
2. Test the API endpoints via Swagger UI or curl
3. Check health endpoints to verify connectivity
4. Explore the CI/CD workflows in `.github/workflows/`
5. Review deployment documentation in `docs/DEPLOYMENT.md`

For any issues, check the individual service logs for detailed error information.