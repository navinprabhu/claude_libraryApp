# Build and Run Library Management System
# This script builds all services and starts them using Docker Compose

param(
    [switch]$NoBuild,
    [switch]$DetachedMode,
    [switch]$CleanBuild,
    [switch]$ShowLogs,
    [string]$Environment = "Development",
    [string]$ComposeFile = "docker-compose.yml"
)

Write-Host "=== Library Management System - Build and Run ===" -ForegroundColor Green

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Check for required tools
Write-Host "`nChecking required tools..." -ForegroundColor Cyan

if (-not (Test-Command "dotnet")) {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

if (-not (Test-Command "docker")) {
    Write-Host "✗ Docker not found. Please install Docker Desktop." -ForegroundColor Red
    exit 1
}

if (-not (Test-Command "docker-compose")) {
    Write-Host "⚠ docker-compose not found. Checking for Docker Compose plugin..." -ForegroundColor Yellow
    docker compose version > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Docker Compose not available. Please install Docker Desktop with Compose support." -ForegroundColor Red
        exit 1
    } else {
        $dockerComposeCmd = "docker compose"
        Write-Host "✓ Docker Compose plugin found" -ForegroundColor Green
    }
} else {
    $dockerComposeCmd = "docker-compose"
    Write-Host "✓ docker-compose found" -ForegroundColor Green
}

# Set environment
$env:ASPNETCORE_ENVIRONMENT = $Environment
Write-Host "Environment set to: $Environment" -ForegroundColor Cyan

# Clean build if requested
if ($CleanBuild) {
    Write-Host "`nCleaning previous builds..." -ForegroundColor Cyan
    dotnet clean
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Clean completed" -ForegroundColor Green
    } else {
        Write-Host "⚠ Clean completed with warnings" -ForegroundColor Yellow
    }
}

# Build solution
if (-not $NoBuild) {
    Write-Host "`nBuilding solution..." -ForegroundColor Cyan
    
    # Restore packages first
    Write-Host "Restoring NuGet packages..." -ForegroundColor Gray
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Package restore failed" -ForegroundColor Red
        exit 1
    }
    
    # Build all projects
    Write-Host "Building all projects..." -ForegroundColor Gray
    dotnet build --no-restore --configuration Release
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Build completed successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Build failed" -ForegroundColor Red
        exit 1
    }
    
    # Run tests
    Write-Host "Running tests..." -ForegroundColor Gray
    dotnet test --no-build --configuration Release --logger:"console;verbosity=minimal"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ All tests passed" -ForegroundColor Green
    } else {
        Write-Host "⚠ Some tests failed - continuing anyway" -ForegroundColor Yellow
    }
} else {
    Write-Host "⏭ Skipping build (NoBuild flag specified)" -ForegroundColor Yellow
}

