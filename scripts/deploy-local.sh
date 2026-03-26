#!/usr/bin/env bash
set -euo pipefail

# Obsidian local Kubernetes deployment script
# Usage: ./scripts/deploy-local.sh [--client-id <id>] [--audience <aud>] [--tag <tag>] [--skip-build]

CLIENT_ID=""
AUDIENCE=""
IMAGE_TAG="latest"
SKIP_BUILD=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --client-id) CLIENT_ID="$2"; shift 2 ;;
        --audience)  AUDIENCE="$2";  shift 2 ;;
        --tag)       IMAGE_TAG="$2"; shift 2 ;;
        --skip-build) SKIP_BUILD=true; shift ;;
        *) echo "Unknown argument: $1"; exit 1 ;;
    esac
done

step()    { echo -e "\n==> $*"; }
success() { echo -e "✓ $*"; }
warn()    { echo -e "⚠ $*"; }

# Preflight checks
step "Preflight checks"
for tool in kubectl helm docker; do
    if ! command -v "$tool" &>/dev/null; then
        echo "Required tool not found: $tool. Please install it and ensure it is on PATH."
        exit 1
    fi
    success "$tool found"
done

# Detect cluster type
step "Detecting cluster type"
CLUSTER_TYPE="docker-desktop"
if command -v minikube &>/dev/null && minikube status --format='{{.Host}}' 2>/dev/null | grep -q Running; then
    CLUSTER_TYPE="minikube"
elif command -v kind &>/dev/null && kind get clusters 2>/dev/null | grep -q .; then
    CLUSTER_TYPE="kind"
fi
success "Cluster type: $CLUSTER_TYPE"

# Build images
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [ "$SKIP_BUILD" = false ]; then
    step "Building Docker images"
    docker build -f "source/Obsidian.Api/Dockerfile" -t "obsidian-api:$IMAGE_TAG" "$REPO_ROOT"
    success "obsidian-api:$IMAGE_TAG built"
    docker build -f "source/Obsidian.Web/Dockerfile" -t "obsidian-web:$IMAGE_TAG" "$REPO_ROOT"
    success "obsidian-web:$IMAGE_TAG built"
fi

# Load images into cluster
step "Loading images into $CLUSTER_TYPE"
case "$CLUSTER_TYPE" in
    minikube)
        minikube image load "obsidian-api:$IMAGE_TAG"
        minikube image load "obsidian-web:$IMAGE_TAG"
        success "Images loaded into minikube"
        ;;
    kind)
        KIND_CLUSTER=$(kind get clusters | head -1)
        kind load docker-image "obsidian-api:$IMAGE_TAG" --name "$KIND_CLUSTER"
        kind load docker-image "obsidian-web:$IMAGE_TAG" --name "$KIND_CLUSTER"
        success "Images loaded into kind cluster: $KIND_CLUSTER"
        ;;
    docker-desktop)
        success "Docker Desktop shares daemon — images available automatically"
        ;;
esac

# Build helm args
HELM_ARGS=(upgrade --install obsidian ./helm/obsidian
    --namespace obsidian --create-namespace
    --set "api.image.tag=$IMAGE_TAG"
    --set "web.image.tag=$IMAGE_TAG")
[ -n "$CLIENT_ID" ] && HELM_ARGS+=(--set "api.env.AzureAd__ClientId=$CLIENT_ID")
if [ -n "$AUDIENCE" ]; then
    HELM_ARGS+=(--set "api.env.AzureAd__Audience=$AUDIENCE")
elif [ -n "$CLIENT_ID" ]; then
    HELM_ARGS+=(--set "api.env.AzureAd__Audience=api://$CLIENT_ID")
fi

# Deploy
step "Deploying with Helm"
helm "${HELM_ARGS[@]}"
success "Helm install/upgrade complete"

# Wait for rollout
step "Waiting for rollout"
kubectl rollout status deployment/obsidian-api -n obsidian --timeout=120s
kubectl rollout status deployment/obsidian-web -n obsidian --timeout=120s
success "All deployments ready"

echo ""
echo "============================================================"
echo "Obsidian deployed successfully!"
echo "============================================================"
warn "Add this entry to your hosts file if not already present:"
echo "  127.0.0.1 obsidian.local api.obsidian.local"
echo ""
echo "Access the application at: http://obsidian.local"
