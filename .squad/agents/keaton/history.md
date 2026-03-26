# Keaton — DevOps Engineer History

## Core Context

**Project:** Obsidian — Minecraft Bedrock server manager
**Stack:** .NET 10, ASP.NET Core API, Blazor WASM, SQLite, .NET Aspire
**User:** Michael R. Schmidt

## Learnings

### 2026-03-26: K8s deployment implementation
- Implemented full local Kubernetes deployment (ADR-008) on `feature/kubernetes-deployment` branch
- Created Dockerfiles for API (multi-stage, non-root, /data PVC) and Web (nginx + WASM)
- Created nginx.conf with SPA routing, WASM MIME types, gzip compression, security headers
- Created entrypoint.sh for runtime API URL injection via sed into appsettings.json
- Created complete Helm chart at helm/obsidian/ — ConfigMap, Secret, Deployments, Services, Ingress, PVC
- Created deploy-local.ps1 and deploy-local.sh with auto-detection of minikube/kind/Docker Desktop
- Added .dockerignore to minimize build context
- Fixed CORS to be configurable via CORS:AllowedOrigins config key
- Merged PR #43 (Copilot review fixes: non-idempotent entrypoint, CORS array binding, PVC conditionals)
- Merged PR #42 (k8s deployment) and PR #44 (copilot-sdk dependency) into main

### Key files owned
- source/Obsidian.Api/Dockerfile
- source/Obsidian.Web/Dockerfile
- source/Obsidian.Web/nginx.conf
- source/Obsidian.Web/entrypoint.sh
- helm/obsidian/ (full chart)
- scripts/deploy-local.ps1
- scripts/deploy-local.sh
- .github/workflows/dotnet.yml (CI pipeline)
- .dockerignore

### CI workflow notes
- CI uses ubuntu-latest with .NET 10.0.x + Node 18
- Installs Playwright browsers and .NET Aspire workload before build/test
- Solution file migrated to Obsidian.slnx format (PR #41)

### Deployment notes
- imagePullPolicy: Never for local images (no registry)
- CORS__AllowedOrigins__0 (indexed) required for .NET env-var array binding
- PVC volume/volumeMount wrapped in sqlite conditional to support future PostgreSQL
- entrypoint.sh restores from .template on each startup (idempotent)
- Ingress needs proxy-read-timeout: 3600 for SignalR WebSocket support
