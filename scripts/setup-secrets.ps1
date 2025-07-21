# PowerShell script to setup Kubernetes secrets for LibraryApp
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("staging", "production")]
    [string]$Environment,
    
    [Parameter(Mandatory=$false)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$false)]
    [string]$KeyVaultName
)

Write-Host "Setting up secrets for $Environment environment..." -ForegroundColor Green

# Function to create Kubernetes secret from Azure Key Vault
function New-K8sSecretFromKeyVault {
    param(
        [string]$SecretName,
        [string]$Namespace,
        [hashtable]$KeyVaultSecrets
    )
    
    Write-Host "Creating secret: $SecretName in namespace: $Namespace" -ForegroundColor Yellow
    
    # Build kubectl command with data from Key Vault
    $secretData = @()
    foreach ($kvp in $KeyVaultSecrets.GetEnumerator()) {
        $vaultValue = az keyvault secret show --name $kvp.Value --vault-name $KeyVaultName --query "value" -o tsv
        if ($vaultValue) {
            $encodedValue = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($vaultValue))
            $secretData += "--from-literal=$($kvp.Key)=$encodedValue"
        } else {
            Write-Warning "Could not retrieve secret $($kvp.Value) from Key Vault"
        }
    }
    
    if ($secretData.Count -gt 0) {
        $command = "kubectl create secret generic $SecretName --namespace=$Namespace " + ($secretData -join " ")
        Write-Host "Executing: $command" -ForegroundColor Gray
        Invoke-Expression $command
    }
}

# Function to create generic Kubernetes secret
function New-K8sSecret {
    param(
        [string]$SecretName,
        [string]$Namespace,
        [hashtable]$SecretData
    )
    
    Write-Host "Creating secret: $SecretName in namespace: $Namespace" -ForegroundColor Yellow
    
    # Check if secret already exists
    $existingSecret = kubectl get secret $SecretName -n $Namespace --ignore-not-found=true
    if ($existingSecret) {
        Write-Host "Secret $SecretName already exists. Deleting..." -ForegroundColor Yellow
        kubectl delete secret $SecretName -n $Namespace
    }
    
    # Build kubectl command
    $secretArgs = @()
    foreach ($kvp in $SecretData.GetEnumerator()) {
        $secretArgs += "--from-literal=$($kvp.Key)=$($kvp.Value)"
    }
    
    $command = "kubectl create secret generic $SecretName --namespace=$Namespace " + ($secretArgs -join " ")
    Write-Host "Executing: $command" -ForegroundColor Gray
    Invoke-Expression $command
}

# Ensure kubectl is configured
Write-Host "Checking kubectl configuration..." -ForegroundColor Blue
$kubectlContext = kubectl config current-context
if (-not $kubectlContext) {
    Write-Error "kubectl is not configured. Please configure kubectl to connect to your cluster."
    exit 1
}
Write-Host "Current kubectl context: $kubectlContext" -ForegroundColor Green

# Ensure namespace exists
Write-Host "Ensuring namespace $Environment exists..." -ForegroundColor Blue
kubectl create namespace $Environment --dry-run=client -o yaml | kubectl apply -f -

# Set default environment-specific values
$defaultSecrets = @{
    staging = @{
        JwtSettings__SecretKey = "staging-super-secret-jwt-key-change-in-production-2024"
        JwtSettings__Issuer = "LibraryApp-Staging"
        JwtSettings__Audience = "LibraryApp-Users"
        JwtSettings__ExpirationMinutes = "60"
        ConnectionStrings__DefaultConnection = "Server=localhost;Database=LibraryApp_Staging;Trusted_Connection=true;"
        ApplicationInsights__ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000"
    }
    production = @{
        JwtSettings__SecretKey = "production-super-secret-jwt-key-change-me-2024-very-long-and-secure"
        JwtSettings__Issuer = "LibraryApp"
        JwtSettings__Audience = "LibraryApp-Users"
        JwtSettings__ExpirationMinutes = "60"
        ConnectionStrings__DefaultConnection = "Server=prod-server.database.windows.net;Database=LibraryApp_Prod;Authentication=Active Directory Managed Identity;"
        ApplicationInsights__ConnectionString = "InstrumentationKey=production-app-insights-key"
    }
}

