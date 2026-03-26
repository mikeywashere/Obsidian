# Squad Decisions

## Active Decisions

### ADR-001: Obsidian System Architecture

**Status:** Approved  
**Date:** 2026-03-21  
**Author:** Morpheus (Lead)

Obsidian is a .NET backend service for controlling Minecraft Bedrock servers. Approved three-layer architecture:

1. **Obsidian.Api** (NEW) — ASP.NET Core Web API with ServersController, PropertiesController, ServerLogHub (SignalR)
2. **Obsidian** (Core) — Refactored with BedrockProcess managing server lifecycle and stdout streaming
3. **Obsidian.Web** (Frontend) — Updated with HttpServerService to call Obsidian.Api instead of MockServerService

**Key Technical Decisions:**
- Process management via `System.Diagnostics.Process` with stdout redirection
- Real-time logs via SignalR hub grouped by serverId
- RESTful API with 8 endpoints for server lifecycle and properties management

**API Endpoints:**
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/servers` | List all servers |
| GET | `/api/servers/{id}` | Get server details |
| POST | `/api/servers` | Create new server |
| POST | `/api/servers/{id}/start` | Start server |
| POST | `/api/servers/{id}/stop` | Stop server |
| GET | `/api/servers/{id}/logs` | Get recent logs |
| GET | `/api/servers/{id}/properties` | Get server.properties |
| PUT | `/api/servers/{id}/properties` | Update server.properties |
| GET | `/api/versions` | List available Bedrock versions |

**Work Breakdown:**
- **Neo (Backend):** Build Obsidian.Api with ServerManager, ServersController, PropertiesController
- **Trinity (Frontend):** Build HttpServerService, HttpServerPropertiesService, wire DI in Blazor WASM

### ADR-002: Backend API Architecture

**Status:** Implemented ✅  
**Date:** 2026-03-20  
**Author:** Neo (Backend Dev)

Implemented clean separation of concerns for Obsidian.Api REST backend:

**Service Layer Pattern**
- `IServerManager` interface for testability
- `ServerManager` singleton with ConcurrentDictionary<string, ManagedServer>
- Private nested `ManagedServer` class encapsulating state

**Server Process Management**
- Cross-platform: Windows (bedrock_server.exe), Unix (bedrock_server)
- Full I/O redirection (stdout/stderr via event handlers)
- Graceful shutdown: send "stop" command, 10s timeout, force kill if needed
- Real-time log capture in-memory with configurable retention

**API Endpoints Implemented**
```
GET    /api/servers                     - List all servers
GET    /api/servers/{id}                - Get server details
GET    /api/servers/{id}/logs           - Get server logs
POST   /api/servers/{id}/start          - Start server
POST   /api/servers/{id}/stop           - Stop server
GET    /api/servers/{id}/properties     - Get server.properties
PUT    /api/servers/{id}/properties     - Update server.properties
```

**Technical Details**
- Resolved LogLevel namespace collision using fully-qualified names
- Made ServerProperties public for API access
- Permissive CORS for development (TODO: tighten for production)

**Known Gaps**
- PropertiesController hardcodes install path (needs refactoring)
- No persistence layer (in-memory only)
- SignalR hub for real-time logs not yet implemented
- No authentication/authorization
- No server registration/deletion endpoints

---

### ADR-003: Frontend HTTP Client Wiring

**Status:** Implemented ✅  
**Date:** 2026-03-25  
**Author:** Trinity (Frontend Dev)

Wired Blazor WASM frontend to Neo's backend API using dedicated HTTP services.

**HTTP Services**
- `HttpServerService`: Server CRUD/control (implements IServerService)
- `IServerPropertiesService` + `HttpServerPropertiesService`: Properties management
- Type-safe via GetFromJsonAsync / PutAsJsonAsync

**Configuration**
- API base URL from appsettings.json (fallback: https://localhost:5001/)
- Dedicated API HttpClient in DI (separate from WASM base client)
- SignalR Client package added (v9.0.14) for future real-time features

**Model Refactoring**
- Copied ServerProperties to Obsidian.Models for sharing
- Updated namespace: Obsidian → Obsidian.Models
- Console project retains copy (no breaking changes)

**Architecture Rationale**
- Clean separation: WASM base client vs API client
- Type-safe JSON serialization
- Configuration-driven API endpoint
- No breaking changes to existing code

---

### ADR-004: Player Tracking Architecture

**Status:** Implemented ✅  
**Date:** 2026-03-25  
**Author:** Morpheus (Lead) / Neo (Backend Dev)

Log-based player tracking for Minecraft Bedrock using server stdout parsing. UDP packet inspection rejected due to AES-256-CFB8 encryption blocking position data retrieval.

**Decision:** Implement player connect/disconnect parsing via regex on `ServerManager.LogReceived` events.

**Key Points:**
- Player join/leave events extracted from server logs
- XUID (Xbox User ID) captured for unique identification
- Spawn position parsed from `Player Spawned` log messages (not real-time movement)
- ~2-3 days implementation vs. ~2-3 weeks for encrypted UDP inspection
- Architecture: `IPlayerTracker` singleton, `PlayerBroadcaster` hosted service, SignalR hub

**REST Endpoint:** `GET /api/servers/{serverId}/players`  
**SignalR Hub:** `/hubs/players` with `PlayerJoined`/`PlayerLeft` events  
**Work Breakdown:** Neo (backend tracker + API), Trinity (UI), Tank (tests)

---

### ADR-005: RakNet Packet Parsing for UdpProxy

**Status:** Implemented ✅  
**Date:** 2026-05-28  
**Author:** Neo (Backend Dev)

Added protocol-aware packet parsing layer to `UdpProxy` for Minecraft Bedrock RakNet/MCPE diagnostics. Parses pre-login packets; encrypted post-login game packets identified but not decrypted (per encryption constraint).

**Architecture:**
- `RakNetPacketType` enum covering all known RakNet IDs
- `ParsedPacket` immutable record carrying structured fields
- `RakNetParser` static parser using `BinaryPrimitives` (never throws)
- `PacketParsed` event on `IUdpProxy` with `ParsedPacketEventArgs`

**Packets Handled:**
- UnconnectedPing/Pong (0x01, 0x1c) — MOTD, player count, world name
- OpenConnection handshake (0x05–0x08)
- NewIncomingConnection (0x13)
- DataPacket (0x80–0x8f) with 3-byte LE sequence numbers
- GamePacket (0xfe) — marked `IsEncrypted=false` (indeterminate)
- Ack/Nack (0xc0, 0xa0)

**Limitations:**
- GamePacket encryption state always false (login tracking out of scope)
- No DataPacket reliability header parsing
- Offline message ID magic not validated

**Result:** Build 0 errors, 72/72 tests pass

---

### ADR-006: SignalR Real-Time Log Streaming

**Status:** Implemented ✅  
**Date:** 2026-03-25  
**Author:** Neo (Backend Dev)

Event-driven SignalR architecture for broadcasting server logs to connected clients.

**Architecture:**
1. `ServerManager.LogReceived` event carrying `ServerLogEventArgs(ServerId, ServerLog)`
2. `ServerLogHub` providing `JoinServer(serverId)` and `LeaveServer(serverId)` group subscriptions
3. `ServerLogBroadcaster` hosted service bridging events → SignalR broadcast

**Hub Endpoint:** `/hubs/serverlogs`  
**Client → Server:** `JoinServer(serverId)`, `LeaveServer(serverId)`  
**Server → Client:** `ReceiveLog(ServerLog)` per `server-{serverId}` group

**CORS:** Explicit origins (https://localhost:7001, http://localhost:5002) with `AllowCredentials()`

**Consequence:** Real-time log delivery; fixed PropertiesController hardcoded path via `GetInstallPath(serverId)` on `IServerManager`.

---

### ADR-007: Frontend SignalR & Properties Editor

**Status:** Implemented ✅  
**Date:** 2026-03-25  
**Author:** Trinity (Frontend Dev)

Blazor WASM UI for real-time log streaming and server.properties editing.

**Design Decisions:**
1. `HubConnection` created in component (standard Blazor WASM pattern), not DI
2. Connection indicator (green "Live" / gray "Disconnected") near log heading
3. Removed "Refresh Logs" button (SignalR makes it obsolete)
4. Server properties grouped: Basic Settings, Server Options, World Settings
5. Clear loading/error/success states with "Retry" button

**Endpoint Dependencies:** `/hubs/serverlogs` (SignalR), `GET/PUT /api/servers/{id}/properties` (REST)

**Graceful Degradation:** If SignalR fails, logs load via REST polling.

---

### ADR-009: .NET Aspire Structure for Obsidian

**Date:** 2026-03-26  
**Author:** Morpheus (Lead / Architect)  
**Status:** Implemented  

The Obsidian solution needed .NET Aspire orchestration to manage service discovery, telemetry, and distributed application hosting. .NET Aspire provides standardized patterns for cloud-native .NET applications.

**Created Projects:**

**1. Obsidian.ServiceDefaults** — Shared configuration library
- `AddServiceDefaults()` extension method for `IHostApplicationBuilder`
- `MapDefaultEndpoints()` extension method for `WebApplication`
- OpenTelemetry logging, metrics, and tracing
- Health check endpoints: `/health` and `/alive`
- HTTP client resilience with retry policies
- Service discovery support

**2. Obsidian.AppHost** — Distributed application orchestrator
- Orchestrates `Obsidian.Api` backend service
- Configures SQLite connection string via parameter
- `Obsidian.Web` (Blazor WASM) NOT orchestrated — runs separately in browser

**Key Decisions:**
- NuGet packages instead of deprecated workload
- WASM not orchestrated (client-side only)
- SQLite parameter-based (no managed database resource)

**Consequences:** Standardized telemetry, health checks, resilience, service discovery, cloud-ready OTLP export.

---

### ADR-010: Authentication Restricted to Personal Microsoft Accounts

**Date:** 2026-05-28  
**Author:** Neo (Backend Dev)  
**Status:** Implemented  

Changed Microsoft Authentication from multi-tenant (`common`) to personal accounts only (`consumers`).

**Changes:**
- API `appsettings.json`: `"TenantId": "consumers"`
- Web `appsettings.json`: `"Authority": "https://login.microsoftonline.com/consumers"`
- **Critical:** `ValidateIssuer = false` (required for personal accounts — they use different issuers)
- Azure Portal app registration must be set to "Personal Microsoft accounts only"

**Identity Claims for Personal Accounts:**
- Stable identifier: `sub` claim (subject)
- Object ID: `oid` claim
- `AdminOverrideClaimsTransformation` reads `oid` first, falls back to `sub`

**Impact:**
- ✅ Build: 0 errors, 113 tests pass
- ⚠️ Organizational accounts can no longer sign in (intentional)
- Requires Azure portal configuration change

---

### ADR-011: Blazor WASM Aspire Service Discovery Integration

**Date:** 2026-12-29  
**Author:** Trinity (Frontend Developer)  
**Status:** Implemented  

Configured Blazor WASM to use Aspire service discovery via environment variable injection, with graceful fallback to appsettings.json.

**API URL Resolution Order:**
1. Check for Aspire HTTPS endpoint (`services:obsidian-api:https:0`)
2. Check for Aspire HTTP endpoint (`services:obsidian-api:http:0`)
3. Fall back to `ApiBaseUrl` in appsettings.json
4. Final hardcoded fallback

**Why AddServiceDiscovery() is NOT Available:**
- Blazor WASM runs entirely in the browser
- No access to service discovery infrastructure
- Configuration binding via `IConfiguration` is sufficient

**Benefits:**
- ✅ Works in Aspire (auto-discovers API from AppHost)
- ✅ Works standalone (fallback to appsettings.json)
- ✅ No runtime dependencies
- ✅ Standard Aspire pattern for WASM apps

**Constraint:** URLs resolved at startup only (acceptable for WASM).

---

### ADR-012: Neo — Aspire API Integration

**Date:** 2026-05-28  
**Author:** Neo (Backend Dev)  
**Status:** Implemented  

Integrated `Obsidian.Api` with `Obsidian.ServiceDefaults` for observability.

**Implementation:**
```csharp
// Program.cs — after CreateBuilder
builder.AddServiceDefaults();

