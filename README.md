# Library Management System - Microservices Solution

A scalable microservices-based Library Management System built with .NET 8, following Domain-Driven Design (DDD) and microservices architecture patterns.

## 🏗️ Solution Structure

```
LibraryApp/
├── LibraryApp.sln                    # Main solution file
├── LibraryApp.Shared.Models/         # Shared DTOs, Enums, and Models
├── LibraryApp.Shared.Infrastructure/ # Common infrastructure components
├── LibraryApp.Shared.Events/         # Event models for service communication
├── setup-local-dev.ps1              # Development environment setup script
├── build-and-run.ps1                # Build and run script with Docker Compose
└── README.md                         # This file
```

## 📦 Shared Libraries

### LibraryApp.Shared.Models
Contains shared data models, DTOs, and enums used across all microservices:

- **DTOs**: BookDto, CreateBookDto, UpdateBookDto, MemberDto, CreateMemberDto, BorrowingRecordDto, BorrowRequestDto
- **Enums**: BookStatus (Available, Borrowed)
- **Common Classes**: ApiResponse<T>, PagedResult<T>

### LibraryApp.Shared.Infrastructure
Provides common infrastructure components and utilities:

- **Interfaces**: IRepository<T>, IJwtTokenService
- **Base Classes**: BaseRepository<T>
- **Middleware**: CorrelationIdMiddleware, RequestLoggingMiddleware
- **Extensions**: ServiceCollectionExtensions, ApplicationBuilderExtensions
- **Configuration**: DatabaseConfiguration helpers

### LibraryApp.Shared.Events
Event models for inter-service communication:

- **Book Events**: BookCreatedEvent, BookUpdatedEvent, BookDeletedEvent
- **Member Events**: MemberRegisteredEvent, MemberUpdatedEvent, MemberDeactivatedEvent
- **Borrowing Events**: BookBorrowedEvent, BookReturnedEvent, BookOverdueEvent

## 🛠️ Technology Stack

- **.NET 8**: Main framework
- **Entity Framework Core**: Data access layer
- **SQL Server**: Primary database
- **Docker**: Containerization
- **Redis**: Caching (optional)
- **RabbitMQ**: Message queuing (optional)

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Docker Desktop
- Git
- PowerShell (for scripts)

### Quick Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/navinprabhu/claude_libraryApp.git
   cd claude_libraryApp
   ```

2. **Run the setup script**:
   ```powershell
   .\setup-local-dev.ps1
   ```

3. **Build and run services**:
   ```powershell
   .\build-and-run.ps1 -DetachedMode
   ```

### Manual Setup

1. **Restore packages**:
   ```bash
   dotnet restore
   ```

2. **Build solution**:
   ```bash
   dotnet build
   ```

3. **Run tests**:
   ```bash
   dotnet test
   ```

## 📋 Development Scripts

### setup-local-dev.ps1
Sets up the local development environment:
- Checks for required tools (.NET, Git, Docker)
- Clones repository (optional)
- Creates environment configuration file
- Restores NuGet packages
- Builds the solution

**Usage**:
```powershell
# Full setup
.\setup-local-dev.ps1

# Skip Docker installation check
.\setup-local-dev.ps1 -SkipDockerInstall

# Skip repository clone
.\setup-local-dev.ps1 -SkipRepoClone
```

### build-and-run.ps1
Builds and runs the solution with Docker Compose:
- Builds all projects
- Runs tests
- Starts infrastructure services (SQL Server, Redis, RabbitMQ)
- Creates Docker Compose configuration

**Usage**:
```powershell
# Build and run in detached mode
.\build-and-run.ps1 -DetachedMode

# Skip build, just run
.\build-and-run.ps1 -NoBuild

# Clean build and run
.\build-and-run.ps1 -CleanBuild

# Show logs
.\build-and-run.ps1 -DetachedMode -ShowLogs
```

## 🏛️ Architecture Patterns

### Microservices Architecture
- **Book Service**: Manages book catalog and inventory
- **Member Service**: Handles member registration and management
- **Borrowing Service**: Manages book borrowing and returns

### Shared Kernel Pattern
- Common models, interfaces, and infrastructure in shared libraries
- Consistent data models across services
- Reusable middleware and extensions

### Repository Pattern
- Generic repository interface and base implementation
- Consistent data access patterns
- Testable and mockable data layer

### Event-Driven Communication
- Domain events for service communication
- Eventual consistency between services
- Decoupled service interactions

## 🔧 Configuration

### Database Connections
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LibraryManagementDB;Trusted_Connection=true;MultipleActiveResultSets=true",
    "BooksDatabase": "Server=(localdb)\\mssqllocaldb;Database=LibraryBooks;Trusted_Connection=true;MultipleActiveResultSets=true",
    "MembersDatabase": "Server=(localdb)\\mssqllocaldb;Database=LibraryMembers;Trusted_Connection=true;MultipleActiveResultSets=true",
    "BorrowingDatabase": "Server=(localdb)\\mssqllocaldb;Database=LibraryBorrowing;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Environment Variables
See `.env.local` file created by the setup script for all available environment variables.

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Next Steps

1. **Implement Microservices**:
   - Create BookService Web API
   - Create MemberService Web API
   - Create BorrowingService Web API

2. **Add API Gateway**:
   - Implement Ocelot or YARP gateway
   - Configure routing and load balancing

3. **Implement Authentication**:
   - Add JWT token service implementation
   - Configure OAuth/OpenID Connect

4. **Add Service Discovery**:
   - Implement Consul or similar service discovery
   - Configure health checks

5. **Add Monitoring**:
   - Implement logging with Serilog
   - Add metrics with Application Insights
   - Configure distributed tracing

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🛟 Support

For questions and support:
- Create an issue in the GitHub repository
- Review the troubleshooting section in the documentation
- Check the setup scripts for common configuration issues

---

**Happy Coding! 🚀**