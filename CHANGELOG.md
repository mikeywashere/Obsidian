# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-03-26

### Added
- ASP.NET Core API for Minecraft Bedrock server lifecycle management (start, stop, logs)
- Blazor WebAssembly frontend with server settings UI and player tracking dashboard
- UDP proxy with RakNet packet parsing for real-time player position and session tracking
- SignalR hub for real-time log streaming and player event broadcasting
- Microsoft Identity authentication scoped to personal MSA accounts (outlook.com, hotmail.com, live.com)
- Admin role management via `UserAdminOverrides` database table
- Entity Framework Core with SQLite for local persistence; Npgsql ready for PostgreSQL upgrade
- .NET Aspire orchestration for local development (AppHost with service discovery)
- Local Kubernetes deployment via Helm chart (`helm/obsidian/`) with nginx ingress
- Multi-stage Dockerfiles for API (non-root, `/data` PVC volume) and Web (nginx + Blazor WASM)
- Runtime API URL injection in WASM container via `entrypoint.sh`
- Deployment scripts for Windows (`scripts/deploy-local.ps1`) and bash (`scripts/deploy-local.sh`) with auto-detection of minikube/kind/Docker Desktop
- CORS origins configurable via `CORS:AllowedOrigins` config key
- Health endpoints at `/health` (readiness) and `/alive` (liveness)
- Solution migrated to `.slnx` format

[Unreleased]: https://github.com/mikeywashere/Obsidian/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/mikeywashere/Obsidian/releases/tag/v1.0.0