// Program.cs — middleware pipeline
app.MapDefaultEndpoints();

// Added health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ObsidianDbContext>("database");
```

**What We Get:**
- OpenTelemetry traces, metrics, logs
- Service discovery for Aspire
- Resilience handlers
- Health endpoints: `GET /health`, `GET /alive`

**Why This Way:**
- Minimal code change (2 extension method calls)
- Standard Aspire pattern
- EF Core health check validates database
- No breaking changes (113 tests pass)

---

### ADR-013: Neo → Trinity Auth API Contract

**Date:** 2026-05-28  
**Author:** Neo (Backend Dev)  

All protected endpoints require JWT bearer token: `Authorization: Bearer <access_token>`

**Token Validation:**
- Authority: `{AzureAd:Instance}{AzureAd:TenantId}` (default: `https://login.microsoftonline.com/common`)
- Audience: `AzureAd:Audience` (set to API's ClientId)
- Issuer validation: disabled (multi-tenant support)

**Policies:**
| Policy | Required Role(s) |
|--------|-----------------|
| `RequireUser` | `User`, `Admin`, or `SystemAdmin` |
| `RequireAdmin` | `Admin` or `SystemAdmin` |
| `RequireSystemAdmin` | `SystemAdmin` only |

**AdminController (`/api/admin`) — All endpoints require `RequireSystemAdmin`:**

- `GET /api/admin/users` — Returns all local admin overrides (empty array `[]` if none exist)
- `POST /api/admin/users` — Grants local admin role
  - Request: `{ "objectId", "displayName", "role" }` (`role` must be "Admin" or "SystemAdmin")
  - `GrantedBy` and `GrantedAt` set server-side (not trusted from client)
  - Returns `201 Created` on success, `409 Conflict` if duplicate
- `DELETE /api/admin/users/{objectId}` — Removes override, returns `204 No Content`

**How `AdminOverrideClaimsTransformation` Works:**
1. Reads `oid` claim (falls back to `sub`)
2. Looks up `UserAdminOverrides` in SQLite
3. If found, adds `ClaimTypes.Role` claim for that role

**Frontend Notes:**
- `GrantedBy` in POST will be **overwritten** server-side (use token `oid`/`sub`)
- `401 Unauthorized` / `403 Forbidden` returned for policy failures

---

### ADR-014: ADR-008 Local Kubernetes Deployment Architecture

**Date:** 2026-06-14  
**Author:** Morpheus (Lead / Architect)  
**Status:** Approved — Ready for Implementation  

Full Kubernetes deployment architecture for local clusters (minikube, kind, Docker Desktop).

**Core Decisions:**

**1. Container Strategy — Hand-Crafted Dockerfiles**
- Explicit Dockerfiles (portable, version-controlled)
- Multi-stage builds minimize image size
- AppHost continues for local dev; k8s uses separate manifests

**2. Database — SQLite + PVC (Default)**
- Local k8s = single node, single replica
- PVC ensures data survives pod restarts
- PostgreSQL documented as upgrade path

**3. Manifest Strategy — Helm Chart**
- `helm/obsidian/` at repository root
- `values.yaml` parameterizes image tags, replicas, ingress, env vars
- `helm upgrade --install` is idempotent

**4. AppHost Role — Local Development Only**
- Never deployed to Kubernetes
- Local dev: `dotnet run --project source/Obsidian.AppHost`
- K8s: Separate pods configured via Helm + env vars

**5. Ingress — nginx Ingress Controller**
- Host routing: `obsidian.local` (Web) and `api.obsidian.local` (API)
- WebSocket support for SignalR (3600s timeout annotations)
- User must add to hosts file: `127.0.0.1 obsidian.local api.obsidian.local`

**6. Blazor WASM Container — nginx + Runtime Config Injection**
- Multi-stage: .NET SDK publish → nginx:alpine
- `entrypoint.sh` performs `envsubst` on `appsettings.json` before nginx starts
- Published `appsettings.json` renamed to `.template` with `${API_URL}` placeholder

**File Layout:**
```
helm/obsidian/                          # Helm chart
  Chart.yaml, values.yaml, templates/
  - namespace.yaml, configmap.yaml, secrets.yaml
  - api/, web/, db/ (deployments, services, PVC)
  - ingress.yaml
source/
  Obsidian.Api/Dockerfile               # Multi-stage, non-root user, /data PVC
  Obsidian.Web/Dockerfile               # Multi-stage → nginx:alpine
  Obsidian.Web/nginx.conf               # SPA routing, WASM MIME types, gzip, security headers
  Obsidian.Web/entrypoint.sh            # envsubst API_URL → appsettings.json
scripts/
  deploy-local.ps1                      # Windows PowerShell (minikube/kind/Docker Desktop detection)
  deploy-local.sh                       # bash (Linux/macOS)
```

**Dockerfile Specifications:**

**Obsidian.Api/Dockerfile:**
- Build context: repo root
- Non-root user (appuser)
- `/data` volume for SQLite mount
- Port 8080, ASPNETCORE_URLS=http://+:8080
- No HTTPS (TLS terminates at ingress)

**Obsidian.Web/Dockerfile:**
- Build stage: .NET SDK publish
- Runtime stage: nginx:alpine + gettext (envsubst)
- Copies published WASM to nginx `wwwroot`
- Renames `appsettings.json` to `.template`
- Runs `nginx.conf` + `entrypoint.sh`

**nginx.conf:**
- SPA fallback: `try_files $uri $uri/ /index.html`
- Cache WASM/DLL/static assets: `expires 1y`
- Security headers (X-Frame-Options, X-Content-Type-Options)
- Gzip compression for WASM

**entrypoint.sh:**
```bash
API_URL="${API_URL:-http://api.obsidian.local}"
sed -i "s|\"ApiBaseUrl\": \"[^\"]*\"|\"ApiBaseUrl\": \"${API_URL}/\"|g" \
    /usr/share/nginx/html/appsettings.json
exec nginx -g 'daemon off;'
```

**Helm Chart Details:**

**values.yaml:**
```yaml
namespace: obsidian
api:
  image: obsidian-api:latest (pullPolicy: Never)
  replicas: 1, port: 8080
  env:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__DefaultConnection: "Data Source=/data/obsidian.db"
    AzureAd__Instance: "https://login.microsoftonline.com/"
    AzureAd__TenantId: "consumers"
    AzureAd__Audience: "" (set via --set or secrets)
    AzureAd__ClientId: "" (set via --set or secrets)
    CORS__AllowedOrigins: "http://obsidian.local"
web:
  image: obsidian-web:latest (pullPolicy: Never)
  replicas: 1, port: 80
  env:
    API_URL: "http://api.obsidian.local"
database:
  type: sqlite
  sqlite:
    pvc:
      size: 1Gi
```

**Ingress Annotations (WebSocket support):**
```yaml
nginx.ingress.kubernetes.io/proxy-read-timeout: "3600"
nginx.ingress.kubernetes.io/proxy-send-timeout: "3600"
nginx.ingress.kubernetes.io/proxy-http-version: "1.1"
nginx.ingress.kubernetes.io/upstream-hash-by: "$arg_id"
```

**Deployment Scripts:**

**deploy-local.ps1 / deploy-local.sh:**
1. Preflight checks (kubectl, helm, docker)
2. Detect cluster type (minikube / kind / Docker Desktop)
3. Build images:
   - `docker build -f source/Obsidian.Api/Dockerfile -t obsidian-api:latest .`
   - `docker build -f source/Obsidian.Web/Dockerfile -t obsidian-web:latest .`
4. Load images into cluster
5. Install/upgrade via Helm:
   - `helm upgrade --install obsidian ./helm/obsidian --namespace obsidian --create-namespace --set api.env.AzureAd__ClientId=$ClientId`
6. Wait for rollout
7. Print hosts file instructions

**CORS Configuration Requirement:**

API's `Program.cs` must support configurable origins (prerequisite):
```csharp
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins")
    .Get<string[]>() ?? ["https://localhost:7001", "http://localhost:5002"];
policy.WithOrigins(allowedOrigins)
```

**PostgreSQL Upgrade Path (Future):**
- Add PostgreSQL StatefulSet template
- Update connection string to `Host=obsidian-postgresql;...`
- Make `Program.cs` database provider configurable

---

### User Directive: Authentication — Personal Accounts Only

**Date:** 2026-03-26  
**By:** Michael R. Schmidt (via Copilot)  

Authentication must target **personal Microsoft accounts only** (outlook.com, hotmail.com, live.com) — NOT corporate/organizational Azure AD accounts.

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
