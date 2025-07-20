# Stop All Services Script for LibraryApp
# This script stops all running LibraryApp containers

param(
    [switch]$RemoveVolumes,
    [switch]$RemoveImages,
    [switch]$Force,
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Stop All Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"

try {
    # Check current status
    Write-Host "Checking current service status..." -ForegroundColor Yellow
    $runningContainers = docker-compose ps -q
    
    if (-not $runningContainers) {
        Write-Host "No LibraryApp containers are currently running." -ForegroundColor Green
        exit 0
    }

    # Show current status
    Write-Host "`nCurrent container status:" -ForegroundColor Yellow
    docker-compose ps

    # Prepare stop arguments
    $stopArgs = @("down")
    
    if ($RemoveVolumes) {
        $stopArgs += "--volumes"
        Write-Host "`n⚠ WARNING: This will remove all data volumes!" -ForegroundColor Red
        if (-not $Force) {
            $confirm = Read-Host "Are you sure you want to remove all data? (y/N)"
            if ($confirm -ne "y" -and $confirm -ne "Y") {
                Write-Host "Operation cancelled." -ForegroundColor Yellow
                exit 0
            }
        }
    }
    
    if ($RemoveImages) {
        $stopArgs += "--rmi", "all"
        Write-Host "`n⚠ WARNING: This will remove all built images!" -ForegroundColor Red
        if (-not $Force) {
            $confirm = Read-Host "Are you sure you want to remove all images? (y/N)"
            if ($confirm -ne "y" -and $confirm -ne "Y") {
                Write-Host "Operation cancelled." -ForegroundColor Yellow
                exit 0
            }
        }
    }

    $stopArgs += "--remove-orphans"

    if ($Verbose) {
        Write-Host "`nCommand: docker-compose $($stopArgs -join ' ')" -ForegroundColor Gray
    }

    # Stop services
    Write-Host "`nStopping LibraryApp services..." -ForegroundColor Yellow
    $stopStartTime = Get-Date
    & docker-compose @stopArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Stop command completed with exit code $LASTEXITCODE"
    }
    
    $stopEndTime = Get-Date
    $stopDuration = $stopEndTime - $stopStartTime

    # Verify services are stopped
    Write-Host "`nVerifying services are stopped..." -ForegroundColor Yellow
    $remainingContainers = docker ps --filter "name=libraryapp" --format "{{.Names}}" 2>$null
    
    if ($remainingContainers) {
        Write-Warning "Some containers are still running:"
        $remainingContainers | ForEach-Object { Write-Host "  • $_" -ForegroundColor Yellow }
        
        if ($Force) {
            Write-Host "`nForce stopping remaining containers..." -ForegroundColor Red
            $remainingContainers | ForEach-Object { docker stop $_ 2>$null }
            $remainingContainers | ForEach-Object { docker rm $_ 2>$null }
        }
    } else {
        Write-Host "✓ All LibraryApp containers stopped" -ForegroundColor Green
    }

    # Show remaining resources
    Write-Host "`nResource status:" -ForegroundColor Yellow
    
    # Check volumes
    $libraryVolumes = docker volume ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null
    if ($libraryVolumes) {
        Write-Host "Remaining volumes:" -ForegroundColor Cyan
        $libraryVolumes | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No LibraryApp volumes remaining" -ForegroundColor Green
    }
    
    # Check images
    $libraryImages = docker images --filter "reference=*libraryapp*" --format "{{.Repository}}:{{.Tag}}" 2>$null
    if ($libraryImages) {
        Write-Host "Remaining images:" -ForegroundColor Cyan
        $libraryImages | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No LibraryApp images remaining" -ForegroundColor Green
    }

    # Check networks
    $libraryNetworks = docker network ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null
    if ($libraryNetworks) {
        Write-Host "Remaining networks:" -ForegroundColor Cyan
        $libraryNetworks | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No LibraryApp networks remaining" -ForegroundColor Green
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Services stopped successfully!" -ForegroundColor Green
    Write-Host "Stopped in $($stopDuration.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    if (-not $RemoveVolumes -and -not $RemoveImages) {
        Write-Host "To completely clean up:" -ForegroundColor Yellow
        Write-Host "• Remove data: .\scripts\stop-all.ps1 -RemoveVolumes" -ForegroundColor White
        Write-Host "• Remove images: .\scripts\stop-all.ps1 -RemoveImages" -ForegroundColor White
        Write-Host "• Full cleanup: .\scripts\clean.ps1" -ForegroundColor White
    }

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Failed to stop services!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}