# Database connection secrets
Write-Host "`nCreating database connection secrets..." -ForegroundColor Cyan

if ($KeyVaultName -and $SubscriptionId) {
    Write-Host "Using Azure Key Vault: $KeyVaultName" -ForegroundColor Blue
    
    # Login to Azure if needed
    $azAccount = az account show --query "id" -o tsv 2>$null
    if (-not $azAccount) {
        Write-Host "Logging into Azure..." -ForegroundColor Yellow
        az login
    }
    
    # Set subscription if provided
    if ($SubscriptionId) {
        az account set --subscription $SubscriptionId
    }
    
    # Create secrets from Key Vault
    $kvSecrets = @{
        "ConnectionStrings__DefaultConnection" = "libraryapp-db-connection-$Environment"
        "JwtSettings__SecretKey" = "libraryapp-jwt-secret-$Environment"
        "ApplicationInsights__ConnectionString" = "libraryapp-appinsights-$Environment"
    }
    
    New-K8sSecretFromKeyVault -SecretName "libraryapp-config" -Namespace $Environment -KeyVaultSecrets $kvSecrets
} else {
    Write-Host "Using default secrets (WARNING: Change these in production!)" -ForegroundColor Yellow
    
    # Create secrets with default values
    New-K8sSecret -SecretName "libraryapp-config" -Namespace $Environment -SecretData $defaultSecrets[$Environment]
}

# Service-specific secrets
Write-Host "`nCreating service-specific secrets..." -ForegroundColor Cyan

# Auth Service secrets
$authSecrets = @{
    "Admin__Email" = "admin@libraryapp.com"
    "Admin__Password" = "AdminPass123!"
    "Smtp__Host" = "smtp.sendgrid.net"
    "Smtp__Port" = "587"
    "Smtp__Username" = "apikey"
    "Smtp__Password" = "SG.your-sendgrid-api-key"
}

New-K8sSecret -SecretName "libraryapp-auth-secrets" -Namespace $Environment -SecretData $authSecrets

# Book Service secrets
$bookSecrets = @{
    "ExternalApis__GoogleBooks__ApiKey" = "your-google-books-api-key"
    "ExternalApis__OpenLibrary__ApiKey" = "your-openlibrary-api-key"
    "Storage__ConnectionString" = "DefaultEndpointsProtocol=https;AccountName=libraryapp;AccountKey=your-storage-key"
}

New-K8sSecret -SecretName "libraryapp-book-secrets" -Namespace $Environment -SecretData $bookSecrets

# Member Service secrets
$memberSecrets = @{
    "EmailService__ApiKey" = "your-email-service-api-key"
    "NotificationService__ApiKey" = "your-notification-service-key"
}

New-K8sSecret -SecretName "libraryapp-member-secrets" -Namespace $Environment -SecretData $memberSecrets

# Monitoring secrets
Write-Host "`nCreating monitoring secrets..." -ForegroundColor Cyan

$monitoringSecrets = @{
    "grafana-admin-password" = "admin123!"
    "prometheus-config" = "basic-config"
    "alertmanager-slack-webhook" = "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK"
}

New-K8sSecret -SecretName "monitoring-secrets" -Namespace "monitoring" -SecretData $monitoringSecrets

# Docker registry secrets
Write-Host "`nCreating Docker registry secrets..." -ForegroundColor Cyan

$dockerConfigJson = @{
    auths = @{
        "docker.io" = @{
            username = $env:DOCKER_USERNAME
            password = $env:DOCKER_PASSWORD
            auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("$($env:DOCKER_USERNAME):$($env:DOCKER_PASSWORD)"))
        }
    }
} | ConvertTo-Json -Depth 10

