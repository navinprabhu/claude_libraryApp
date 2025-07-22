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

# Get the script directory and project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

# Change to project root directory where docker-compose.yml is located
Set-Location $ProjectRoot
Write-Host "Working directory set to: $ProjectRoot" -ForegroundColor Gray

# Verify required docker-compose files exist
if (-not (Test-Path "docker-compose.yml")) {
    Write-Host "ERROR: docker-compose.yml not found in $ProjectRoot" -ForegroundColor Red
    Write-Host "Please ensure you are running this script from the scripts directory in the LibraryApp project" -ForegroundColor Yellow
    exit 1
}
Write-Host "Found docker-compose.yml in project root" -ForegroundColor Green

if (Test-Path "docker-compose.override.yml") {
    Write-Host "Found docker-compose.override.yml in project root" -ForegroundColor Green
} else {
    Write-Host "WARNING: docker-compose.override.yml not found - development overrides will not be applied" -ForegroundColor Yellow
}

# Help function
function Show-Help {
    Write-Host "LibraryApp Docker Build Script" -ForegroundColor Cyan
    Write-Host "==============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\scripts\build-all.ps1 [OPTIONS]                  # From project root"
    Write-Host "  .\build-all.ps1 [OPTIONS]                          # From scripts directory"
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
    Write-Host "  .\scripts\build-all.ps1                           # Basic build from project root"
    Write-Host "  .\scripts\build-all.ps1 -Clean -NoBuildCache      # Clean build without cache"
    Write-Host "  .\scripts\build-all.ps1 -Environment Production   # Production build"
    Write-Host "  .\scripts\build-all.ps1 -Verbose                  # Verbose output"
    Write-Host "  .\scripts\build-all.ps1 -DryRun                   # Preview actions"
    Write-Host ""
    Write-Host "TROUBLESHOOTING:" -ForegroundColor Yellow
    Write-Host "  If you get Docker connectivity errors, try:"
    Write-Host "  - docker context use default"
    Write-Host "  - Restart Docker Desktop"
    Write-Host "  - Run the script with -DryRun to test without Docker"
    Write-Host ""
    exit 0
}

# Function to check if Docker Desktop is running
function Test-DockerDesktop {
    Write-Host "  Checking if Docker Desktop is running..." -ForegroundColor Gray
    
    # Check for Docker Desktop process on Windows
    if ($env:OS -like "*Windows*" -or $IsWindows) {
        $dockerDesktop = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
        if ($dockerDesktop) {
            Write-Host "  * Docker Desktop process is running" -ForegroundColor Green
            return $true
        } else {
            Write-Host "  * Docker Desktop process is NOT running" -ForegroundColor Red
            return $false
        }
    }
    
    # For other platforms, assume Docker service
    try {
        $dockerInfo = docker info 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  * Docker service appears to be running" -ForegroundColor Green
            return $true
        }
    } catch {
        Write-Host "  * Docker service appears to be stopped" -ForegroundColor Red
        return $false
    }
    
    return $false
}

# Function to attempt Docker context fix
function Try-FixDockerContext {
    Write-Host "Attempting to fix Docker context..." -ForegroundColor Yellow
    
    # First check if Docker Desktop is running
    $dockerDesktopRunning = Test-DockerDesktop
    if (-not $dockerDesktopRunning) {
        Write-Host "  * Docker Desktop is not running - please start Docker Desktop first" -ForegroundColor Red
        Write-Host "    Steps to fix:" -ForegroundColor Yellow
        Write-Host "    1. Start Docker Desktop application" -ForegroundColor Gray
        Write-Host "    2. Wait for Docker Desktop to fully initialize" -ForegroundColor Gray
        Write-Host "    3. Look for 'Docker Desktop is running' in the system tray" -ForegroundColor Gray
        return $false
    }
    
    # Try switching to default context
    Write-Host "  Trying default context..." -ForegroundColor Gray
    try {
        $contextSwitchOutput = docker context use default 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  * Switched to default context" -ForegroundColor Green
            # Test if it works now
            $testOutput = docker ps -q 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  * Docker is now working with default context!" -ForegroundColor Green
                return $true
            } else {
                Write-Host "  * Default context still not working: $testOutput" -ForegroundColor Yellow
            }
        } else {
            Write-Host "  * Failed to switch to default context: $contextSwitchOutput" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  * Error switching to default context: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    # Try desktop-linux context if available
    Write-Host "  Trying desktop-linux context..." -ForegroundColor Gray
    try {
        $contextSwitchOutput = docker context use desktop-linux 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  * Switched to desktop-linux context" -ForegroundColor Green
            # Test if it works now
            $testOutput = docker ps -q 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  * Docker is now working with desktop-linux context!" -ForegroundColor Green
                return $true
            } else {
                Write-Host "  * Desktop-linux context still not working: $testOutput" -ForegroundColor Yellow
            }
        } else {
            Write-Host "  * Failed to switch to desktop-linux context: $contextSwitchOutput" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  * Error switching to desktop-linux context: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host "  * Unable to fix Docker context automatically" -ForegroundColor Red
    return $false
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
    if (Test-Path "docker-compose.override.yml") {
        $env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"
        Write-Host "Building for Development environment..." -ForegroundColor Green
        Write-Host "Using compose files: docker-compose.yml + docker-compose.override.yml" -ForegroundColor Gray
    } else {
        $env:COMPOSE_FILE = "docker-compose.yml"
        Write-Host "Building for Development environment (without override file)..." -ForegroundColor Yellow
        Write-Host "Using compose files: docker-compose.yml only" -ForegroundColor Gray
    }
} else {
    $env:COMPOSE_FILE = "docker-compose.yml"
    Write-Host "Building for $Environment environment..." -ForegroundColor Green
    Write-Host "Using compose files: docker-compose.yml only" -ForegroundColor Gray
}

