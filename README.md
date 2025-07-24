# LibraryApp - Modern Microservices Library Management System

[![CI/CD Pipeline](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/ci.yml/badge.svg)](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/ci.yml)
[![Deployment](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/cd.yml/badge.svg)](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/cd.yml)
[![Security Scan](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/security.yml/badge.svg)](https://github.com/navinprabhu/claude_libraryApp/actions/workflows/security.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.1.0-blue.svg)](https://reactjs.org/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-blue.svg)](https://www.docker.com/)
[![Azure Ready](https://img.shields.io/badge/Azure-Ready-blue.svg)](https://azure.microsoft.com/)

A modern, cloud-native library management system built with .NET 8 microservices architecture and React frontend, featuring comprehensive CI/CD pipelines, monitoring, and enterprise-grade security. Ready for Azure cloud deployment.

## 🏗️ System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Web     │    │  Mobile Apps    │    │  Third-party    │
│   Application   │    │  (Future)       │    │  Integrations   │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────▼──────────────┐
                    │     API Gateway            │
                    │     (Ocelot)               │
                    │  - Routing & Load Balance  │
                    │  - Rate Limiting           │
                    │  - Authentication          │
                    └─────────────┬──────────────┘
                                  │
           ┌──────────────────────┼──────────────────────┐
           │                      │                      │
    ┌──────▼──────┐      ┌────────▼────────┐    ┌────────▼────────┐    
    │Auth Service │      │ Book Service    │    │Member Service   │
    │Port: 5001   │      │ Port: 5002      │    │Port: 5003       │
    └──────┬──────┘      └─────────┬───────┘    └─────────┬───────┘
           │                       │                      │
    ┌──────▼──────┐      ┌─────────▼───────┐    ┌─────────▼───────┐
    │Auth Database│      │ Book Database   │    │Member Database  │
    │PostgreSQL   │      │ PostgreSQL      │    │PostgreSQL       │
    │Port: 5432   │      │ Port: 5433      │    │Port: 5434       │
    └─────────────┘      └─────────────────┘    └─────────────────┘
                                  │
                         ┌────────▼────────┐
                         │ Redis Cache     │
                         │ Port: 6379      │
                         └─────────────────┘
```

## 🌟 Features

### 🎨 **Modern React Frontend**
- **React 19.1.0** with TypeScript and modern hooks
- **Material-UI v7** component library with custom theming
- **React Query v5** for advanced data fetching and caching
- **Interactive dashboards** with Recharts visualizations
- **Global search** with autocomplete functionality  
- **Responsive design** optimized for mobile and desktop
- **Code splitting** and lazy loading for optimal performance
- **Error boundaries** with comprehensive error handling

### 🔐 **Authentication Service (.NET 8)**
- JWT token-based authentication with refresh tokens
- Role-based access control (Admin, Member)
- BCrypt password hashing for security
- Token validation and refresh mechanisms
- Cross-service authentication middleware

### 📖 **Book Management Service (.NET 8)**
- Complete CRUD operations for books
- Book borrowing and return workflows
- ISBN validation and cataloging
- Borrowing history and audit trails
- Real-time availability tracking

### 👥 **Member Management Service (.NET 8)**
- Member registration and profile management
- Member status tracking (Active, Suspended, Inactive)
- Borrowing history aggregation
- Cross-service data synchronization

### 🌐 **API Gateway (Ocelot)**
- Single entry point with intelligent routing
- Request routing and load balancing
- Rate limiting and throttling (configurable per endpoint)
- CORS configuration and security policies
- Centralized authentication and authorization

## 🛠️ Technology Stack

### Backend Technologies
| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Framework** | .NET | 8.0 | Core application runtime |
| **Language** | C# | 12.0 | Primary programming language |
| **API Gateway** | Ocelot | 23.3.3 | Service orchestration and routing |
| **Database** | PostgreSQL | 15-alpine | Primary data storage |
| **Cache** | Redis | 7-alpine | In-memory caching |
| **ORM** | Entity Framework Core | 9.0.7 | Data access layer |
| **Authentication** | JWT Bearer | 8.0.12 | Token-based security |
| **Logging** | Serilog | 9.0.0 | Structured logging |
| **Testing** | xUnit + Moq | Latest | Unit and integration testing |
| **Documentation** | Swagger/OpenAPI | 6.4.0 | API documentation |
| **Resilience** | Polly | 23.3.3 | Circuit breaker and retry patterns |

### Backend Dependencies (.NET Packages)
```xml
<!-- Authentication Service -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.12" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.2" />

<!-- API Gateway -->
<PackageReference Include="Ocelot" Version="23.3.3" />
<PackageReference Include="Ocelot.Provider.Polly" Version="23.3.3" />
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />

<!-- Common Packages -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.7" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="8.0.12" />
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
```

### Frontend Technologies
| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Framework** | React | 19.1.0 | UI framework |
| **Language** | TypeScript | 4.9.5 | Type-safe JavaScript |
| **UI Library** | Material-UI | 7.2.0 | Component library |
| **Data Fetching** | React Query | 5.83.0 | Server state management |
| **Routing** | React Router | 7.7.0 | Client-side routing |
| **Charts** | Recharts | 3.1.0 | Data visualization |
| **Forms** | React Hook Form | 7.60.0 | Form handling |
| **HTTP Client** | Axios | 1.11.0 | API communication |
| **Date Utils** | date-fns | 4.1.0 | Date manipulation |
| **Validation** | Zod | 4.0.5 | Schema validation |

### Frontend Dependencies (NPM Packages)
```json
{
  "dependencies": {
    "@emotion/react": "^11.14.0",
    "@emotion/styled": "^11.14.1",
    "@hookform/resolvers": "^5.1.1",
    "@mui/icons-material": "^7.2.0",
    "@mui/material": "^7.2.0",
    "@mui/x-data-grid": "^8.9.1",
    "@tanstack/react-query": "^5.83.0",
    "axios": "^1.11.0",
    "date-fns": "^4.1.0",
    "react": "^19.1.0",
    "react-dom": "^19.1.0",
    "react-hook-form": "^7.60.0",
    "react-router-dom": "^7.7.0",
    "recharts": "^3.1.0",
    "typescript": "^4.9.5",
    "zod": "^4.0.5"
  },
  "devDependencies": {
    "@playwright/test": "^1.54.1",
    "@tanstack/react-query-devtools": "^5.83.0",
    "@testing-library/jest-dom": "^6.6.3",
    "@testing-library/react": "^16.3.0",
    "@testing-library/user-event": "^14.6.1",
    "cypress": "^14.5.2",
    "jest-environment-jsdom": "^30.0.5"
  }
}
```

### Infrastructure & DevOps
| Component | Technology | Purpose |
|-----------|------------|---------|
| **Containerization** | Docker & Docker Compose | Local development & packaging |
| **Orchestration** | Kubernetes (AKS) | Container orchestration |
| **Cloud Platform** | Microsoft Azure | Cloud hosting |
| **CI/CD** | GitHub Actions | Automated build & deployment |
| **Monitoring** | Azure Monitor | Application monitoring |
| **Logging** | Azure Log Analytics | Centralized logging |

## 🚀 Quick Start

### Prerequisites

- **Windows 10/11** (for local development)
- **Docker Desktop** for Windows
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download](https://nodejs.org/)
- **Git** for Windows
- **Visual Studio 2022** or **VS Code**

### 🏃‍♂️ Running Locally

1. **Clone the repository**
   ```bash
   git clone https://github.com/navinprabhu/claude_libraryApp.git
   cd claude_libraryApp
   ```

2. **Setup local development environment**
   ```powershell
   .\scripts\setup-local-dev.ps1
   ```

3. **Start backend services**
   ```powershell
   .\scripts\start-dev.ps1 -Detached -Build
   ```

4. **Start frontend development server**
   ```bash
   cd library-frontend
   npm install
   npm start
   ```

5. **Access the application**
   - **Frontend Web App**: http://localhost:3000
   - **API Gateway**: http://localhost:5000
   - **Swagger UI**: http://localhost:5000/swagger
   - **Individual Services**: 
     - Auth: http://localhost:5001
     - Books: http://localhost:5002  
     - Members: http://localhost:5003

### 🔑 Default Login Credentials
- **Username**: `admin`
- **Password**: `password`

### 🐳 Docker Development

```powershell
# Start all services with Docker Compose
docker-compose up -d

# View aggregated logs
.\scripts\logs.ps1

# Stop all services
.\scripts\stop-all.ps1

# Clean restart
docker-compose down -v && .\scripts\build-all.ps1 -Clean && .\scripts\start-dev.ps1 -Detached
```

## 📁 Project Structure

```
claude_libraryApp/
├── 📁 LibraryApp.AuthService/          # Authentication microservice
├── 📁 LibraryApp.BookService/          # Book management microservice  
├── 📁 LibraryApp.MemberService/        # Member management microservice
├── 📁 LibraryApp.ApiGateway/           # API Gateway with Ocelot
├── 📁 LibraryApp.Shared.Models/        # Common DTOs and entities
├── 📁 LibraryApp.Shared.Infrastructure/ # Common utilities & middleware
├── 📁 LibraryApp.Shared.Events/        # Event models for messaging
├── 📁 LibraryApp.Tests/                # Unit & integration tests
├── 📁 library-frontend/                # React TypeScript frontend
│   ├── 📁 src/
│   │   ├── 📁 components/              # React components
│   │   ├── 📁 pages/                   # Page components
│   │   ├── 📁 contexts/                # React contexts (Auth, Notifications)
│   │   ├── 📁 hooks/                   # Custom React hooks
│   │   ├── 📁 services/                # API services
│   │   ├── 📁 types/                   # TypeScript type definitions
│   │   └── 📁 utils/                   # Utility functions
│   ├── 📄 package.json                 # Frontend dependencies
│   └── 📄 tsconfig.json                # TypeScript configuration
├── 📁 scripts/                         # PowerShell automation scripts
│   ├── ⚡ setup-local-dev.ps1         # Environment setup
│   ├── 🔧 start-dev.ps1               # Start all services
│   ├── 📊 logs.ps1                     # View logs
│   ├── 🧹 stop-all.ps1                # Stop services
│   └── 🗑️ clean.ps1                   # Cleanup containers
├── 📁 .github/workflows/               # GitHub Actions CI/CD
├── 📄 docker-compose.yml               # Multi-service orchestration
├── 📄 docker-compose.override.yml      # Development overrides
├── 📄 LibraryApp.sln                   # .NET solution file
├── 📄 CLAUDE.md                        # Development context & instructions
└── 📄 README.md                        # This file
```

## 🔧 API Endpoints

### 🎨 Frontend Routes
```
/                    # Dashboard (protected)
/login               # Login page
/books               # Books management (protected)
/members             # Members management (protected)
/transactions        # Transaction history (protected)
/reports             # Reports and analytics (protected)
/settings            # Application settings (protected)
```

### 🔐 Authentication Service (Port 5001)
```http
POST   /api/auth/login          # User login
POST   /api/auth/validate       # Token validation
POST   /api/auth/refresh        # Refresh token
GET    /api/auth/userinfo       # Get user information
GET    /health                  # Health check endpoint
```

### 📚 Book Service (Port 5002)
```http
GET    /api/books               # Get all books
GET    /api/books/{id}          # Get book by ID
POST   /api/books               # Create new book [Admin]
PUT    /api/books/{id}          # Update book [Admin]
DELETE /api/books/{id}          # Delete book [Admin]
POST   /api/books/{id}/borrow   # Borrow book
POST   /api/books/{id}/return   # Return book
GET    /api/books/{id}/history  # Get borrowing history
GET    /health                  # Health check endpoint
```

### 👥 Member Service (Port 5003)
```http
GET    /api/members             # Get all members [Admin]
GET    /api/members/{id}        # Get member by ID
POST   /api/members             # Register new member
PUT    /api/members/{id}        # Update member profile
GET    /api/members/{id}/borrowed-books    # Get borrowed books
GET    /api/members/{id}/history           # Get borrowing history
GET    /health                             # Health check endpoint
```

### 🌐 API Gateway (Port 5000)
```http
# Routes all above endpoints through gateway
GET    /health/gateway          # Gateway health
GET    /health/services         # Aggregated service health
GET    /swagger                 # API documentation
```

## 🧪 Testing

### Backend Testing
```powershell
# Run all .NET tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific service tests
dotnet test LibraryApp.Tests/
```

### Frontend Testing
```bash
cd library-frontend

# Run Jest unit tests
npm test

# Run Cypress e2e tests
npm run cypress:open

# Run Playwright tests
npm run test:e2e
```

### Integration Testing
```powershell
# Run integration tests with TestContainers
dotnet test LibraryApp.Tests/ --filter Category=Integration
```

## 📊 Monitoring & Observability

### Health Checks
- **Individual Services**: `http://localhost:{port}/health`
- **Gateway Health**: `http://localhost:5000/health/gateway`
- **Aggregated Health**: `http://localhost:5000/health/services`

### Structured Logging
- **Console Output**: Development environment with Serilog
- **File Logging**: `logs/` directory per service
- **Correlation IDs**: Request tracing across all services
- **Log Levels**: Debug, Information, Warning, Error, Critical

### Metrics & Monitoring
- **Application Performance**: Built-in ASP.NET Core metrics
- **Custom Business Metrics**: Book borrowing rates, user activity
- **Health Check Dashboard**: Real-time system status

## 🔒 Security Features

- **JWT Authentication**: Bearer token-based security with refresh tokens
- **Role-Based Access Control**: Admin and Member roles with permissions
- **Input Validation**: Comprehensive data validation and sanitization
- **HTTPS Enforcement**: TLS encryption for all communications
- **CORS Configuration**: Secure cross-origin request handling
- **Rate Limiting**: API throttling and abuse prevention
- **Password Security**: BCrypt hashing with salt
- **XSS Protection**: Content Security Policy headers
- **SQL Injection Prevention**: Parameterized queries via EF Core

## 🌐 API Documentation

Interactive API documentation available via Swagger UI:
- **Gateway Swagger**: http://localhost:5000/swagger
- **Auth Service**: http://localhost:5001/swagger
- **Book Service**: http://localhost:5002/swagger
- **Member Service**: http://localhost:5003/swagger

## 📋 API Usage Examples

### Authentication
```bash
# Login to get JWT token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Response
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiration": "2025-07-24T04:52:41.0899947Z",
    "username": "admin",
    "role": "Admin",
    "permissions": ["books:read", "books:write", "books:delete", ...]
  },
  "statusCode": 200
}
```

### Create a Book (Admin Only)
```bash
curl -X POST http://localhost:5000/api/books \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "The Great Gatsby",
    "author": "F. Scott Fitzgerald",
    "isbn": "9780743273565",
    "genre": "Fiction"
  }'
```

### Borrow a Book
```bash
curl -X POST http://localhost:5000/api/books/1/borrow \
  -H "Authorization: Bearer {your-jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{"memberId": 1}'
```

---

## ☁️ Azure Cloud Deployment Plan

### 🎯 Azure Architecture Overview

```
Internet
    ↓
Azure Front Door (CDN + WAF)
    ↓
Azure Application Gateway (Load Balancer)
    ↓
Azure Kubernetes Service (AKS)
    ├── Frontend Pods (React App)
    ├── API Gateway Pods (Ocelot)
    ├── Auth Service Pods
    ├── Book Service Pods
    └── Member Service Pods
    ↓
Azure Database for PostgreSQL (Flexible Server)
Azure Cache for Redis
Azure Key Vault (Secrets)
Azure Monitor + Log Analytics
```

### 🏗️ Azure Services Required

| Service | SKU/Tier | Purpose | Estimated Monthly Cost (USD) |
|---------|----------|---------|------------------------------|
| **Azure Kubernetes Service (AKS)** | Standard | Container orchestration | $150-300 |
| **Azure Database for PostgreSQL** | General Purpose, 2 vCores | Primary database | $120-200 |
| **Azure Cache for Redis** | Basic, C1 | In-memory cache | $20-40 |
| **Azure Container Registry (ACR)** | Basic | Docker image storage | $5-15 |
| **Azure Key Vault** | Standard | Secrets management | $2-5 |
| **Azure Application Gateway** | Standard v2 | Load balancer + WAF | $30-50 |
| **Azure Front Door** | Standard | CDN + Global load balancer | $25-45 |
| **Azure Monitor** | Pay-as-you-go | Monitoring + alerting | $20-40 |
| **Azure Log Analytics** | Pay-as-you-go | Centralized logging | $15-30 |
| **Azure Storage Account** | Standard LRS | Static files + backups | $5-10 |
| **Total Estimated Cost** | | | **$392-735/month** |

### 📋 Phase 1: Infrastructure Setup

#### 1.1 Resource Group & Networking
```bash
# Create resource group
az group create --name rg-libraryapp-prod --location eastus2

# Create virtual network
az network vnet create \
  --resource-group rg-libraryapp-prod \
  --name vnet-libraryapp \
  --address-prefix 10.0.0.0/16 \
  --subnet-name subnet-aks \
  --subnet-prefix 10.0.1.0/24
```

#### 1.2 Azure Container Registry
```bash
# Create ACR
az acr create \
  --resource-group rg-libraryapp-prod \
  --name acrlibraryapp \
  --sku Basic \
  --admin-enabled true
```

#### 1.3 Azure Database for PostgreSQL
```bash
# Create PostgreSQL flexible server
az postgres flexible-server create \
  --resource-group rg-libraryapp-prod \
  --name psql-libraryapp-prod \
  --location eastus2 \
  --admin-user libraryadmin \
  --admin-password <secure-password> \
  --sku-name Standard_B2s \
  --tier Burstable \
  --storage-size 32 \
  --version 15

# Create databases
az postgres flexible-server db create \
  --resource-group rg-libraryapp-prod \
  --server-name psql-libraryapp-prod \
  --database-name AuthDatabase

az postgres flexible-server db create \
  --resource-group rg-libraryapp-prod \
  --server-name psql-libraryapp-prod \
  --database-name BookDatabase

az postgres flexible-server db create \
  --resource-group rg-libraryapp-prod \
  --server-name psql-libraryapp-prod \
  --database-name MemberDatabase
```

#### 1.4 Azure Cache for Redis
```bash
# Create Redis cache
az redis create \
  --resource-group rg-libraryapp-prod \
  --name redis-libraryapp-prod \
  --location eastus2 \
  --sku Basic \
  --vm-size c1
```

#### 1.5 Azure Key Vault
```bash
# Create Key Vault
az keyvault create \
  --name kv-libraryapp-prod \
  --resource-group rg-libraryapp-prod \
  --location eastus2 \
  --sku standard
```

### 📋 Phase 2: Kubernetes Setup

#### 2.1 Create AKS Cluster
```bash
# Create AKS cluster
az aks create \
  --resource-group rg-libraryapp-prod \
  --name aks-libraryapp-prod \
  --node-count 3 \
  --node-vm-size Standard_D2s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys \
  --attach-acr acrlibraryapp \
  --enable-managed-identity
```

#### 2.2 Configure kubectl
```bash
# Get AKS credentials
az aks get-credentials \
  --resource-group rg-libraryapp-prod \
  --name aks-libraryapp-prod
```

### 📋 Phase 3: Configuration Management

#### 3.1 Environment-Specific Configuration Files

**Create `appsettings.Production.json` for each service:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host={POSTGRES_HOST};Port=5432;Database={DATABASE_NAME};Username={POSTGRES_USER};Password={POSTGRES_PASSWORD};SSL Mode=Require;"
  },
  "JwtSettings": {
    "SecretKey": "{JWT_SECRET_FROM_KEYVAULT}",
    "Issuer": "LibraryApp.AuthService",
    "Audience": "LibraryApp.ApiClients",
    "ExpiryMinutes": 60
  },
  "Redis": {
    "ConnectionString": "{REDIS_CONNECTION_FROM_KEYVAULT}"
  },
  "ServiceUrls": {
    "AuthService": "http://auth-service:5001",
    "BookService": "http://book-service:5002", 
    "MemberService": "http://member-service:5003"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApplicationInsights": {
    "ConnectionString": "{APPINSIGHTS_CONNECTION_FROM_KEYVAULT}"
  }
}
```

#### 3.2 Kubernetes ConfigMaps and Secrets

**ConfigMap for non-sensitive settings:**
```yaml
# k8s/configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: libraryapp-config
  namespace: libraryapp
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  JWT_ISSUER: "LibraryApp.AuthService"
  JWT_AUDIENCE: "LibraryApp.ApiClients"
  JWT_EXPIRY_MINUTES: "60"
  SERVICE_URLS__AUTHSERVICE: "http://auth-service:5001"
  SERVICE_URLS__BOOKSERVICE: "http://book-service:5002"
  SERVICE_URLS__MEMBERSERVICE: "http://member-service:5003"
```

**Secret for sensitive data (populated from Key Vault):**
```yaml
# k8s/secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: libraryapp-secrets
  namespace: libraryapp
type: Opaque
data:
  # Base64 encoded values (will be populated by Azure Key Vault)
  JWT_SECRET_KEY: ""
  POSTGRES_CONNECTION_STRING: ""
  REDIS_CONNECTION_STRING: ""
  APPLICATION_INSIGHTS_CONNECTION_STRING: ""
```

#### 3.3 Dockerfile Updates for Production

**Update each service Dockerfile for multi-stage builds:**
```dockerfile
# Example: LibraryApp.AuthService/Dockerfile.Production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

# Copy project files
COPY LibraryApp.AuthService/LibraryApp.AuthService.csproj LibraryApp.AuthService/
COPY LibraryApp.Shared.Models/LibraryApp.Shared.Models.csproj LibraryApp.Shared.Models/
COPY LibraryApp.Shared.Infrastructure/LibraryApp.Shared.Infrastructure.csproj LibraryApp.Shared.Infrastructure/

# Restore dependencies
RUN dotnet restore LibraryApp.AuthService/LibraryApp.AuthService.csproj

# Copy source code
COPY . .

# Build application
RUN dotnet publish LibraryApp.AuthService/LibraryApp.AuthService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build-env /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "LibraryApp.AuthService.dll"]
```

### 📋 Phase 4: Kubernetes Deployments

#### 4.1 Namespace and RBAC
```yaml
# k8s/namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: libraryapp
---
# k8s/rbac.yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: libraryapp-service-account
  namespace: libraryapp
