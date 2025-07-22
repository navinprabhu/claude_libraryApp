# Build All Containers Script for LibraryApp
# This script builds all Docker containers for the LibraryApp microservices

param(
    [switch]$Clean,
    [switch]$NoBuildCache,
    [string]$Environment = "Development",
    [switch]$Verbose,
    [switch]$Help,
    [switch]$DryRun
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Help function
function Show-Help {
    Write-Host "LibraryApp Docker Build Script" -ForegroundColor Cyan
    Write-Host "==============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\build-all.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "  -Clean             Remove existing containers and images before building"
    Write-Host "  -NoBuildCache      Build without using Docker cache"
    Write-Host "  -Environment       Target environment (Development, Staging, Production). Default: Development"
    Write-Host "  -Verbose           Show detailed build output"
    Write-Host "  -DryRun            Show what would be done without executing"
    Write-Host "  -Help              Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  .\build-all.ps1                                    # Basic build"
    Write-Host "  .\build-all.ps1 -Clean -NoBuildCache              # Clean build without cache"
    Write-Host "  .\build-all.ps1 -Environment Production           # Production build"
    Write-Host "  .\build-all.ps1 -Verbose                          # Verbose output"
    Write-Host "  .\build-all.ps1 -DryRun                           # Preview actions"
    Write-Host ""
    exit 0
}

# Show help if requested
if ($Help) {
    Show-Help
}

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Docker Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No actual changes will be made" -ForegroundColor Magenta
    Write-Host ""
}

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
    # Check if Docker is running (skip in dry-run mode)
    if ($DryRun) {
        Write-Host "DRY RUN: Skipping Docker status check" -ForegroundColor Magenta
    } else {
        Write-Host "Checking Docker status..." -ForegroundColor Yellow
        $dockerInfo = docker info 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Docker is not running or accessible." -ForegroundColor Red
            Write-Host "Please ensure Docker Desktop is installed and running, then try again." -ForegroundColor Yellow
            Write-Host "On Windows: Start Docker Desktop" -ForegroundColor Gray
            Write-Host "On Linux: sudo systemctl start docker" -ForegroundColor Gray
            exit 1
        }
        Write-Host "* Docker is running" -ForegroundColor Green
    }

    # Clean up if requested
    if ($Clean) {
        Write-Host "Cleaning up existing containers and images..." -ForegroundColor Yellow
        
        if ($DryRun) {
            Write-Host "DRY RUN: Would execute: docker-compose down --remove-orphans" -ForegroundColor Magenta
            Write-Host "DRY RUN: Would execute: docker image prune -f" -ForegroundColor Magenta
            if ($NoBuildCache) {
                Write-Host "DRY RUN: Would execute: docker builder prune -f" -ForegroundColor Magenta
                Write-Host "* DRY RUN: Build cache would be cleared" -ForegroundColor Magenta
            }
            Write-Host "* DRY RUN: Cleanup would be completed" -ForegroundColor Magenta
        } else {
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
    
    if ($DryRun) {
        Write-Host "DRY RUN: Would execute: docker-compose $($buildArgs -join ' ')" -ForegroundColor Magenta
        Write-Host "DRY RUN: Build would target services: auth-service, book-service, member-service, api-gateway" -ForegroundColor Magenta
        Write-Host "* DRY RUN: All images would be built successfully" -ForegroundColor Magenta
    } else {
        $buildStartTime = Get-Date
        & docker-compose @buildArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE"
        }
        
        $buildEndTime = Get-Date
        $buildDuration = $buildEndTime - $buildStartTime
        
        Write-Host "* All images built successfully" -ForegroundColor Green
        Write-Host "Build completed in $($buildDuration.TotalMinutes.ToString('F1')) minutes" -ForegroundColor Cyan
    }

    if (-not $DryRun) {
        # List built images
        Write-Host "`nBuilt images:" -ForegroundColor Yellow
        $builtImages = docker images --filter "reference=*libraryapp*" --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}"
        if ($builtImages) {
            Write-Host $builtImages
        } else {
            Write-Host "No LibraryApp images found. Checking with alternative filters..." -ForegroundColor Yellow
            docker images --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}\t{{.CreatedAt}}" | Select-Object -First 10
        }

        # Verify images
        Write-Host "`nVerifying built images..." -ForegroundColor Yellow
    } else {
        Write-Host "`nDRY RUN: Would list and verify built images" -ForegroundColor Magenta
    }
    
    if (-not $DryRun) {
        $services = @("auth-service", "book-service", "member-service", "api-gateway")
        $allImagesFound = $true
        
        foreach ($service in $services) {
            # Try multiple patterns to find the service images
            $patterns = @("*${service}*", "*libraryapp*${service}*", "*${service}*latest*")
            $imageFound = $false
            
            foreach ($pattern in $patterns) {
                $images = docker images --filter "reference=${pattern}" --format "{{.Repository}}:{{.Tag}}" --no-trunc
                if ($images) {
                    $image = $images | Select-Object -First 1
                    Write-Host "* ${service}: $image" -ForegroundColor Green
                    $imageFound = $true
                    break
                }
            }
            
            if (-not $imageFound) {
                Write-Warning "! ${service}: Image not found with any pattern"
                $allImagesFound = $false
            }
        }
        
        if (-not $allImagesFound) {
            Write-Host "`nNote: Some service images were not found. This might be normal if:" -ForegroundColor Yellow
            Write-Host "- This is the first build" -ForegroundColor Gray
            Write-Host "- Images use different naming conventions" -ForegroundColor Gray
            Write-Host "- Docker Compose creates images with project prefix" -ForegroundColor Gray
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