try {
    # Check if Docker is running (skip in dry-run mode)
    if ($DryRun) {
        Write-Host "DRY RUN: Skipping Docker status check" -ForegroundColor Magenta
    } else {
        Write-Host "Checking Docker status..." -ForegroundColor Yellow
        
        # Try multiple Docker connectivity checks
        $dockerWorking = $false
        $dockerError = ""
        
        # First, try docker version (lighter than docker info)
        Write-Host "  Checking Docker client..." -ForegroundColor Gray
        try {
            $dockerVersionOutput = docker version --format json 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  * Docker client is working" -ForegroundColor Green
                
                # Now try a simple docker command that doesn't require daemon connection
                Write-Host "  Checking Docker daemon connectivity..." -ForegroundColor Gray
                $dockerPsOutput = docker ps -q 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $dockerWorking = $true
                    Write-Host "  * Docker daemon is accessible" -ForegroundColor Green
                } else {
                    $dockerError = "Docker daemon is not accessible - $dockerPsOutput"
                }
            } else {
                $dockerError = "Docker client is not working - $dockerVersionOutput"
            }
        } catch {
            $dockerError = "Docker command execution failed - $($_.Exception.Message)"
        }
        
        if (-not $dockerWorking) {
            Write-Host "Docker connectivity issue detected:" -ForegroundColor Yellow
            Write-Host "  Error: $dockerError" -ForegroundColor Red
            Write-Host ""
            Write-Host "Current Docker context:" -ForegroundColor Yellow
            docker context show 2>$null
            Write-Host ""
            
            # Attempt automatic fix
            $fixed = Try-FixDockerContext
            if ($fixed) {
                Write-Host "* Docker connectivity restored!" -ForegroundColor Green
            } else {
                Write-Host ""
                Write-Host "DOCKER CONNECTIVITY ISSUE DETECTED" -ForegroundColor Red
                Write-Host "====================================" -ForegroundColor Red
                Write-Host ""
                Write-Host "Common solutions:" -ForegroundColor Yellow
                Write-Host "  1. Start Docker Desktop (if not running)" -ForegroundColor Gray
                Write-Host "  2. Restart Docker Desktop completely" -ForegroundColor Gray
                Write-Host "  3. Run Docker Desktop as Administrator" -ForegroundColor Gray
                Write-Host "  4. Check Windows features: Enable 'Windows Subsystem for Linux' and 'Virtual Machine Platform'" -ForegroundColor Gray
                Write-Host "  5. For WSL2: Run 'wsl --update' in PowerShell as Administrator" -ForegroundColor Gray
                Write-Host ""
                Write-Host "Available Docker contexts:" -ForegroundColor Yellow
                docker context ls 2>$null
                Write-Host ""
                Write-Host "Current Docker context:" -ForegroundColor Yellow
                docker context show 2>$null
                Write-Host ""
                Write-Host "You can test Docker manually with:" -ForegroundColor Yellow
                Write-Host "  docker version" -ForegroundColor Gray
                Write-Host "  docker ps" -ForegroundColor Gray
                Write-Host ""
                
                $choice = Read-Host "Continue anyway? Docker build will likely fail. (y/N)"
                if ($choice -notmatch '^[Yy]') {
                    Write-Host ""
                    Write-Host "Build cancelled. Please fix Docker connectivity and try again." -ForegroundColor Red
                    Write-Host "TIP: Use -DryRun flag to test the script without Docker" -ForegroundColor Yellow
                    exit 1
                } else {
                    Write-Host "Continuing with potentially limited Docker functionality..." -ForegroundColor Yellow
                }
            }
        } else {
            Write-Host "* Docker is running and accessible" -ForegroundColor Green
        }
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