```

#### 4.2 Database Migration Job
```yaml
# k8s/migration-job.yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: database-migration
  namespace: libraryapp
spec:
  template:
    spec:
      restartPolicy: OnFailure
      containers:
      - name: migration
        image: acrlibraryapp.azurecr.io/libraryapp-migration:latest
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        envFrom:
        - configMapRef:
            name: libraryapp-config
        - secretRef:
            name: libraryapp-secrets
```

#### 4.3 Service Deployments
```yaml
# k8s/auth-service.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: auth-service
  namespace: libraryapp
spec:
  replicas: 2
  selector:
    matchLabels:
      app: auth-service
  template:
    metadata:
      labels:
        app: auth-service
    spec:
      serviceAccountName: libraryapp-service-account
      containers:
      - name: auth-service
        image: acrlibraryapp.azurecr.io/libraryapp-auth:latest
        ports:
        - containerPort: 5001
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ASPNETCORE_URLS
          value: "http://+:5001"
        envFrom:
        - configMapRef:
            name: libraryapp-config
        - secretRef:
            name: libraryapp-secrets
        livenessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 30
        readinessProbe:
          httpGet:
            path: /health
            port: 5001
          initialDelaySeconds: 5
          periodSeconds: 10
        resources:
          requests:
            memory: "128Mi"
            cpu: "50m"
          limits:
            memory: "256Mi"
            cpu: "200m"
