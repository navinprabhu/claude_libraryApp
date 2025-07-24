# Azure Cloud Deployment Guide for LibraryApp

This comprehensive guide provides step-by-step instructions for deploying the LibraryApp microservices system to Microsoft Azure using native Azure services and Azure Kubernetes Service (AKS).

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Cost Estimation](#cost-estimation)
4. [Phase-by-Phase Deployment](#phase-by-phase-deployment)
5. [Configuration Management](#configuration-management)
6. [CI/CD Setup](#cicd-setup)
7. [Monitoring & Security](#monitoring--security)
8. [Troubleshooting](#troubleshooting)

## 🛠️ Prerequisites

### Required Tools
- **Azure CLI** (v2.50+) - [Install Guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **kubectl** - [Install Guide](https://kubernetes.io/docs/tasks/tools/)
- **Helm** (v3.0+) - [Install Guide](https://helm.sh/docs/intro/install/)
- **Docker Desktop** - [Install Guide](https://docs.docker.com/desktop/)
- **Git** - [Install Guide](https://git-scm.com/downloads)

### Azure Subscription Requirements
- **Active Azure Subscription** with appropriate permissions
- **Resource Group Creation** permissions
- **Kubernetes Service Admin** role
- **Key Vault Administrator** role
- **SQL Database Contributor** role

### Local Development Setup
```bash
# Clone the repository
git clone https://github.com/navinprabhu/claude_libraryApp.git
cd claude_libraryApp

# Login to Azure
az login

# Set default subscription (if you have multiple)
az account set --subscription "your-subscription-id"
```

## 🏗️ Architecture Overview

### Azure Services Architecture
```
                           🌐 Internet
                              │
                    ┌─────────▼─────────┐
                    │  Azure Front Door │
                    │  (CDN + WAF)      │
                    └─────────┬─────────┘
                              │
                ┌─────────────▼─────────────┐
                │  Azure Application Gateway │
                │  (Load Balancer + SSL)    │
                └─────────────┬─────────────┘
                              │
                    ┌─────────▼─────────┐
                    │ Azure Kubernetes  │
                    │ Service (AKS)     │
                    │                   │
                    │ ┌───────────────┐ │
                    │ │ Frontend Pods │ │
                    │ └───────────────┘ │
                    │ ┌───────────────┐ │
                    │ │Gateway Pods   │ │
                    │ └───────────────┘ │
                    │ ┌───────────────┐ │
                    │ │Service Pods   │ │
                    │ └───────────────┘ │
                    └─────────┬─────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
┌───────▼───────┐    ┌────────▼────────┐   ┌────────▼────────┐
│Azure Database │    │ Azure Cache     │   │ Azure Key Vault │
│for PostgreSQL │    │ for Redis       │   │ (Secrets)       │
│(Flexible)     │    │                 │   │                 │
└───────────────┘    └─────────────────┘   └─────────────────┘
        │
┌───────▼───────┐
│ Azure Monitor │
│ + Log         │
│ Analytics     │
└───────────────┘
```

### Service Mapping
| Local Service | Azure Service | Purpose |
|---------------|---------------|---------|
| Docker Compose | Azure Kubernetes Service (AKS) | Container orchestration |
| PostgreSQL Containers | Azure Database for PostgreSQL | Managed database service |
| Redis Container | Azure Cache for Redis | Managed cache service |
| Local Secrets | Azure Key Vault | Secure secrets management |
| Docker Registry | Azure Container Registry | Private container images |
| Local Load Balancer | Azure Application Gateway | Load balancing + WAF |
| File Storage | Azure Storage Account | Static files + backups |

## 💰 Cost Estimation

### Monthly Cost Breakdown (USD)

| Service | SKU | vCPUs/Memory | Storage | Est. Cost |
|---------|-----|--------------|---------|-----------|
| **AKS Cluster** | Standard_D2s_v3 (3 nodes) | 6 vCPUs, 18GB RAM | - | $150-300 |
| **Azure Database for PostgreSQL** | General Purpose B2s | 2 vCPUs, 4GB RAM | 32GB SSD | $120-200 |
| **Azure Cache for Redis** | Basic C1 | 1GB Memory | - | $20-40 |
| **Azure Container Registry** | Basic | - | 10GB | $5-15 |
| **Azure Key Vault** | Standard | - | - | $2-5 |
| **Azure Application Gateway** | Standard v2 | 2 Compute Units | - | $30-50 |
| **Azure Front Door** | Standard | - | - | $25-45 |
| **Azure Monitor + Log Analytics** | Pay-as-you-go | - | 5GB/month | $20-40 |
| **Azure Storage Account** | Standard LRS | - | 100GB | $5-10 |
| **Networking (Bandwidth)** | - | - | 100GB outbound | $10-20 |
| **Total Estimated Cost** | | | | **$387-725/month** |

### Cost Optimization Tips
1. **Use Azure Reserved Instances** for AKS nodes (save 30-40%)
2. **Enable auto-scaling** to scale down during low usage
3. **Use Azure Hybrid Benefit** if you have Windows Server licenses
4. **Monitor and optimize** resource usage regularly
5. **Use Azure Cost Management** for budget alerts

## 🚀 Phase-by-Phase Deployment

### Phase 1: Infrastructure Setup (Week 1)

#### 1.1 Create Resource Group
```bash
# Set variables
export RESOURCE_GROUP="rg-libraryapp-prod"
export LOCATION="eastus2"
export PROJECT_NAME="libraryapp"

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --tags project=$PROJECT_NAME environment=production
```

#### 1.2 Create Virtual Network
```bash
# Create VNet with multiple subnets
az network vnet create \
  --resource-group $RESOURCE_GROUP \
  --name vnet-libraryapp \
  --address-prefix 10.0.0.0/16 \
  --subnet-name subnet-aks \
  --subnet-prefix 10.0.1.0/24

# Create additional subnets
az network vnet subnet create \
  --resource-group $RESOURCE_GROUP \
  --vnet-name vnet-libraryapp \
  --name subnet-appgateway \
  --address-prefix 10.0.2.0/24

az network vnet subnet create \
  --resource-group $RESOURCE_GROUP \
  --vnet-name vnet-libraryapp \
  --name subnet-database \
  --address-prefix 10.0.3.0/24
```

#### 1.3 Create Azure Container Registry
```bash
export ACR_NAME="acrlibraryapp$(date +%s)"

# Create ACR
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $ACR_NAME \
  --sku Basic \
  --admin-enabled true

# Get ACR login server
export ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
echo "ACR Login Server: $ACR_LOGIN_SERVER"
```

#### 1.4 Create Azure Database for PostgreSQL
```bash
export POSTGRES_SERVER="psql-libraryapp-prod"
export POSTGRES_ADMIN="libraryadmin"
export POSTGRES_PASSWORD="SecurePassword123!"

# Create PostgreSQL flexible server
az postgres flexible-server create \
  --resource-group $RESOURCE_GROUP \
  --name $POSTGRES_SERVER \
  --location $LOCATION \
  --admin-user $POSTGRES_ADMIN \
  --admin-password $POSTGRES_PASSWORD \
  --sku-name Standard_B2s \
  --tier Burstable \
  --storage-size 32 \
  --version 15 \
  --public-access 0.0.0.0 \
  --vnet vnet-libraryapp \
  --subnet subnet-database

# Create individual databases
az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $POSTGRES_SERVER \
  --database-name AuthDatabase

az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $POSTGRES_SERVER \
  --database-name BookDatabase

az postgres flexible-server db create \
  --resource-group $RESOURCE_GROUP \
  --server-name $POSTGRES_SERVER \
  --database-name MemberDatabase

# Get connection string
export POSTGRES_CONNECTION_STRING="Host=$POSTGRES_SERVER.postgres.database.azure.com;Port=5432;Database={DATABASE_NAME};Username=$POSTGRES_ADMIN;Password=$POSTGRES_PASSWORD;SSL Mode=Require;"
```

#### 1.5 Create Azure Cache for Redis
```bash
export REDIS_NAME="redis-libraryapp-prod"

# Create Redis cache
az redis create \
  --resource-group $RESOURCE_GROUP \
  --name $REDIS_NAME \
  --location $LOCATION \
  --sku Basic \
  --vm-size c1 \
  --subnet-id $(az network vnet subnet show --resource-group $RESOURCE_GROUP --vnet-name vnet-libraryapp --name subnet-database --query id --output tsv)

# Get Redis connection string
export REDIS_PRIMARY_KEY=$(az redis list-keys --resource-group $RESOURCE_GROUP --name $REDIS_NAME --query primaryKey --output tsv)
export REDIS_CONNECTION_STRING="$REDIS_NAME.redis.cache.windows.net:6380,password=$REDIS_PRIMARY_KEY,ssl=True,abortConnect=False"
```

#### 1.6 Create Azure Key Vault
```bash
export KEYVAULT_NAME="kv-libraryapp-$(date +%s)"

# Create Key Vault
az keyvault create \
  --name $KEYVAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku standard \
  --enable-rbac-authorization true

# Store secrets in Key Vault
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "postgres-connection-string" \
  --value "$POSTGRES_CONNECTION_STRING"

az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "redis-connection-string" \
  --value "$REDIS_CONNECTION_STRING"

az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "jwt-secret-key" \
  --value "YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789"
```

### Phase 2: Kubernetes Setup (Week 2)

#### 2.1 Create AKS Cluster
```bash
export AKS_NAME="aks-libraryapp-prod"

# Create AKS cluster
az aks create \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME \
  --node-count 3 \
  --node-vm-size Standard_D2s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys \
  --attach-acr $ACR_NAME \
  --enable-managed-identity \
  --network-plugin azure \
  --vnet-subnet-id $(az network vnet subnet show --resource-group $RESOURCE_GROUP --vnet-name vnet-libraryapp --name subnet-aks --query id --output tsv) \
  --enable-cluster-autoscaler \
  --min-count 1 \
  --max-count 5

# Get AKS credentials
az aks get-credentials \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_NAME \
  --overwrite-existing

# Verify cluster connection
kubectl get nodes
```

#### 2.2 Install Required Add-ons
```bash
# Install Azure Key Vault provider for Secrets Store CSI driver
az aks enable-addons \
  --addons azure-keyvault-secrets-provider \
  --name $AKS_NAME \
  --resource-group $RESOURCE_GROUP

# Install NGINX Ingress Controller
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update

helm install ingress-nginx ingress-nginx/ingress-nginx \
  --namespace ingress-nginx \
  --create-namespace \
  --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-health-probe-request-path"=/healthz
```

### Phase 3: Configuration Management (Weeks 3-4)

#### 3.1 Create Kubernetes Namespace and RBAC
```bash
# Create namespace
kubectl create namespace libraryapp

# Create service account
kubectl create serviceaccount libraryapp-service-account -n libraryapp

# Get AKS managed identity
export AKS_IDENTITY_CLIENT_ID=$(az aks show --resource-group $RESOURCE_GROUP --name $AKS_NAME --query identityProfile.kubeletidentity.clientId --output tsv)

# Grant Key Vault access to AKS managed identity
az keyvault set-policy \
  --name $KEYVAULT_NAME \
  --object-id $(az ad sp show --id $AKS_IDENTITY_CLIENT_ID --query id --output tsv) \
  --secret-permissions get list
```

#### 3.2 Create Configuration Files

**Create `k8s/namespace.yaml`:**
```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: libraryapp
  labels:
    name: libraryapp
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: libraryapp-service-account
  namespace: libraryapp
```

**Create `k8s/configmap.yaml`:**
```yaml
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
  ASPNETCORE_URLS: "http://+:5001"
```

**Create `k8s/secret-provider.yaml`:**
```yaml
apiVersion: secrets-store.csi.x-k8s.io/v1
kind: SecretProviderClass
metadata:
  name: libraryapp-secrets
  namespace: libraryapp
spec:
  provider: azure
  parameters:
    usePodIdentity: "false"
    useVMManagedIdentity: "true"
    userAssignedIdentityID: "${AKS_IDENTITY_CLIENT_ID}"
    keyvaultName: "${KEYVAULT_NAME}"
    objects: |
      array:
        - |
          objectName: postgres-connection-string
          objectType: secret
          objectVersion: ""
        - |
          objectName: redis-connection-string
          objectType: secret
          objectVersion: ""
        - |
          objectName: jwt-secret-key
          objectType: secret
          objectVersion: ""
    tenantId: "${AZURE_TENANT_ID}"
  secretObjects:
  - secretName: libraryapp-secrets
    type: Opaque
    data:
    - objectName: postgres-connection-string
      key: POSTGRES_CONNECTION_STRING
    - objectName: redis-connection-string
      key: REDIS_CONNECTION_STRING
    - objectName: jwt-secret-key
      key: JWT_SECRET_KEY
```

#### 3.3 Apply Base Configuration
```bash
# Apply namespace and RBAC
kubectl apply -f k8s/namespace.yaml

# Apply ConfigMap
envsubst < k8s/configmap.yaml | kubectl apply -f -

# Apply Secret Provider Class
export AZURE_TENANT_ID=$(az account show --query tenantId --output tsv)
envsubst < k8s/secret-provider.yaml | kubectl apply -f -
```

### Phase 4: Application Deployment (Weeks 5-6)

#### 4.1 Build and Push Docker Images
```bash
# Build backend services
az acr build --registry $ACR_NAME --image libraryapp-auth:latest ./LibraryApp.AuthService
az acr build --registry $ACR_NAME --image libraryapp-book:latest ./LibraryApp.BookService
az acr build --registry $ACR_NAME --image libraryapp-member:latest ./LibraryApp.MemberService
az acr build --registry $ACR_NAME --image libraryapp-gateway:latest ./LibraryApp.ApiGateway

# Build frontend (create production Dockerfile first)
cd library-frontend
cat > Dockerfile.prod << 'EOF'
# Build stage
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
EOF

# Create nginx config
cat > nginx.conf << 'EOF'
events {
    worker_connections 1024;
}
http {
    include       /etc/nginx/mime.types;
    default_type  application/octet-stream;
    
    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;
        
        location / {
            try_files $uri $uri/ /index.html;
        }
        
        location /health {
            access_log off;
            return 200 "healthy\n";
            add_header Content-Type text/plain;
        }
    }
}
EOF

# Build frontend image
az acr build --registry $ACR_NAME --image libraryapp-frontend:latest . -f Dockerfile.prod
cd ..
```

#### 4.2 Create Service Deployments

**Create `k8s/auth-service.yaml`:**
```yaml
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
        image: ${ACR_LOGIN_SERVER}/libraryapp-auth:latest
        ports:
        - containerPort: 5001
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        envFrom:
        - configMapRef:
            name: libraryapp-config
        volumeMounts:
        - name: secrets-store
          mountPath: "/mnt/secrets-store"
          readOnly: true
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: libraryapp-secrets
              key: POSTGRES_CONNECTION_STRING
        - name: JwtSettings__SecretKey
          valueFrom:
            secretKeyRef:
              name: libraryapp-secrets
              key: JWT_SECRET_KEY
        - name: Redis__ConnectionString
          valueFrom:
            secretKeyRef:
              name: libraryapp-secrets
              key: REDIS_CONNECTION_STRING
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
      volumes:
      - name: secrets-store
        csi:
          driver: secrets-store.csi.k8s.io
          readOnly: true
          volumeAttributes:
            secretProviderClass: "libraryapp-secrets"
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

**Similarly create deployment files for book-service, member-service, api-gateway, and frontend following the same pattern.**

#### 4.3 Deploy All Services
```bash
# Replace environment variables in deployment files
envsubst < k8s/auth-service.yaml | kubectl apply -f -
envsubst < k8s/book-service.yaml | kubectl apply -f -
envsubst < k8s/member-service.yaml | kubectl apply -f -
envsubst < k8s/api-gateway.yaml | kubectl apply -f -
envsubst < k8s/frontend.yaml | kubectl apply -f -

# Check deployment status
kubectl get pods -n libraryapp
kubectl get services -n libraryapp
```

### Phase 5: Load Balancer & Ingress (Week 7)

#### 5.1 Create Application Gateway
```bash
export APP_GATEWAY_NAME="appgw-libraryapp"

# Create public IP for Application Gateway
az network public-ip create \
  --resource-group $RESOURCE_GROUP \
  --name pip-appgateway \
  --allocation-method Static \
  --sku Standard

# Create Application Gateway
az network application-gateway create \
  --name $APP_GATEWAY_NAME \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --vnet-name vnet-libraryapp \
  --subnet subnet-appgateway \
  --capacity 2 \
  --sku Standard_v2 \
  --public-ip-address pip-appgateway \
  --frontend-port 80 \
  --http-settings-cookie-based-affinity Disabled \
  --http-settings-port 80 \
  --http-settings-protocol Http
```

#### 5.2 Configure Ingress

**Create `k8s/ingress.yaml`:**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: libraryapp-ingress
  namespace: libraryapp
  annotations:
    kubernetes.io/ingress.class: azure/application-gateway
    appgw.ingress.kubernetes.io/health-probe-path: "/health"
    appgw.ingress.kubernetes.io/health-probe-interval: "30"
    appgw.ingress.kubernetes.io/health-probe-timeout: "30"
    appgw.ingress.kubernetes.io/health-probe-unhealthy-threshold: "3"
spec:
  rules:
  - host: libraryapp.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: frontend
            port:
              number: 80
  - host: api.libraryapp.yourdomain.com
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

```bash
# Apply ingress configuration
kubectl apply -f k8s/ingress.yaml

# Get ingress external IP
kubectl get ingress -n libraryapp
```

### Phase 6: CI/CD Pipeline Setup (Week 8)

#### 6.1 Create GitHub Secrets

In your GitHub repository, go to Settings > Secrets and variables > Actions, and add:

```
AZURE_CREDENTIALS = {
  "clientId": "<service-principal-client-id>",
  "clientSecret": "<service-principal-client-secret>",
  "subscriptionId": "<azure-subscription-id>",
  "tenantId": "<azure-tenant-id>",
  "resourceManagerEndpointUrl": "https://management.azure.com/"
}
ACR_LOGIN_SERVER = <your-acr-login-server>
AKS_CLUSTER_NAME = aks-libraryapp-prod
AKS_RESOURCE_GROUP = rg-libraryapp-prod
```

#### 6.2 Create GitHub Actions Workflow

**Create `.github/workflows/azure-deploy.yml`:**
```yaml
name: Deploy to Azure AKS

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  AZURE_RESOURCE_GROUP: ${{ secrets.AKS_RESOURCE_GROUP }}
  AKS_CLUSTER_NAME: ${{ secrets.AKS_CLUSTER_NAME }}
  ACR_LOGIN_SERVER: ${{ secrets.ACR_LOGIN_SERVER }}

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v4
    
    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Build and push backend images
      run: |
        az acr build --registry ${{ env.ACR_LOGIN_SERVER }} \
          --image libraryapp-auth:${{ github.sha }} \
          ./LibraryApp.AuthService
        az acr build --registry ${{ env.ACR_LOGIN_SERVER }} \
          --image libraryapp-book:${{ github.sha }} \
          ./LibraryApp.BookService
        az acr build --registry ${{ env.ACR_LOGIN_SERVER }} \
          --image libraryapp-member:${{ github.sha }} \
          ./LibraryApp.MemberService
        az acr build --registry ${{ env.ACR_LOGIN_SERVER }} \
          --image libraryapp-gateway:${{ github.sha }} \
          ./LibraryApp.ApiGateway
    
    - name: Build and push frontend image
      run: |
        cd library-frontend
        az acr build --registry ${{ env.ACR_LOGIN_SERVER }} \
          --image libraryapp-frontend:${{ github.sha }} \
          . -f Dockerfile.prod
    
    - name: Get AKS credentials
      run: |
        az aks get-credentials \
          --resource-group ${{ env.AZURE_RESOURCE_GROUP }} \
          --name ${{ env.AKS_CLUSTER_NAME }}
    
    - name: Deploy to AKS
      run: |
        # Update deployment images
        kubectl set image deployment/auth-service \
          auth-service=${{ env.ACR_LOGIN_SERVER }}/libraryapp-auth:${{ github.sha }} \
          -n libraryapp
        kubectl set image deployment/book-service \
          book-service=${{ env.ACR_LOGIN_SERVER }}/libraryapp-book:${{ github.sha }} \
          -n libraryapp
        kubectl set image deployment/member-service \
          member-service=${{ env.ACR_LOGIN_SERVER }}/libraryapp-member:${{ github.sha }} \
          -n libraryapp
        kubectl set image deployment/api-gateway \
          api-gateway=${{ env.ACR_LOGIN_SERVER }}/libraryapp-gateway:${{ github.sha }} \
          -n libraryapp
        kubectl set image deployment/frontend \
          frontend=${{ env.ACR_LOGIN_SERVER }}/libraryapp-frontend:${{ github.sha }} \
          -n libraryapp
    
    - name: Verify deployment
      run: |
        kubectl rollout status deployment/auth-service -n libraryapp --timeout=300s
        kubectl rollout status deployment/book-service -n libraryapp --timeout=300s
        kubectl rollout status deployment/member-service -n libraryapp --timeout=300s
        kubectl rollout status deployment/api-gateway -n libraryapp --timeout=300s
        kubectl rollout status deployment/frontend -n libraryapp --timeout=300s
        
        # Run health checks
        kubectl get pods -n libraryapp
```

### Phase 7: Monitoring & Observability (Week 9)

#### 7.1 Configure Azure Monitor
```bash
# Enable Container Insights
az aks enable-addons \
  --addons monitoring \
  --name $AKS_NAME \
  --resource-group $RESOURCE_GROUP

# Create Log Analytics Workspace
az monitor log-analytics workspace create \
  --resource-group $RESOURCE_GROUP \
  --workspace-name law-libraryapp \
  --location $LOCATION

# Get workspace ID
export LOG_ANALYTICS_WORKSPACE_ID=$(az monitor log-analytics workspace show \
  --resource-group $RESOURCE_GROUP \
  --workspace-name law-libraryapp \
  --query customerId \
  --output tsv)
```

#### 7.2 Application Insights Integration
```bash
# Create Application Insights
az monitor app-insights component create \
  --app libraryapp-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP \
  --application-type web

# Get connection string
export APP_INSIGHTS_CONNECTION_STRING=$(az monitor app-insights component show \
  --app libraryapp-insights \
  --resource-group $RESOURCE_GROUP \
  --query connectionString \
  --output tsv)

# Store in Key Vault
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "application-insights-connection-string" \
  --value "$APP_INSIGHTS_CONNECTION_STRING"
```

### Phase 8: Security & Compliance (Week 10)

#### 8.1 Network Security Groups
```bash
# Create NSG for AKS subnet
az network nsg create \
  --resource-group $RESOURCE_GROUP \
  --name nsg-aks-subnet

# Allow inbound HTTPS
az network nsg rule create \
  --resource-group $RESOURCE_GROUP \
  --nsg-name nsg-aks-subnet \
  --name allow-https-inbound \
  --protocol tcp \
  --priority 100 \
  --destination-port-ranges 443 \
  --access allow \
  --direction inbound

# Associate NSG with AKS subnet
az network vnet subnet update \
  --resource-group $RESOURCE_GROUP \
  --vnet-name vnet-libraryapp \
  --name subnet-aks \
  --network-security-group nsg-aks-subnet
```

#### 8.2 SSL Certificate Setup
```bash
# Create managed certificate (requires custom domain)
az network application-gateway ssl-cert create \
  --resource-group $RESOURCE_GROUP \
  --gateway-name $APP_GATEWAY_NAME \
  --name libraryapp-ssl-cert \
  --cert-file /path/to/your/certificate.pfx \
  --cert-password "your-cert-password"
```

## 🔧 Configuration Management

### Environment-Specific Settings

**Development Environment:**
- Local Docker containers
- In-memory/local databases
- Console logging
- Debug mode enabled

**Production Environment:**
- Azure managed services
- Azure Database for PostgreSQL
- Azure Monitor logging
- Production optimizations

### Configuration Sources Priority
1. **Kubernetes Secrets** (highest priority)
2. **ConfigMaps**
3. **Environment Variables**
4. **appsettings.Production.json**
5. **appsettings.json** (lowest priority)

### Database Connection Management

**Development:**
```csharp
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=AuthDatabase;Username=auth_user;Password=password123"
}
```

**Production:**
```csharp
"ConnectionStrings": {
  "DefaultConnection": "Host=psql-libraryapp-prod.postgres.database.azure.com;Port=5432;Database=AuthDatabase;Username=libraryadmin;Password={from-keyvault};SSL Mode=Require;"
}
```

## 📊 Monitoring & Alerting

### Key Metrics to Monitor

1. **Application Metrics:**
   - Request rate and response time
   - Error rate and status codes
   - Authentication success/failure rates
   - Database connection pool usage

2. **Infrastructure Metrics:**
   - Pod CPU and memory usage
   - Node resource utilization
   - Database performance metrics
   - Redis cache hit rate

3. **Business Metrics:**
   - Book borrowing rates
   - User activity patterns
   - System availability

### Alert Configuration
```bash
# Create action group for notifications
az monitor action-group create \
  --resource-group $RESOURCE_GROUP \
  --name libraryapp-alerts \
  --short-name libapp-alert \
  --email-receivers name=admin email=admin@yourdomain.com

# Create high CPU alert
az monitor metrics alert create \
  --name "High CPU Usage" \
  --resource-group $RESOURCE_GROUP \
  --scopes "/subscriptions/{subscription-id}/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.ContainerService/managedClusters/$AKS_NAME" \
  --condition "avg Percentage CPU > 80" \
  --window-size 5m \
  --evaluation-frequency 1m \
  --action libraryapp-alerts \
  --description "Alert when AKS CPU usage exceeds 80%"
```

## 🛡️ Security Best Practices

### 1. Identity and Access Management
- Use Azure Active Directory for authentication
- Implement least privilege access
- Enable multi-factor authentication
- Regular access reviews

### 2. Network Security
- Use private endpoints for Azure services
- Implement network segmentation
- Enable Azure Firewall
- Regular security assessments

### 3. Data Protection
- Enable encryption at rest and in transit
- Use Azure Key Vault for secrets
- Implement data backup strategies
- Regular vulnerability scans

### 4. Container Security
- Use minimal base images
- Scan images for vulnerabilities
- Implement Pod Security Standards
- Regular updates and patches

## 🔍 Troubleshooting

### Common Issues and Solutions

#### 1. Pod Startup Issues
```bash
# Check pod status
kubectl get pods -n libraryapp

# Describe pod for events
kubectl describe pod <pod-name> -n libraryapp

# Check logs
kubectl logs <pod-name> -n libraryapp
```

#### 2. Database Connection Issues
```bash
# Test database connectivity
kubectl run -it --rm debug --image=postgres:15 --restart=Never -- psql -h psql-libraryapp-prod.postgres.database.azure.com -U libraryadmin -d AuthDatabase

# Check secrets
kubectl get secret libraryapp-secrets -n libraryapp -o yaml
```

#### 3. Service Discovery Issues
```bash
# Check service endpoints
kubectl get endpoints -n libraryapp

# Test service connectivity
kubectl run -it --rm debug --image=busybox --restart=Never -- nslookup auth-service.libraryapp.svc.cluster.local
```

#### 4. Ingress Issues
```bash
# Check ingress status
kubectl get ingress -n libraryapp

# Check Application Gateway logs
az network application-gateway show \
  --resource-group $RESOURCE_GROUP \
  --name $APP_GATEWAY_NAME
```

### Debugging Commands

```bash
# Get all resources in namespace
kubectl get all -n libraryapp

# Check resource usage
kubectl top nodes
kubectl top pods -n libraryapp

# Check cluster events
kubectl get events --sort-by=.metadata.creationTimestamp

# Access pod shell
kubectl exec -it <pod-name> -n libraryapp -- /bin/bash
```

## 📝 Post-Deployment Checklist

### ✅ Functional Testing
- [ ] All services are running and healthy
- [ ] Database connections are working
- [ ] Redis cache is operational
- [ ] Frontend is accessible via public URL
- [ ] API Gateway is routing requests correctly
- [ ] Authentication flow is working
- [ ] Book management operations work
- [ ] Member management operations work

### ✅ Performance Testing
- [ ] Load testing completed
- [ ] Response times within acceptable limits
- [ ] Auto-scaling working correctly
- [ ] Database performance optimized
- [ ] Cache hit rates are optimal

### ✅ Security Testing
- [ ] SSL certificates configured and valid
- [ ] Security scanning completed
- [ ] Network policies enforced
- [ ] Secret management working
- [ ] Access controls validated

### ✅ Monitoring & Alerting
- [ ] All monitoring dashboards configured
- [ ] Alerts are working and tested
- [ ] Log aggregation functioning
- [ ] Backup strategies implemented
- [ ] Disaster recovery plan documented

### ✅ Documentation
- [ ] Architecture documentation updated
- [ ] Operational runbooks created
- [ ] Troubleshooting guides written
- [ ] Contact information documented
- [ ] Change management process defined

## 📞 Support and Maintenance

### Regular Maintenance Tasks

**Weekly:**
- Review application logs for errors
- Check resource utilization
- Verify backup completion
- Review security alerts

**Monthly:**
- Update container images
- Review and rotate secrets
- Performance optimization
- Cost optimization review

**Quarterly:**
- Security assessment
- Disaster recovery testing
- Architecture review
- Capacity planning

### Support Contacts
- **Azure Support**: Azure Portal > Support
- **Application Issues**: GitHub Issues
- **Security Issues**: Private security disclosure process

---

This deployment guide provides a comprehensive roadmap for deploying the LibraryApp to Azure. Each phase builds upon the previous one, ensuring a systematic and secure deployment process.

For additional support or questions about this deployment guide, please create an issue in the GitHub repository.