<#
.SYNOPSIS
    Deploys Obsidian to a local Kubernetes cluster.

.PARAMETER ClientId
    Azure AD Client ID for the API (required).

.PARAMETER Audience
    Azure AD Audience for the API (defaults to api://ClientId).

.PARAMETER ImageTag
    Docker image tag to use (default: latest).

.PARAMETER SkipBuild
    Skip Docker build step (use existing images).

.EXAMPLE
    .\scripts\deploy-local.ps1 -ClientId "your-client-id"
#>
param(
    [string]$ClientId = "",
    [string]$Audience = "",
    [string]$ImageTag = "latest",
    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step { param([string]$Message) Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Write-Success { param([string]$Message) Write-Host "✓ $Message" -ForegroundColor Green }
function Write-Warn { param([string]$Message) Write-Host "⚠ $Message" -ForegroundColor Yellow }

# Validate required parameters
if (-not $ClientId) {
    throw "ClientId is required. Use -ClientId <your-azure-ad-client-id>"
}

# Preflight checks
Write-Step "Preflight checks"
foreach ($tool in @("kubectl", "helm", "docker")) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        throw "Required tool not found: $tool. Please install it and ensure it is on PATH."
    }
    Write-Success "$tool found"
}

# Detect cluster type
Write-Step "Detecting cluster type"
$clusterType = "docker-desktop"
if (Get-Command minikube -ErrorAction SilentlyContinue) {
    $minikubeStatus = minikube status --format='{{.Host}}' 2>$null
    if ($minikubeStatus -eq "Running") {
        $clusterType = "minikube"
    }
}
if (Get-Command kind -ErrorAction SilentlyContinue) {
    $kindClusters = kind get clusters 2>$null
    if ($kindClusters) { $clusterType = "kind" }
}
Write-Success "Cluster type: $clusterType"

# Build images
if (-not $SkipBuild) {
    Write-Step "Building Docker images"
    $repoRoot = Split-Path $PSScriptRoot -Parent
    Push-Location $repoRoot
    try {
        docker build -f "source\Obsidian.Api\Dockerfile" -t "obsidian-api:$ImageTag" .
        Write-Success "obsidian-api:$ImageTag built"
        docker build -f "source\Obsidian.Web\Dockerfile" -t "obsidian-web:$ImageTag" .
        Write-Success "obsidian-web:$ImageTag built"
    } finally {
        Pop-Location
    }
}

# Load images into cluster
Write-Step "Loading images into $clusterType"
switch ($clusterType) {
    "minikube" {
        minikube image load "obsidian-api:$ImageTag"
        minikube image load "obsidian-web:$ImageTag"
        Write-Success "Images loaded into minikube"
    }
    "kind" {
        $kindCluster = (kind get clusters)[0]
        kind load docker-image "obsidian-api:$ImageTag" --name $kindCluster
        kind load docker-image "obsidian-web:$ImageTag" --name $kindCluster
        Write-Success "Images loaded into kind cluster: $kindCluster"
    }
    "docker-desktop" {
        Write-Success "Docker Desktop shares daemon — images available automatically"
    }
}

# Build helm set args
$helmArgs = @(
    "upgrade", "--install", "obsidian", "./helm/obsidian",
    "--namespace", "obsidian", "--create-namespace",
    "--set", "api.image.tag=$ImageTag",
    "--set", "web.image.tag=$ImageTag"
)
if ($ClientId) { $helmArgs += "--set", "api.env.AzureAd__ClientId=$ClientId" }
if ($Audience) { $helmArgs += "--set", "api.env.AzureAd__Audience=$Audience" }
elseif ($ClientId) { $helmArgs += "--set", "api.env.AzureAd__Audience=api://$ClientId" }

# Deploy
Write-Step "Deploying with Helm"
& helm @helmArgs
Write-Success "Helm install/upgrade complete"

# Wait for rollout
Write-Step "Waiting for rollout"
kubectl rollout status deployment/obsidian-api -n obsidian --timeout=120s
kubectl rollout status deployment/obsidian-web -n obsidian --timeout=120s
Write-Success "All deployments ready"

# Done
Write-Host "`n" + ("=" * 60) -ForegroundColor Green
Write-Host "Obsidian deployed successfully!" -ForegroundColor Green
Write-Host ("=" * 60) -ForegroundColor Green
Write-Warn "Add this entry to your hosts file if not already present:"
Write-Host "  127.0.0.1 obsidian.local api.obsidian.local"
Write-Host ""
Write-Host "Access the application at: http://obsidian.local" -ForegroundColor Cyan