---
apiVersion: v1
kind: Service
metadata:
  name: auth-service
  namespace: libraryapp
spec:
  selector:
    app: auth-service
  ports:
  - port: 5001
    targetPort: 5001
  type: ClusterIP
```

#### 4.4 Frontend Deployment
```yaml
# k8s/frontend.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: frontend
  namespace: libraryapp
spec:
  replicas: 2
  selector:
    matchLabels:
      app: frontend
  template:
    metadata:
      labels:
        app: frontend
    spec:
      containers:
      - name: frontend
        image: acrlibraryapp.azurecr.io/libraryapp-frontend:latest
        ports:
        - containerPort: 80
        env:
        - name: REACT_APP_API_URL
          value: "https://api.libraryapp.com"
        resources:
          requests:
            memory: "64Mi"
            cpu: "25m"
          limits:
            memory: "128Mi"
            cpu: "100m"
---
apiVersion: v1
kind: Service
metadata:
  name: frontend
  namespace: libraryapp
spec:
  selector:
    app: frontend
  ports:
  - port: 80
    targetPort: 80
  type: ClusterIP
```

### 📋 Phase 5: Load Balancer & Ingress

#### 5.1 Application Gateway Configuration
```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: libraryapp-ingress
  namespace: libraryapp
  annotations:
    kubernetes.io/ingress.class: azure/application-gateway
    appgw.ingress.kubernetes.io/ssl-redirect: "true"
    appgw.ingress.kubernetes.io/cookie-based-affinity: "true"
