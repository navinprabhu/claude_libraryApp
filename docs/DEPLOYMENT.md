# LibraryApp Deployment Guide

This guide covers deployment strategies and procedures for the LibraryApp microservices system.

## Table of Contents
- [Architecture Overview](#architecture-overview)
- [Environment Configuration](#environment-configuration)
- [Local Development](#local-development)
- [Staging Deployment](#staging-deployment)
- [Production Deployment](#production-deployment)
- [Monitoring and Alerting](#monitoring-and-alerting)
- [Troubleshooting](#troubleshooting)

## Architecture Overview

### Microservices Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Client Apps   │    │   Load Balancer │    │   API Gateway   │
│                 │───▶│                 │───▶│                 │
│ Web/Mobile/API  │    │                 │    │   (Port 5000)   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        │
                                                        ▼
                       ┌────────────────────────────────────────────┐
                       │              Service Mesh                  │
                       └────────────────────────────────────────────┘
                                                        │
                    ┌──────────────┬──────────────┬──────────────┐
                    ▼              ▼              ▼              ▼
            ┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
            │ Auth Service │ │ Book Service │ │Member Service│ │   Database   │
            │ (Port 5001)  │ │ (Port 5002)  │ │ (Port 5003)  │ │   Cluster    │
            └──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

### Technology Stack

- **Runtime**: .NET 8.0
- **Containerization**: Docker
- **Orchestration**: Kubernetes
- **API Gateway**: Ocelot
- **Authentication**: JWT
- **Monitoring**: Application Insights, Prometheus
- **Logging**: Serilog, ELK Stack
- **CI/CD**: GitHub Actions

## Environment Configuration

### Environment Types

1. **Development** - Local developer machines
2. **Staging** - Cloud-based testing environment
3. **Production** - Live production environment

### Configuration Management

#### Environment Variables

Each service uses the following configuration pattern:

```bash
# Database Configuration
ConnectionStrings__DefaultConnection="Server=localhost;Database=LibraryApp_Auth;Trusted_Connection=true;"

# JWT Configuration
JwtSettings__SecretKey="your-super-secret-key-here"
JwtSettings__Issuer="LibraryApp"
JwtSettings__Audience="LibraryApp-Users"
JwtSettings__ExpirationMinutes="60"

# Service URLs
ExternalServices__AuthService__BaseUrl="https://auth.libraryapp.com"
ExternalServices__BookService__BaseUrl="https://books.libraryapp.com"
ExternalServices__MemberService__BaseUrl="https://members.libraryapp.com"

# Logging
Logging__LogLevel__Default="Information"
Logging__LogLevel__Microsoft="Warning"

# Application Insights
ApplicationInsights__ConnectionString="InstrumentationKey=your-key"

# CORS
Cors__AllowedOrigins__0="https://app.libraryapp.com"
Cors__AllowedOrigins__1="https://admin.libraryapp.com"
```

#### Configuration per Environment

**appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ExternalServices": {
    "AuthService": {
      "BaseUrl": "http://localhost:5001"
    },
    "BookService": {
      "BaseUrl": "http://localhost:5002"
    },
    "MemberService": {
      "BaseUrl": "http://localhost:5003"
    }
  }
}
```

**appsettings.Staging.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ExternalServices": {
    "AuthService": {
      "BaseUrl": "https://auth-staging.libraryapp.com"
    },
    "BookService": {
      "BaseUrl": "https://books-staging.libraryapp.com"
    },
    "MemberService": {
      "BaseUrl": "https://members-staging.libraryapp.com"
    }
  }
}
```

**appsettings.Production.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "LibraryApp": "Information"
    }
  },
  "ExternalServices": {
    "AuthService": {
      "BaseUrl": "https://auth.libraryapp.com"
    },
    "BookService": {
      "BaseUrl": "https://books.libraryapp.com"
    },
    "MemberService": {
      "BaseUrl": "https://members.libraryapp.com"
    }
  }
}
```

## Local Development

### Prerequisites

- Docker Desktop
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Quick Start

1. **Clone Repository**
   ```bash
   git clone https://github.com/your-org/libraryapp.git
   cd libraryapp
   ```

2. **Start Services**
   ```bash
   # Using Docker Compose
   docker-compose up -d

   # Or using PowerShell script
   .\scripts\start-dev.ps1
   ```

3. **Verify Services**
   ```bash
   # Check service health
   curl http://localhost:5000/health  # API Gateway
   curl http://localhost:5001/health  # Auth Service
   curl http://localhost:5002/health  # Book Service
   curl http://localhost:5003/health  # Member Service
   ```

4. **Access Swagger Documentation**
   - API Gateway: http://localhost:5000/swagger
   - Auth Service: http://localhost:5001/swagger
   - Book Service: http://localhost:5002/swagger
   - Member Service: http://localhost:5003/swagger

### Development Workflow

1. **Make Code Changes**
2. **Run Tests**
   ```bash
   dotnet test
   ```
3. **Build and Test Locally**
   ```bash
   docker-compose up --build
   ```
4. **Submit Pull Request**

## Staging Deployment

### Infrastructure

- **Platform**: Azure Kubernetes Service (AKS)
- **Load Balancer**: Azure Load Balancer
- **Database**: Azure SQL Database
- **Storage**: Azure Blob Storage
- **Monitoring**: Azure Application Insights

### Deployment Process

#### Automated Deployment via GitHub Actions

Staging deployment is triggered automatically on:
- Merge to `develop` branch
- Manual workflow dispatch

```yaml
# Deployment is handled by .github/workflows/cd.yml
# Services are deployed to staging namespace in AKS
```

#### Manual Deployment

1. **Build Images**
   ```bash
   # Build all service images
   docker build -t libraryapp-auth:staging -f LibraryApp.AuthService/Dockerfile .
   docker build -t libraryapp-book:staging -f LibraryApp.BookService/Dockerfile .
   docker build -t libraryapp-member:staging -f LibraryApp.MemberService/Dockerfile .
   docker build -t libraryapp-gateway:staging -f LibraryApp.ApiGateway/Dockerfile .
   ```

2. **Push to Registry**
   ```bash
   docker tag libraryapp-auth:staging your-registry.azurecr.io/libraryapp-auth:staging
   docker push your-registry.azurecr.io/libraryapp-auth:staging
   # Repeat for other services
   ```

3. **Deploy to Kubernetes**
   ```bash
   kubectl apply -f k8s/staging/ --namespace=staging
   ```

#### Kubernetes Manifests

**Namespace**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: staging
```

**Auth Service Deployment**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: libraryapp-auth
  namespace: staging
spec:
  replicas: 2
  selector:
    matchLabels:
      app: libraryapp-auth
  template:
    metadata:
      labels:
        app: libraryapp-auth
    spec:
      containers:
      - name: libraryapp-auth
        image: your-registry.azurecr.io/libraryapp-auth:staging
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Staging"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection
              key: auth-connection
        resources:
          requests:
            cpu: 100m
            memory: 128Mi
          limits:
            cpu: 500m
            memory: 512Mi
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### Environment Verification

After deployment, verify the staging environment:

```bash
# Check pod status
kubectl get pods -n staging

# Check service health
curl https://staging.libraryapp.com/health

# Run smoke tests
curl -X POST https://staging-auth.libraryapp.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "test@example.com", "password": "TestPass123!"}'
```

## Production Deployment

### Infrastructure

- **Platform**: Azure Kubernetes Service (AKS) with multiple node pools
- **Load Balancer**: Azure Application Gateway with WAF
- **Database**: Azure SQL Database with failover group
- **CDN**: Azure CDN for static assets
- **Monitoring**: Azure Monitor + Application Insights
- **Security**: Azure Key Vault for secrets

### Deployment Strategy

#### Blue-Green Deployment

Production uses blue-green deployment for zero-downtime updates:

1. **Blue Environment** - Current production
2. **Green Environment** - New version deployment
3. **Traffic Switch** - Gradual traffic migration
4. **Validation** - Health checks and monitoring
5. **Rollback** - Quick rollback if issues detected

#### Deployment Process

1. **Pre-deployment Checks**
   ```bash
   # Verify staging environment
   curl https://staging.libraryapp.com/health
   
   # Run integration tests
   dotnet test --filter Category=Integration
   
   # Security scan
   docker scan libraryapp-auth:latest
   ```

2. **Deploy to Green Environment**
   ```bash
   # Deploy new version to green slots
   kubectl apply -f k8s/production/green/ --namespace=production
   
   # Wait for deployment completion
   kubectl rollout status deployment/libraryapp-auth-green -n production
   ```

3. **Health Check and Validation**
   ```bash
   # Internal health checks
   kubectl exec -n production deployment/libraryapp-auth-green -- curl localhost:8080/health
   
   # Run smoke tests against green environment
   ./scripts/smoke-tests.sh green
   ```

4. **Traffic Migration**
   ```bash
   # Gradually shift traffic from blue to green
   # 10% -> 25% -> 50% -> 100%
   kubectl patch service libraryapp-auth -n production \
     -p '{"spec":{"selector":{"version":"green"}}}'
   ```

5. **Post-deployment Monitoring**
   - Application metrics monitoring
   - Error rate monitoring
   - Performance metrics monitoring
   - User experience monitoring

6. **Cleanup Blue Environment**
   ```bash
   # After successful deployment, cleanup old version
   kubectl delete deployment libraryapp-auth-blue -n production
   ```

### Production Configuration

#### Resource Limits

```yaml
resources:
  requests:
    cpu: 200m
    memory: 256Mi
  limits:
    cpu: 1000m
    memory: 1Gi
```

#### Horizontal Pod Autoscaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: libraryapp-auth-hpa
  namespace: production
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: libraryapp-auth
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

#### Network Policies

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: libraryapp-network-policy
  namespace: production
spec:
  podSelector:
    matchLabels:
      app: libraryapp
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: istio-system
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - namespaceSelector:
        matchLabels:
          name: production
    ports:
    - protocol: TCP
      port: 8080
  - to: []
    ports:
    - protocol: TCP
      port: 443
    - protocol: TCP
      port: 53
    - protocol: UDP
      port: 53
```

## Monitoring and Alerting

### Application Monitoring

#### Health Checks

Each service implements multiple health check endpoints:

```csharp
// Basic health check
app.MapHealthChecks("/health");

// Detailed health check with dependencies
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness probe for Kubernetes
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});
```

#### Metrics Collection

```csharp
// Custom metrics
services.AddSingleton<IMetrics, MetricsService>();

