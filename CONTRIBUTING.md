# Contributing to LibraryApp

Thank you for your interest in contributing to LibraryApp! This document provides guidelines and information for contributors.

## Table of Contents
- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Code Standards](#code-standards)
- [Testing](#testing)
- [Documentation](#documentation)
- [Pull Request Process](#pull-request-process)
- [Release Process](#release-process)

## Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

### Our Standards

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

## Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **Docker Desktop** (for containerization)
- **Git** (for version control)
- **Visual Studio 2022** or **VS Code** (recommended IDEs)

### Development Environment Setup

1. **Fork and Clone the Repository**
   ```bash
   git clone https://github.com/your-username/libraryapp.git
   cd libraryapp
   ```

2. **Set Up Local Environment**
   ```bash
   # Restore NuGet packages
   dotnet restore LibraryApp.sln
   
   # Build the solution
   dotnet build LibraryApp.sln
   
   # Run tests
   dotnet test
   ```

3. **Start Services with Docker**
   ```bash
   # Start all services
   docker-compose up -d
   
   # View logs
   docker-compose logs -f
   ```

4. **Access Services**
   - API Gateway: http://localhost:5000
   - Auth Service: http://localhost:5001
   - Book Service: http://localhost:5002
   - Member Service: http://localhost:5003

## Development Workflow

### Branch Strategy

We use **GitFlow** branching strategy:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `release/*` - Release preparation
- `hotfix/*` - Critical production fixes

### Creating a Feature Branch

```bash
# Start from develop
git checkout develop
git pull origin develop

# Create feature branch
git checkout -b feature/your-feature-name

# Work on your feature
# ... make changes ...

# Commit your changes
git add .
git commit -m "feat: add your feature description"

# Push to your fork
git push origin feature/your-feature-name
```

### Commit Message Convention

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat` - New features
- `fix` - Bug fixes
- `docs` - Documentation changes
- `style` - Code style changes (formatting, etc.)
- `refactor` - Code refactoring
- `test` - Adding or modifying tests
- `chore` - Maintenance tasks

**Examples:**
```
feat(auth): add JWT token refresh functionality
fix(book): resolve book availability calculation bug
docs: update API documentation for member endpoints
test(member): add integration tests for member registration
```

## Code Standards

### C# Coding Standards

1. **Follow Microsoft C# Coding Conventions**
   - Use PascalCase for public members
   - Use camelCase for private fields
   - Use meaningful variable names

2. **Architecture Patterns**
   - Repository Pattern for data access
   - Dependency Injection for loose coupling
   - Clean Architecture principles

3. **Code Quality**
   ```csharp
   // Good: Clear and descriptive
   public async Task<ApiResponse<Member>> GetMemberByIdAsync(int memberId)
   {
       var member = await _memberRepository.GetByIdAsync(memberId);
       return member != null 
           ? ApiResponse<Member>.SuccessResponse(member)
           : ApiResponse<Member>.ErrorResponse("Member not found", 404);
   }
   
   // Avoid: Unclear and poorly structured
   public async Task<object> Get(int id)
   {
       var m = await _repo.Get(id);
       return m ?? "not found";
   }
   ```

4. **Error Handling**
   - Use try-catch blocks appropriately
   - Return structured error responses
   - Log errors with appropriate context

5. **Documentation**
   - Use XML documentation comments for public APIs
   - Include parameter descriptions and return values
   ```csharp
   /// <summary>
   /// Retrieves a member by their unique identifier
   /// </summary>
   /// <param name="memberId">The unique identifier of the member</param>
   /// <returns>The member information if found, null otherwise</returns>
   public async Task<Member?> GetMemberByIdAsync(int memberId)
   ```

### API Design Standards

1. **RESTful Endpoints**
   ```
   GET    /api/books           - Get all books
   GET    /api/books/{id}      - Get specific book
   POST   /api/books           - Create new book
   PUT    /api/books/{id}      - Update book
   DELETE /api/books/{id}      - Delete book
   ```

2. **Response Format**
   ```csharp
   // Consistent response structure
   public class ApiResponse<T>
   {
       public bool Success { get; set; }
       public T? Data { get; set; }
       public string Message { get; set; }
       public int StatusCode { get; set; }
       public DateTime Timestamp { get; set; }
   }
   ```

3. **HTTP Status Codes**
   - 200 OK - Successful GET/PUT
   - 201 Created - Successful POST
   - 204 No Content - Successful DELETE
   - 400 Bad Request - Invalid input
   - 401 Unauthorized - Authentication required
   - 403 Forbidden - Insufficient permissions
   - 404 Not Found - Resource not found
   - 500 Internal Server Error - Server error

## Testing

### Test Categories

1. **Unit Tests**
   - Test individual components in isolation
   - Mock external dependencies
   - Fast execution
   ```csharp
   [Test]
   public async Task GetMemberByIdAsync_ValidId_ReturnsMember()
   {
       // Arrange
       var memberId = 1;
       var expectedMember = new Member { Id = memberId, Name = "Test" };
       _mockRepository.Setup(r => r.GetByIdAsync(memberId))
                     .ReturnsAsync(expectedMember);
       
       // Act
       var result = await _memberService.GetMemberByIdAsync(memberId);
       
       // Assert
       Assert.IsTrue(result.Success);
       Assert.AreEqual(expectedMember.Id, result.Data.Id);
   }
   ```

2. **Integration Tests**
   - Test service interactions
   - Use test databases
   - Validate end-to-end scenarios

3. **Performance Tests**
   - Load testing
   - Stress testing
   - Memory usage validation

### Test Requirements

- **Minimum 80% code coverage** for new features
- **All tests must pass** before merging
- **Integration tests** for API endpoints
- **Performance tests** for critical paths

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Unit

# Run tests for specific service
dotnet test LibraryApp.BookService.Tests/
```

## Documentation

### Required Documentation

1. **Code Documentation**
   - XML comments for public APIs
   - README files for each service
   - Architecture Decision Records (ADRs)

2. **API Documentation**
   - OpenAPI/Swagger specifications
   - Endpoint descriptions
   - Request/response examples

3. **Deployment Documentation**
   - Environment setup guides
   - Configuration instructions
   - Troubleshooting guides

### Writing Documentation

- Use clear, concise language
- Include code examples
- Provide step-by-step instructions
- Keep documentation up-to-date with code changes

## Pull Request Process

### Before Creating a PR

1. **Ensure all tests pass**
   ```bash
   dotnet test
   ```

2. **Run code analysis**
   ```bash
   dotnet build --verbosity normal
   ```

3. **Update documentation** if needed

4. **Rebase your branch** if necessary
   ```bash
   git rebase develop
   ```

### PR Checklist

- [ ] Code follows project standards
- [ ] All tests pass
- [ ] Documentation updated
- [ ] No merge conflicts
- [ ] Descriptive PR title and description
- [ ] Linked to related issues

### PR Review Process

1. **Automated Checks**
   - CI/CD pipeline runs
   - Code coverage analysis
   - Security scanning

2. **Manual Review**
   - Code quality assessment
   - Architecture review
   - Security considerations

3. **Approval Requirements**
   - At least 2 approvals for main branch
   - All conversations resolved
   - All checks passing

## Release Process

### Semantic Versioning

We follow [Semantic Versioning](https://semver.org/):

- `MAJOR.MINOR.PATCH`
- **MAJOR** - Breaking changes
- **MINOR** - New features (backward compatible)
- **PATCH** - Bug fixes (backward compatible)

### Release Steps

1. **Create Release Branch**
   ```bash
   git checkout develop
   git checkout -b release/v1.2.0
   ```

2. **Update Version Numbers**
   - Update project files
   - Update documentation
   - Update changelog

3. **Final Testing**
   - Run full test suite
   - Performance testing
   - Security scanning

4. **Create Release PR**
   - Merge to main
   - Tag release
   - Deploy to production

## Getting Help

### Communication Channels

- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - General questions and ideas
- **Project Wiki** - Documentation and guides

### Maintainer Response Times

- **Critical bugs** - Within 24 hours
- **General questions** - Within 3 business days
- **Feature requests** - Within 1 week

### Security Issues

For security vulnerabilities:
1. Do NOT open a public issue
2. Email security@libraryapp.com
3. Include detailed description
4. Wait for maintainer response

## Recognition

Contributors will be recognized in:
- Release notes
- Contributors section
- Project documentation

Thank you for contributing to LibraryApp! 🎉