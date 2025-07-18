# Setup Local Development Environment for Library Management System
# This script sets up the local development environment

param(
    [switch]$SkipDockerInstall,
    [switch]$SkipRepoClone,
    [string]$RepoUrl = "https://github.com/navinprabhu/claude_libraryApp.git"
)

Write-Host "=== Library Management System - Local Development Setup ===" -ForegroundColor Green

# Check if running as administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsIdentity]::GetCurrent().Name)

if (-not $isAdmin) {
    Write-Host "Warning: Running without administrator privileges. Some installations may fail." -ForegroundColor Yellow
}

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Check for required tools
Write-Host "`n1. Checking for required tools..." -ForegroundColor Cyan

# Check .NET SDK
if (Test-Command "dotnet") {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check Git
if (Test-Command "git") {
    $gitVersion = git --version
    Write-Host "✓ Git found: $gitVersion" -ForegroundColor Green
} else {
    Write-Host "✗ Git not found. Please install Git from https://git-scm.com/" -ForegroundColor Red
    exit 1
}

# Check Docker Desktop
if (-not $SkipDockerInstall) {
    Write-Host "`n2. Checking Docker Desktop..." -ForegroundColor Cyan
    
    if (Test-Command "docker") {
        try {
            $dockerVersion = docker --version
            Write-Host "✓ Docker found: $dockerVersion" -ForegroundColor Green
            
            # Test Docker daemon
            docker info > $null 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Docker daemon is running" -ForegroundColor Green
            } else {
                Write-Host "⚠ Docker is installed but daemon is not running. Please start Docker Desktop." -ForegroundColor Yellow
            }
        } catch {
            Write-Host "⚠ Docker command failed. Please ensure Docker Desktop is properly installed and running." -ForegroundColor Yellow
        }
    } else {
        Write-Host "✗ Docker not found." -ForegroundColor Red
        Write-Host "Please install Docker Desktop from https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
        Write-Host "After installation, restart your machine and run this script again." -ForegroundColor Yellow
        
        $response = Read-Host "Do you want to continue without Docker? (y/N)"
        if ($response -ne "y" -and $response -ne "Y") {
            exit 1
        }
    }
} else {
    Write-Host "⏭ Skipping Docker installation check" -ForegroundColor Yellow
}

# Clone repository
if (-not $SkipRepoClone) {
    Write-Host "`n3. Setting up repository..." -ForegroundColor Cyan
    
    $repoName = "claude_libraryApp"
    
    if (Test-Path $repoName) {
        Write-Host "Repository directory already exists. Pulling latest changes..." -ForegroundColor Yellow
        Set-Location $repoName
        git pull origin main
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to pull latest changes. Please check your git configuration." -ForegroundColor Red
        }
        Set-Location ..
    } else {
        Write-Host "Cloning repository: $RepoUrl" -ForegroundColor Cyan
        git clone $RepoUrl
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Repository cloned successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to clone repository. Please check the URL and your internet connection." -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host "⏭ Skipping repository clone" -ForegroundColor Yellow
}

# Setup environment variables
Write-Host "`n4. Setting up environment variables..." -ForegroundColor Cyan

$envFile = ".env.local"
if (-not (Test-Path $envFile)) {
    $envContent = @"
# Library Management System - Local Development Environment
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000

# Database Configuration
CONNECTION_STRING_DEFAULT=Server=(localdb)\mssqllocaldb;Database=LibraryManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true
CONNECTION_STRING_BOOKS=Server=(localdb)\mssqllocaldb;Database=LibraryBooks;Trusted_Connection=true;MultipleActiveResultSets=true
CONNECTION_STRING_MEMBERS=Server=(localdb)\mssqllocaldb;Database=LibraryMembers;Trusted_Connection=true;MultipleActiveResultSets=true
CONNECTION_STRING_BORROWING=Server=(localdb)\mssqllocaldb;Database=LibraryBorrowing;Trusted_Connection=true;MultipleActiveResultSets=true

# JWT Configuration
JWT_SECRET_KEY=your-256-bit-secret-key-here-please-change-in-production
JWT_ISSUER=LibraryApp
JWT_AUDIENCE=LibraryApp.Users
JWT_EXPIRATION_MINUTES=60

# CORS Configuration
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:4200,https://localhost:3000,https://localhost:4200

# Logging Configuration
LOGGING_LEVEL=Information
SERILOG_MINIMUM_LEVEL=Information

# External Services (if needed)
# REDIS_CONNECTION_STRING=localhost:6379
# RABBITMQ_CONNECTION_STRING=amqp://guest:guest@localhost:5672/
"@

    Set-Content -Path $envFile -Value $envContent
    Write-Host "✓ Created $envFile with default configuration" -ForegroundColor Green
} else {
    Write-Host "✓ Environment file already exists: $envFile" -ForegroundColor Green
}

# Restore NuGet packages
Write-Host "`n5. Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ NuGet packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "`n6. Building solution..." -ForegroundColor Cyan
dotnet build --no-restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Solution built successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Setup Complete! ===" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Review and update the .env.local file with your specific configuration"
Write-Host "2. Run 'dotnet run' in each service directory to start individual services"
Write-Host "3. Or use the build-and-run.ps1 script to start all services with Docker Compose"
Write-Host "4. Access the applications at the URLs specified in your configuration"

Write-Host "`nUseful commands:" -ForegroundColor Cyan
Write-Host "- Build all projects: dotnet build"
Write-Host "- Run tests: dotnet test"
Write-Host "- Clean solution: dotnet clean"
Write-Host "- Update packages: dotnet list package --outdated"

Write-Host "`nHappy coding! 🚀" -ForegroundColor Green