// Application Insights
services.AddApplicationInsightsTelemetry();

// Prometheus metrics
services.AddPrometheusMetrics();
```

### Alert Configuration

#### Critical Alerts

1. **Service Down**
   - Trigger: Health check failure > 2 minutes
   - Action: Page on-call engineer

2. **High Error Rate**
   - Trigger: Error rate > 5% for 5 minutes
   - Action: Slack notification + Email

3. **High Response Time**
   - Trigger: P95 > 2 seconds for 10 minutes
   - Action: Slack notification

4. **Database Connection Issues**
   - Trigger: DB connection failure
   - Action: Immediate escalation

#### Warning Alerts

1. **High CPU Usage**
   - Trigger: CPU > 80% for 15 minutes
   - Action: Slack notification

2. **High Memory Usage**
   - Trigger: Memory > 85% for 15 minutes
   - Action: Slack notification

3. **Disk Space Low**
   - Trigger: Disk usage > 90%
   - Action: Email notification

### Logging Strategy

#### Log Levels

- **Trace**: Detailed execution flow
- **Debug**: Debugging information (development only)
- **Information**: General application flow
- **Warning**: Unexpected situations that don't break functionality
- **Error**: Error conditions that require attention
- **Critical**: Critical failures that may cause application termination

#### Structured Logging

```csharp
_logger.LogInformation("User {UserId} borrowed book {BookId} at {Timestamp}",
    userId, bookId, DateTime.UtcNow);

