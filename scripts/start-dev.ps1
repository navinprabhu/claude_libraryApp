# Start Development Environment Script for LibraryApp
# This script starts the complete LibraryApp development environment using Docker Compose

param(
    [switch]$Build,
    [switch]$Detached,
    [switch]$Recreate,
    [string]$Service,
    [switch]$Logs,
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Development Environment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables for development
$env:DOCKER_BUILDKIT = 1
$env:COMPOSE_DOCKER_CLI_BUILD = 1
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"

try {
    # Check if Docker is running
    Write-Host "Checking Docker status..." -ForegroundColor Yellow
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker is not running or accessible. Please start Docker Desktop and try again."
    }
    Write-Host "✓ Docker is running" -ForegroundColor Green

    # Create logs directory if it doesn't exist
    $logsDir = "logs"
    if (!(Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        New-Item -ItemType Directory -Path "$logsDir/auth" -Force | Out-Null
        New-Item -ItemType Directory -Path "$logsDir/book" -Force | Out-Null
        New-Item -ItemType Directory -Path "$logsDir/member" -Force | Out-Null
        New-Item -ItemType Directory -Path "$logsDir/gateway" -Force | Out-Null
        Write-Host "✓ Created logs directories" -ForegroundColor Green
    }

    # Build services if requested
    if ($Build) {
        Write-Host "Building services..." -ForegroundColor Yellow
        $buildArgs = @("build")
        if ($Verbose) {
            $buildArgs += "--progress=plain"
        }
        & docker-compose @buildArgs
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE"
        }
        Write-Host "✓ Build completed" -ForegroundColor Green
    }

    # Prepare docker-compose up arguments
    $upArgs = @("up")
    
    if ($Detached) {
        $upArgs += "-d"
    }
    
    if ($Recreate) {
        $upArgs += "--force-recreate"
    }
    
    if ($Service) {
        $upArgs += $Service
        Write-Host "Starting service: $Service" -ForegroundColor Yellow
    } else {
        Write-Host "Starting all services..." -ForegroundColor Yellow
    }

    # Display service information
    Write-Host "`nServices will be available at:" -ForegroundColor Cyan
    Write-Host "• API Gateway:    http://localhost:5000" -ForegroundColor White
    Write-Host "• Auth Service:   http://localhost:5001" -ForegroundColor White
    Write-Host "• Book Service:   http://localhost:5002" -ForegroundColor White
    Write-Host "• Member Service: http://localhost:5003" -ForegroundColor White
    Write-Host "• Auth DB:        localhost:5432" -ForegroundColor White
    Write-Host "• Book DB:        localhost:5433" -ForegroundColor White
    Write-Host "• Member DB:      localhost:5434" -ForegroundColor White
    Write-Host "• Redis:          localhost:6379" -ForegroundColor White
    Write-Host ""
    
    # Display health check endpoints
    Write-Host "Health Check Endpoints:" -ForegroundColor Cyan
    Write-Host "• Gateway Health: http://localhost:5000/health/gateway" -ForegroundColor White
    Write-Host "• Services Health: http://localhost:5000/health/services" -ForegroundColor White
    Write-Host ""

    if ($Verbose) {
        Write-Host "Command: docker-compose $($upArgs -join ' ')" -ForegroundColor Gray
    }

    # Start services
    $startTime = Get-Date
    & docker-compose @upArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start services with exit code $LASTEXITCODE"
    }

    if ($Detached) {
        # Wait a moment and then show status
        Start-Sleep -Seconds 5
        
        Write-Host "`nService Status:" -ForegroundColor Yellow
        docker-compose ps
        
        Write-Host "`n========================================" -ForegroundColor Cyan
        Write-Host "Development environment started!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Useful commands:" -ForegroundColor Yellow
        Write-Host "• View logs: .\scripts\logs.ps1" -ForegroundColor White
        Write-Host "• Stop all: .\scripts\stop-all.ps1" -ForegroundColor White
        Write-Host "• Clean up: .\scripts\clean.ps1" -ForegroundColor White
        Write-Host ""
        
        if ($Logs) {
            Write-Host "Following logs (Ctrl+C to stop)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
            docker-compose logs -f
        }
    }

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Failed to start development environment!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    
    # Show container status for debugging
    Write-Host "`nContainer status:" -ForegroundColor Yellow
    docker-compose ps
    
    exit 1
}