spec:
  tls:
  - secretName: libraryapp-tls
  rules:
  - host: libraryapp.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend
            port:
              number: 80
  - host: api.libraryapp.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: api-gateway
            port:
              number: 5000
```

### 📋 Phase 6: CI/CD Pipeline

#### 6.1 GitHub Actions Workflow
```yaml
# .github/workflows/azure-deploy.yml
name: Deploy to Azure AKS

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  AZURE_RESOURCE_GROUP: rg-libraryapp-prod
  AKS_CLUSTER_NAME: aks-libraryapp-prod
  ACR_NAME: acrlibraryapp

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Build and push Docker images
      run: |
        az acr build --registry $ACR_NAME --image libraryapp-auth:${{ github.sha }} ./LibraryApp.AuthService
        az acr build --registry $ACR_NAME --image libraryapp-book:${{ github.sha }} ./LibraryApp.BookService
        az acr build --registry $ACR_NAME --image libraryapp-member:${{ github.sha }} ./LibraryApp.MemberService
        az acr build --registry $ACR_NAME --image libraryapp-gateway:${{ github.sha }} ./LibraryApp.ApiGateway
        az acr build --registry $ACR_NAME --image libraryapp-frontend:${{ github.sha }} ./library-frontend
    
    - name: Get AKS credentials
      run: |
        az aks get-credentials --resource-group $AZURE_RESOURCE_GROUP --name $AKS_CLUSTER_NAME
    
    - name: Deploy to AKS
      run: |
        kubectl apply -f k8s/
        kubectl set image deployment/auth-service auth-service=$ACR_NAME.azurecr.io/libraryapp-auth:${{ github.sha }} -n libraryapp
        kubectl set image deployment/book-service book-service=$ACR_NAME.azurecr.io/libraryapp-book:${{ github.sha }} -n libraryapp
        kubectl set image deployment/member-service member-service=$ACR_NAME.azurecr.io/libraryapp-member:${{ github.sha }} -n libraryapp
        kubectl set image deployment/api-gateway api-gateway=$ACR_NAME.azurecr.io/libraryapp-gateway:${{ github.sha }} -n libraryapp
        kubectl set image deployment/frontend frontend=$ACR_NAME.azurecr.io/libraryapp-frontend:${{ github.sha }} -n libraryapp
    
    - name: Verify deployment
      run: |
        kubectl rollout status deployment/auth-service -n libraryapp
        kubectl rollout status deployment/book-service -n libraryapp
        kubectl rollout status deployment/member-service -n libraryapp
        kubectl rollout status deployment/api-gateway -n libraryapp
        kubectl rollout status deployment/frontend -n libraryapp