# Create docker registry secret
kubectl create secret docker-registry regcred `
    --docker-server=docker.io `
    --docker-username=$env:DOCKER_USERNAME `
    --docker-password=$env:DOCKER_PASSWORD `
    --docker-email=$env:DOCKER_EMAIL `
    --namespace=$Environment `
    --dry-run=client -o yaml | kubectl apply -f -

# TLS secrets for ingress
Write-Host "`nCreating TLS secrets..." -ForegroundColor Cyan

if ($Environment -eq "production") {
    # Production TLS certificate (use cert-manager in real scenarios)
    Write-Host "For production, use cert-manager to automatically provision TLS certificates" -ForegroundColor Yellow
    Write-Host "Example cert-manager annotation: cert-manager.io/cluster-issuer: letsencrypt-prod" -ForegroundColor Gray
} else {
    # Self-signed certificate for staging
    Write-Host "Creating self-signed certificate for staging..." -ForegroundColor Yellow
    
    # Generate self-signed certificate
    $certPath = "$env:TEMP\staging-cert.crt"
    $keyPath = "$env:TEMP\staging-cert.key"
    
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 `
        -keyout $keyPath `
        -out $certPath `
        -subj "/C=US/ST=State/L=City/O=LibraryApp/OU=IT/CN=staging.libraryapp.com" `
        -addext "subjectAltName=DNS:staging.libraryapp.com,DNS:*.staging.libraryapp.com"
    
    if (Test-Path $certPath -and Test-Path $keyPath) {
        kubectl create secret tls staging-tls-secret `
            --cert=$certPath `
            --key=$keyPath `
            --namespace=$Environment `
            --dry-run=client -o yaml | kubectl apply -f -
            
        # Cleanup temp files
        Remove-Item $certPath, $keyPath -Force
    }
}

# Create RBAC for service accounts
Write-Host "`nCreating RBAC for service accounts..." -ForegroundColor Cyan

@"
apiVersion: v1
kind: ServiceAccount
metadata:
  name: libraryapp-serviceaccount
  namespace: $Environment
---
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  namespace: $Environment
  name: libraryapp-role
rules:
- apiGroups: [""]
  resources: ["pods", "services", "endpoints"]
  verbs: ["get", "list", "watch"]
- apiGroups: ["apps"]
  resources: ["deployments", "replicasets"]
  verbs: ["get", "list", "watch"]
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: libraryapp-rolebinding
  namespace: $Environment
subjects:
- kind: ServiceAccount
  name: libraryapp-serviceaccount
  namespace: $Environment
roleRef:
  kind: Role
  name: libraryapp-role
  apiGroup: rbac.authorization.k8s.io
"@ | kubectl apply -f -

# Verify secrets
Write-Host "`nVerifying created secrets..." -ForegroundColor Cyan
kubectl get secrets -n $Environment
Write-Host ""
kubectl get secrets -n monitoring

Write-Host "`nSecret setup completed successfully!" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor White
Write-Host "Secrets created:" -ForegroundColor White
Write-Host "  - libraryapp-config (main configuration)" -ForegroundColor Gray
Write-Host "  - libraryapp-auth-secrets (auth service specific)" -ForegroundColor Gray
Write-Host "  - libraryapp-book-secrets (book service specific)" -ForegroundColor Gray
Write-Host "  - libraryapp-member-secrets (member service specific)" -ForegroundColor Gray
Write-Host "  - regcred (docker registry)" -ForegroundColor Gray
Write-Host "  - staging-tls-secret (TLS certificate)" -ForegroundColor Gray
Write-Host ""
Write-Host "IMPORTANT SECURITY NOTES:" -ForegroundColor Red
Write-Host "1. Change default passwords and secrets in production" -ForegroundColor Yellow
Write-Host "2. Use Azure Key Vault or similar for production secrets" -ForegroundColor Yellow
Write-Host "3. Enable secret rotation policies" -ForegroundColor Yellow
Write-Host "4. Monitor secret access and usage" -ForegroundColor Yellow
Write-Host "5. Use cert-manager for automatic TLS certificate management" -ForegroundColor Yellow