# Check if Docker Compose file exists
if (-not (Test-Path $ComposeFile)) {
    Write-Host "`nCreating basic Docker Compose file..." -ForegroundColor Cyan
    
    $composeContent = @"
version: '3.8'

services:
  # SQL Server Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: libraryapp-sqlserver
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: LibraryApp123!
      MSSQL_PID: Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - libraryapp-network

  # Redis Cache (optional)
  redis:
    image: redis:7-alpine
    container_name: libraryapp-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - libraryapp-network

  # RabbitMQ Message Broker (optional)
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: libraryapp-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: libraryapp
      RABBITMQ_DEFAULT_PASS: libraryapp123
    ports:
      - "5672:5672"
      - "15672:15672"  # Management UI
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - libraryapp-network

  # Placeholder for Book Service (will be added when microservices are implemented)
  # book-service:
  #   build:
  #     context: .
  #     dockerfile: src/LibraryApp.BookService/Dockerfile
  #   container_name: libraryapp-book-service
  #   environment:
  #     - ASPNETCORE_ENVIRONMENT=Development
  #     - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=LibraryBooks;User Id=sa;Password=LibraryApp123!;TrustServerCertificate=true
  #   ports:
  #     - "5001:80"
  #   depends_on:
  #     - sqlserver
  #   networks:
  #     - libraryapp-network

  # Placeholder for Member Service
  # member-service:
  #   build:
  #     context: .
  #     dockerfile: src/LibraryApp.MemberService/Dockerfile
  #   container_name: libraryapp-member-service
  #   environment:
  #     - ASPNETCORE_ENVIRONMENT=Development
  #     - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=LibraryMembers;User Id=sa;Password=LibraryApp123!;TrustServerCertificate=true
  #   ports:
  #     - "5002:80"
  #   depends_on:
  #     - sqlserver
  #   networks:
  #     - libraryapp-network

  # Placeholder for Borrowing Service
  # borrowing-service:
  #   build:
  #     context: .
  #     dockerfile: src/LibraryApp.BorrowingService/Dockerfile
  #   container_name: libraryapp-borrowing-service
  #   environment:
  #     - ASPNETCORE_ENVIRONMENT=Development
  #     - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=LibraryBorrowing;User Id=sa;Password=LibraryApp123!;TrustServerCertificate=true
  #   ports:
  #     - "5003:80"
  #   depends_on:
  #     - sqlserver
  #   networks:
  #     - libraryapp-network

volumes:
  sqlserver_data:
  redis_data:
  rabbitmq_data:

networks:
  libraryapp-network:
    driver: bridge
"@

    Set-Content -Path $ComposeFile -Value $composeContent
    Write-Host "✓ Created basic Docker Compose file: $ComposeFile" -ForegroundColor Green
    Write-Host "Note: Microservice containers are commented out until the services are implemented." -ForegroundColor Yellow
}

# Start services with Docker Compose
Write-Host "`nStarting infrastructure services..." -ForegroundColor Cyan

$composeArgs = @("up")
if ($DetachedMode) {
    $composeArgs += "-d"
}

try {
    if ($dockerComposeCmd -eq "docker compose") {
        & docker compose @composeArgs
    } else {
        & docker-compose @composeArgs
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Services started successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to start some services" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ Error starting services: $_" -ForegroundColor Red
    exit 1
}

# Show service status
Write-Host "`nService Status:" -ForegroundColor Cyan
try {
    if ($dockerComposeCmd -eq "docker compose") {
        docker compose ps
    } else {
        docker-compose ps
    }
} catch {
    Write-Host "Could not retrieve service status" -ForegroundColor Yellow
}

# Show logs if requested
if ($ShowLogs -and $DetachedMode) {
    Write-Host "`nShowing service logs (press Ctrl+C to stop)..." -ForegroundColor Cyan
    try {
        if ($dockerComposeCmd -eq "docker compose") {
            docker compose logs -f
        } else {
            docker-compose logs -f
        }
    } catch {
        Write-Host "Could not show logs" -ForegroundColor Yellow
    }
}

Write-Host "`n=== Services Status ===" -ForegroundColor Green
Write-Host "Infrastructure services are now running!" -ForegroundColor Green
Write-Host "`nAvailable services:" -ForegroundColor Cyan
Write-Host "- SQL Server: localhost:1433 (sa/LibraryApp123!)"
Write-Host "- Redis: localhost:6379"
Write-Host "- RabbitMQ: localhost:5672 (libraryapp/libraryapp123)"
Write-Host "- RabbitMQ Management: http://localhost:15672"

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Implement individual microservices (BookService, MemberService, BorrowingService)"
Write-Host "2. Update docker-compose.yml to include microservice containers"
Write-Host "3. Run database migrations for each service"
Write-Host "4. Test API endpoints"

Write-Host "`nUseful commands:" -ForegroundColor Cyan
Write-Host "- Stop services: docker compose down"
Write-Host "- View logs: docker compose logs -f"
Write-Host "- Restart services: docker compose restart"
Write-Host "- Remove volumes: docker compose down -v"

if (-not $DetachedMode) {
    Write-Host "`nPress Ctrl+C to stop all services..." -ForegroundColor Yellow
}