```

### 📋 Phase 7: Monitoring & Observability

#### 7.1 Azure Monitor Setup
```bash
# Enable Container Insights
az aks enable-addons \
  --resource-group rg-libraryapp-prod \
  --name aks-libraryapp-prod \
  --addons monitoring
```

#### 7.2 Application Insights Integration
```yaml
# k8s/monitoring.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: application-insights-config
  namespace: libraryapp
data:
  ApplicationInsights__ConnectionString: "{CONNECTION_STRING_FROM_KEYVAULT}"
  APPLICATIONINSIGHTS_CONNECTION_STRING: "{CONNECTION_STRING_FROM_KEYVAULT}"
```

### 📋 Phase 8: Security & Compliance

#### 8.1 Azure Key Vault Integration
```bash
# Install Key Vault provider
helm repo add csi-secrets-store-provider-azure https://azure.github.io/secrets-store-csi-driver-provider-azure/charts
helm install csi csi-secrets-store-provider-azure/csi-secrets-store-provider-azure \
  --namespace kube-system
```

#### 8.2 Network Security
```yaml
# k8s/network-policy.yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: libraryapp-network-policy
  namespace: libraryapp
spec:
  podSelector: {}
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
  egress:
  - to: []
    ports:
    - protocol: TCP
      port: 443
    - protocol: TCP
      port: 53
    - protocol: UDP
      port: 53
