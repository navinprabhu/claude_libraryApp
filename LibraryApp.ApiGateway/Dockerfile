# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["LibraryApp.ApiGateway/LibraryApp.ApiGateway.csproj", "LibraryApp.ApiGateway/"]
COPY ["LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj", "LibraryApp.Shared.Infrastructure/"]
COPY ["LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj", "LibraryApp.Shared.Models/"]
RUN dotnet restore "LibraryApp.ApiGateway/LibraryApp.ApiGateway.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/LibraryApp.ApiGateway"
RUN dotnet build "LibraryApp.ApiGateway.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "LibraryApp.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' --uid 1000 appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# Switch to the non-root user
USER appuser

# Copy the published application
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 5000
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5000/health/gateway || exit 1

# Start the application
ENTRYPOINT ["dotnet", "LibraryApp.ApiGateway.dll"]