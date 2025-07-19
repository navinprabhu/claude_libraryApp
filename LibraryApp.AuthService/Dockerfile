# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001
EXPOSE 80

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["LibraryApp.AuthService/LibraryApp.AuthService.csproj", "LibraryApp.AuthService/"]
COPY ["LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj", "LibraryApp.Shared.Models/"]
COPY ["LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj", "LibraryApp.Shared.Infrastructure/"]
COPY ["LibraryApp.Shared.Events/LibraryApp.Shared.Events.csproj", "LibraryApp.Shared.Events/"]

# Restore dependencies
RUN dotnet restore "LibraryApp.AuthService/LibraryApp.AuthService.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/LibraryApp.AuthService"
RUN dotnet build "LibraryApp.AuthService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LibraryApp.AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5001

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "LibraryApp.AuthService.dll"]