# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002
EXPOSE 40002

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Development stage for hot reload and debugging
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

# Install curl for health checks in development
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy project files for dependency restoration
COPY ["LibraryApp.BookService/LibraryApp.BookService.csproj", "LibraryApp.BookService/"]
COPY ["LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj", "LibraryApp.Shared.Models/"]
COPY ["LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj", "LibraryApp.Shared.Infrastructure/"]
COPY ["LibraryApp.Shared.Events/LibraryApp.Shared.Events.csproj", "LibraryApp.Shared.Events/"]

# Restore dependencies
RUN dotnet restore "LibraryApp.BookService/LibraryApp.BookService.csproj"

# Copy source code
COPY . .

# Install dotnet tools for development
RUN dotnet tool install --global dotnet-ef
RUN dotnet tool install --global dotnet-watch
ENV PATH="${PATH}:/root/.dotnet/tools"

# Set working directory to the service
WORKDIR /app/LibraryApp.BookService

# Development environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5002
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Default command for development (can be overridden)
CMD ["dotnet", "watch", "run", "--urls", "http://+:5002"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["LibraryApp.BookService/LibraryApp.BookService.csproj", "LibraryApp.BookService/"]
COPY ["LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj", "LibraryApp.Shared.Models/"]
COPY ["LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj", "LibraryApp.Shared.Infrastructure/"]
COPY ["LibraryApp.Shared.Events/LibraryApp.Shared.Events.csproj", "LibraryApp.Shared.Events/"]

# Restore dependencies
RUN dotnet restore "LibraryApp.BookService/LibraryApp.BookService.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/LibraryApp.BookService"
RUN dotnet build "LibraryApp.BookService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "LibraryApp.BookService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Production runtime stage
FROM base AS production
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Production environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5002

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5002/health || exit 1

ENTRYPOINT ["dotnet", "LibraryApp.BookService.dll"]