_logger.LogError(exception, "Failed to process payment for order {OrderId}",
    orderId);
```

#### Log Aggregation

- **Development**: Console + File
- **Staging**: Azure Log Analytics
- **Production**: Azure Log Analytics + ELK Stack

## Troubleshooting

### Common Issues

#### Service Communication Failures

**Symptoms:**
- HTTP 503 Service Unavailable
- Timeout errors
- Connection refused errors

**Diagnosis:**
```bash
# Check service status
kubectl get pods -n production

# Check service logs
kubectl logs -f deployment/libraryapp-auth -n production

# Check network connectivity
kubectl exec -n production deployment/libraryapp-auth -- nslookup libraryapp-book
```

**Resolution:**
1. Verify service discovery configuration
2. Check network policies
3. Restart affected services
4. Scale up if resource constraints

#### Database Connection Issues

**Symptoms:**
- Database timeout errors
- Connection pool exhaustion
- Authentication failures

**Diagnosis:**
```bash
# Check connection string configuration
kubectl get secret db-connection -n production -o yaml

# Check database health
az sql db show-connection-string --name LibraryApp --server libraryapp-server

# Monitor connection pool
kubectl exec -n production deployment/libraryapp-auth -- netstat -an | grep 1433
```

**Resolution:**
1. Verify connection string and credentials
2. Check database server health
3. Adjust connection pool settings
4. Review database locks and performance

#### Performance Issues

**Symptoms:**
- High response times
- Increased CPU/memory usage
- Request timeouts

**Diagnosis:**
```bash
# Check resource usage
kubectl top pods -n production

