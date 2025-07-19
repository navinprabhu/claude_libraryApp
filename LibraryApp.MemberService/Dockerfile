# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY LibraryApp.MemberService.csproj .
COPY ../LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj ../LibraryApp.Shared.Models/
COPY ../LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj ../LibraryApp.Shared.Infrastructure/
COPY ../LibraryApp.Shared.Events/LibraryApp.Shared.Events.csproj ../LibraryApp.Shared.Events/

RUN dotnet restore

# Copy the entire source code
COPY . .
COPY ../LibraryApp.Shared.Models/ ../LibraryApp.Shared.Models/
COPY ../LibraryApp.Shared.Infrastructure/ ../LibraryApp.Shared.Infrastructure/
COPY ../LibraryApp.Shared.Events/ ../LibraryApp.Shared.Events/

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 5003

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5003

# Define the entry point for the application
ENTRYPOINT ["dotnet", "LibraryApp.MemberService.dll"]

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5003/health || exit 1