```

### 🚀 Deployment Timeline

| Phase | Duration | Key Activities |
|-------|----------|----------------|
| **Phase 1** | 1 week | Infrastructure setup, Azure services provisioning |
| **Phase 2** | 1 week | AKS cluster setup, kubectl configuration |
| **Phase 3** | 2 weeks | Configuration management, environment-specific settings |
| **Phase 4** | 2 weeks | Kubernetes deployments, service configurations |
| **Phase 5** | 1 week | Load balancer setup, ingress configuration |
| **Phase 6** | 1 week | CI/CD pipeline setup, automated deployments |
| **Phase 7** | 1 week | Monitoring, logging, alerting setup |
| **Phase 8** | 1 week | Security hardening, compliance validation |
| **Total** | **10 weeks** | **Complete Azure deployment** |

### 📊 Post-Deployment Checklist

- [ ] All services healthy and responding
- [ ] Database connections working
- [ ] Redis cache operational  
- [ ] Frontend accessible via public URL
- [ ] API Gateway routing correctly
- [ ] Authentication flow working
- [ ] SSL certificates configured
- [ ] Monitoring alerts configured
- [ ] Backup strategy implemented
- [ ] Security scanning completed
- [ ] Performance testing passed
- [ ] Documentation updated

---

## 🔄 CI/CD Pipeline

### GitHub Actions Workflows

- **Continuous Integration** (`.github/workflows/ci.yml`)
  - Build all microservices and frontend
  - Run unit and integration tests
  - Build and test Docker containers
  - Code coverage reporting
  - Security scanning

- **Continuous Deployment** (`.github/workflows/azure-deploy.yml`)
  - Build production Docker images
  - Push to Azure Container Registry
  - Deploy to AKS staging environment
  - Run smoke tests and health checks
  - Deploy to production (manual approval)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes and add tests
4. Commit your changes (`git commit -m 'Add some amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

### Code Standards
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use [React TypeScript Best Practices](https://react-typescript-cheatsheet.netlify.app/)
- Write unit tests for new functionality
- Update documentation as needed
- Ensure all CI checks pass

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Navin Prabhu**
- GitHub: [@navinprabhu](https://github.com/navinprabhu)
- Project: [claude_libraryApp](https://github.com/navinprabhu/claude_libraryApp)

## 🙏 Acknowledgments

- Built with guidance from Claude AI
- Inspired by modern microservices and cloud-native patterns
- Following .NET, React, and Azure best practices
- Community contributions and feedback

## 📞 Support

For support and questions:
- 🐛 **Bug Reports**: [Create an Issue](https://github.com/navinprabhu/claude_libraryApp/issues)
- 💡 **Feature Requests**: [Discussion Board](https://github.com/navinprabhu/claude_libraryApp/discussions)
- 📧 **Contact**: Open an issue for general questions

---

⭐ **Star this repository** if you find it helpful!

📚 **Happy coding and happy reading!** 📚