# Review application metrics
curl https://libraryapp.com/metrics

# Analyze Application Insights data
az monitor log-analytics query --workspace-id $WORKSPACE_ID \
  --analytics-query "requests | where timestamp > ago(1h) | summarize avg(duration) by bin(timestamp, 5m)"
```

**Resolution:**
1. Scale up replicas if needed
2. Optimize database queries
3. Implement caching strategies
4. Review code for performance bottlenecks

### Emergency Procedures

#### Immediate Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/libraryapp-auth -n production

# Verify rollback
kubectl rollout status deployment/libraryapp-auth -n production

# Update traffic routing if using blue-green
kubectl patch service libraryapp-auth -n production \
  -p '{"spec":{"selector":{"version":"blue"}}}'
```

#### Service Isolation

```bash
# Remove service from load balancer
kubectl patch service libraryapp-auth -n production \
  -p '{"spec":{"selector":{"app":"none"}}}'

# Scale down problematic service
kubectl scale deployment libraryapp-auth --replicas=0 -n production

# Enable maintenance mode
kubectl apply -f k8s/maintenance-mode.yaml
```

### Support Contacts

- **On-call Engineer**: +1-XXX-XXX-XXXX
- **DevOps Team**: devops@libraryapp.com
- **Security Team**: security@libraryapp.com
- **Database Team**: dba@libraryapp.com

### Useful Commands

```bash
# Get all resources in namespace
kubectl get all -n production

# Describe problematic pod
kubectl describe pod <pod-name> -n production

# Get recent events
kubectl get events -n production --sort-by='.lastTimestamp'

# Port forward for debugging
kubectl port-forward deployment/libraryapp-auth 8080:8080 -n production

# Execute commands in pod
kubectl exec -it deployment/libraryapp-auth -n production -- /bin/bash

# Copy files from pod
kubectl cp production/libraryapp-auth-xxx:/app/logs ./logs
```

This deployment guide provides comprehensive information for deploying and maintaining the LibraryApp microservices system across different environments. Always follow the deployment checklist and monitor applications closely during and after deployments.