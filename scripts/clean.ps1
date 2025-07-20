# Clean Up Script for LibraryApp
# This script performs a complete cleanup of all LibraryApp Docker resources

param(
    [switch]$All,
    [switch]$Containers,
    [switch]$Images,
    [switch]$Volumes,
    [switch]$Networks,
    [switch]$BuildCache,
    [switch]$Force,
    [switch]$DryRun,
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Display banner
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LibraryApp Docker Cleanup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set environment variables
$env:COMPOSE_FILE = "docker-compose.yml;docker-compose.override.yml"

# If no specific options are provided, default to All
if (-not ($Containers -or $Images -or $Volumes -or $Networks -or $BuildCache)) {
    $All = $true
}

if ($All) {
    $Containers = $true
    $Images = $true
    $Volumes = $true
    $Networks = $true
    $BuildCache = $true
}

try {
    # Check Docker status
    Write-Host "Checking Docker status..." -ForegroundColor Yellow
    $dockerInfo = docker info 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker is not running or accessible. Please start Docker Desktop and try again."
    }
    Write-Host "✓ Docker is running" -ForegroundColor Green

    # Show current state
    Write-Host "`nCurrent LibraryApp resources:" -ForegroundColor Yellow
    
    # Check containers
    $libraryContainers = docker ps -a --filter "name=libraryapp" --format "{{.Names}}" 2>$null
    if ($libraryContainers) {
        Write-Host "Containers found:" -ForegroundColor Cyan
        $libraryContainers | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No containers found" -ForegroundColor Green
    }
    
    # Check images
    $libraryImages = docker images --filter "reference=*libraryapp*" --format "{{.Repository}}:{{.Tag}}" 2>$null
    if ($libraryImages) {
        Write-Host "Images found:" -ForegroundColor Cyan
        $libraryImages | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No images found" -ForegroundColor Green
    }
    
    # Check volumes
    $libraryVolumes = docker volume ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null
    if ($libraryVolumes) {
        Write-Host "Volumes found:" -ForegroundColor Cyan
        $libraryVolumes | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No volumes found" -ForegroundColor Green
    }
    
    # Check networks
    $libraryNetworks = docker network ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null
    if ($libraryNetworks) {
        Write-Host "Networks found:" -ForegroundColor Cyan
        $libraryNetworks | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
    } else {
        Write-Host "✓ No networks found" -ForegroundColor Green
    }

    # If dry run, show what would be cleaned
    if ($DryRun) {
        Write-Host "`n========================================" -ForegroundColor Yellow
        Write-Host "DRY RUN - The following would be cleaned:" -ForegroundColor Yellow
        Write-Host "========================================" -ForegroundColor Yellow
        
        if ($Containers -and $libraryContainers) {
            Write-Host "Would remove containers:" -ForegroundColor Red
            $libraryContainers | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
        }
        
        if ($Images -and $libraryImages) {
            Write-Host "Would remove images:" -ForegroundColor Red
            $libraryImages | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
        }
        
        if ($Volumes -and $libraryVolumes) {
            Write-Host "Would remove volumes (DATA LOSS!):" -ForegroundColor Red
            $libraryVolumes | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
        }
        
        if ($Networks -and $libraryNetworks) {
            Write-Host "Would remove networks:" -ForegroundColor Red
            $libraryNetworks | ForEach-Object { Write-Host "  • $_" -ForegroundColor White }
        }
        
        if ($BuildCache) {
            Write-Host "Would clear build cache" -ForegroundColor Red
        }
        
        Write-Host "`nRun without -DryRun to perform cleanup" -ForegroundColor Yellow
        exit 0
    }

    # Warning for destructive operations
    if (($Volumes -and $libraryVolumes) -or ($Images -and $libraryImages)) {
        Write-Host "`n⚠ WARNING: This operation will:" -ForegroundColor Red
        if ($Volumes -and $libraryVolumes) {
            Write-Host "  • Remove all database data (PERMANENT DATA LOSS!)" -ForegroundColor Red
        }
        if ($Images -and $libraryImages) {
            Write-Host "  • Remove all built images (will need to rebuild)" -ForegroundColor Red
        }
        
        if (-not $Force) {
            $confirm = Read-Host "`nAre you sure you want to continue? (y/N)"
            if ($confirm -ne "y" -and $confirm -ne "Y") {
                Write-Host "Operation cancelled." -ForegroundColor Yellow
                exit 0
            }
        }
    }

    Write-Host "`nStarting cleanup..." -ForegroundColor Yellow
    $cleanupStartTime = Get-Date

    # Stop and remove containers first
    if ($Containers) {
        Write-Host "`nStopping and removing containers..." -ForegroundColor Yellow
        
        # Use docker-compose down to properly stop services
        $downArgs = @("down", "--remove-orphans")
        if ($Volumes) {
            $downArgs += "--volumes"
        }
        
        if ($Verbose) {
            Write-Host "Command: docker-compose $($downArgs -join ' ')" -ForegroundColor Gray
        }
        
        & docker-compose @downArgs 2>$null
        
        # Force remove any remaining containers
        if ($libraryContainers) {
            $libraryContainers | ForEach-Object {
                if ($Verbose) {
                    Write-Host "Removing container: $_" -ForegroundColor Gray
                }
                docker rm -f $_ 2>$null
            }
        }
        
        Write-Host "✓ Containers removed" -ForegroundColor Green
    }

    # Remove images
    if ($Images -and $libraryImages) {
        Write-Host "`nRemoving images..." -ForegroundColor Yellow
        
        $libraryImages | ForEach-Object {
            if ($Verbose) {
                Write-Host "Removing image: $_" -ForegroundColor Gray
            }
            docker rmi -f $_ 2>$null
        }
        
        Write-Host "✓ Images removed" -ForegroundColor Green
    }

    # Remove volumes (if not already done by docker-compose down --volumes)
    if ($Volumes -and $libraryVolumes -and -not $Containers) {
        Write-Host "`nRemoving volumes..." -ForegroundColor Yellow
        
        $libraryVolumes | ForEach-Object {
            if ($Verbose) {
                Write-Host "Removing volume: $_" -ForegroundColor Gray
            }
            docker volume rm -f $_ 2>$null
        }
        
        Write-Host "✓ Volumes removed" -ForegroundColor Green
    }

    # Remove networks
    if ($Networks -and $libraryNetworks) {
        Write-Host "`nRemoving networks..." -ForegroundColor Yellow
        
        $libraryNetworks | ForEach-Object {
            if ($Verbose) {
                Write-Host "Removing network: $_" -ForegroundColor Gray
            }
            docker network rm $_ 2>$null
        }
        
        Write-Host "✓ Networks removed" -ForegroundColor Green
    }

    # Clean build cache
    if ($BuildCache) {
        Write-Host "`nCleaning build cache..." -ForegroundColor Yellow
        
        if ($Verbose) {
            Write-Host "Command: docker builder prune -f" -ForegroundColor Gray
        }
        
        docker builder prune -f 2>$null
        Write-Host "✓ Build cache cleared" -ForegroundColor Green
    }

    # Additional cleanup - remove unused resources
    Write-Host "`nCleaning unused Docker resources..." -ForegroundColor Yellow
    docker system prune -f 2>$null
    Write-Host "✓ Unused resources cleaned" -ForegroundColor Green

    $cleanupEndTime = Get-Date
    $cleanupDuration = $cleanupEndTime - $cleanupStartTime

    # Verify cleanup
    Write-Host "`nVerifying cleanup..." -ForegroundColor Yellow
    
    $remainingContainers = docker ps -a --filter "name=libraryapp" --format "{{.Names}}" 2>$null
    $remainingImages = docker images --filter "reference=*libraryapp*" --format "{{.Repository}}:{{.Tag}}" 2>$null
    $remainingVolumes = docker volume ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null
    $remainingNetworks = docker network ls --filter "name=libraryapp" --format "{{.Name}}" 2>$null

    if ($remainingContainers -or $remainingImages -or $remainingVolumes -or $remainingNetworks) {
        Write-Host "Some resources may still exist:" -ForegroundColor Yellow
        if ($remainingContainers) { $remainingContainers | ForEach-Object { Write-Host "  Container: $_" -ForegroundColor Yellow } }
        if ($remainingImages) { $remainingImages | ForEach-Object { Write-Host "  Image: $_" -ForegroundColor Yellow } }
        if ($remainingVolumes) { $remainingVolumes | ForEach-Object { Write-Host "  Volume: $_" -ForegroundColor Yellow } }
        if ($remainingNetworks) { $remainingNetworks | ForEach-Object { Write-Host "  Network: $_" -ForegroundColor Yellow } }
    } else {
        Write-Host "✓ All LibraryApp resources cleaned" -ForegroundColor Green
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Cleanup completed successfully!" -ForegroundColor Green
    Write-Host "Cleanup completed in $($cleanupDuration.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To rebuild and start:" -ForegroundColor Yellow
    Write-Host "• Build: .\scripts\build-all.ps1" -ForegroundColor White
    Write-Host "• Start: .\scripts\start-dev.ps1" -ForegroundColor White

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "Cleanup failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}