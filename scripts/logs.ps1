# View Logs Script for LibraryApp
# This script provides easy access to container logs with filtering and following options

param(
    [string]$Service,
    [switch]$Follow,
    [int]$Tail = 100,
    [string]$Since,
    [switch]$Timestamps,
    [switch]$NoColor,
    [switch]$AllServices
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Logs Viewer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"

# Available services
$availableServices = @(
    "auth-service",
    "book-service", 
    "member-service",
    "api-gateway",
    "auth-db",
    "book-db",
    "member-db",
    "redis"
)

try {
    # Check if any containers are running
    Write-Host "Checking container status..." -ForegroundColor Yellow
    $runningContainers = docker-compose ps -q
    
    if (-not $runningContainers) {
        Write-Host "No LibraryApp containers are currently running." -ForegroundColor Red
        Write-Host "Use 'start-dev.ps1' to start the development environment." -ForegroundColor Yellow
        exit 1
    }

    # Show available services if no service specified
    if (-not $Service -and -not $AllServices) {
        Write-Host "Available services:" -ForegroundColor Yellow
        docker-compose ps --format "table {{.Name}}\t{{.Status}}\t{{.Ports}}"
        Write-Host ""
        Write-Host "Usage examples:" -ForegroundColor Cyan
        Write-Host "• View all logs: .\scripts\logs.ps1 -AllServices" -ForegroundColor White
        Write-Host "• Follow API Gateway: .\scripts\logs.ps1 -Service api-gateway -Follow" -ForegroundColor White
        Write-Host "• Last 50 lines: .\scripts\logs.ps1 -Service auth-service -Tail 50" -ForegroundColor White
        Write-Host "• Since 1 hour ago: .\scripts\logs.ps1 -Service book-service -Since 1h" -ForegroundColor White
        Write-Host ""
        exit 0
    }

    # Validate service name
    if ($Service -and $Service -notin $availableServices) {
        Write-Host "Invalid service name: $Service" -ForegroundColor Red
        Write-Host "Available services: $($availableServices -join ', ')" -ForegroundColor Yellow
        exit 1
    }

    # Prepare logs arguments
    $logsArgs = @("logs")
    
    if ($Follow) {
        $logsArgs += "-f"
    }
    
    if ($Tail -gt 0) {
        $logsArgs += "--tail", $Tail.ToString()
    }
    
    if ($Since) {
        $logsArgs += "--since", $Since
    }
    
    if ($Timestamps) {
        $logsArgs += "--timestamps"
    }
    
    if ($NoColor) {
        $logsArgs += "--no-color"
    }

    # Add service name or show all
    if ($Service) {
        $logsArgs += $Service
        Write-Host "Viewing logs for service: $Service" -ForegroundColor Green
    } else {
        Write-Host "Viewing logs for all services" -ForegroundColor Green
    }

    # Display log options
    if ($Follow) {
        Write-Host "Following logs (Press Ctrl+C to stop)..." -ForegroundColor Yellow
    } else {
        Write-Host "Showing last $Tail lines..." -ForegroundColor Yellow
    }
    
    if ($Since) {
        Write-Host "Since: $Since" -ForegroundColor Gray
    }
    
    Write-Host ""

    # Special handling for database logs
    if ($Service -and $Service -like "*-db") {
        Write-Host "Tip: Database logs may be verbose. Use -Tail to limit output." -ForegroundColor Cyan
        Write-Host ""
    }

    # Show logs
    & docker-compose @logsArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Logs command completed with exit code $LASTEXITCODE"
    }

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Failed to view logs!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

# If we get here and it was a follow operation, user pressed Ctrl+C
if ($Follow) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Log following stopped" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
}