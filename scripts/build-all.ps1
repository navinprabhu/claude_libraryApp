# Build All Containers Script for LibraryApp
# This script builds all Docker containers for the LibraryApp microservices

param(
    [switch]$Clean,
    [switch]$NoBuildCache,
    [string]$Environment = "Development",
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Docker Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:DOCKER_BUILDKIT = 1
$env:COMPOSE_DOCKER_CLI_BUILD = 1
$env:ASPNETCORE_ENVIRONMENT = $Environment

if ($Environment -eq "Development") {
    $env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"
    Write-Host "Building for Development environment..." -ForegroundColor Green
} else {
    $env:COMPOSE_FILE = "docker-compose.yml"
    Write-Host "Building for $Environment environment..." -ForegroundColor Green
}

try {
    # Check if Docker is running
    Write-Host "Checking Docker status..." -ForegroundColor Yellow
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker is not running or accessible. Please start Docker Desktop and try again."
    }
    Write-Host "* Docker is running" -ForegroundColor Green

    # Clean up if requested
    if ($Clean) {
        Write-Host "Cleaning up existing containers and images..." -ForegroundColor Yellow
        
        # Stop and remove containers
        docker-compose down --remove-orphans 2>$null
        
        # Remove unused images
        docker image prune -f 2>$null
        
        # Remove build cache if requested
        if ($NoBuildCache) {
            docker builder prune -f 2>$null
            Write-Host "* Build cache cleared" -ForegroundColor Green
        }
        
        Write-Host "* Cleanup completed" -ForegroundColor Green
    }

    # Build arguments
    $buildArgs = @("build")
    
    if ($NoBuildCache) {
        $buildArgs += "--no-cache"
    }
    
    if ($Verbose) {
        $buildArgs += "--progress=plain"
    }

    # Build all services
    Write-Host "Building all Docker images..." -ForegroundColor Yellow
    Write-Host "Command: docker-compose $($buildArgs -join ' ')" -ForegroundColor Gray
    
    $buildStartTime = Get-Date
    & docker-compose @buildArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
    }
    
    $buildEndTime = Get-Date
    $buildDuration = $buildEndTime - $buildStartTime
    
    Write-Host "* All images built successfully" -ForegroundColor Green
    Write-Host "Build completed in $($buildDuration.TotalMinutes.ToString('F1')) minutes" -ForegroundColor Cyan

    # List built images
    Write-Host "`nBuilt images:" -ForegroundColor Yellow
    docker images --filter "reference=libraryapp*" --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"

    # Verify images
    Write-Host "`nVerifying built images..." -ForegroundColor Yellow
    $services = @("auth-service", "book-service", "member-service", "api-gateway")
    foreach ($service in $services) {
        $image = docker images --filter "reference=*${service}*" --format "{{.Repository}}:{{.Tag}}" | Select-Object -First 1
        if ($image) {
            Write-Host "* $service: $image" -ForegroundColor Green
        } else {
            Write-Warning "! $service: Image not found"
        }
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Use 'start-dev.ps1' to start the development environment" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Build failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}