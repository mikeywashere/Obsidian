# Morpheus — History

## Core Context

- **Project:** Obsidian — .NET backend service to control Minecraft Bedrock servers with a Blazor WASM frontend
- **Role:** Lead / Architect
- **Joined:** 2026-03-20T23:20:52.200Z

## Project Sessions

### 2026-03-21 — Architecture Analysis & Planning

**Task:** Analyzed entire codebase to design the system architecture for Obsidian.

**Key Findings:**
1. **No API layer exists** — Blazor WASM uses `MockServerService` directly
2. **`IServerService` interface is already defined** — good contract for servers
3. **`ServerProperties` model is complete** — 40+ properties with Load/Save
4. **`UdpProxy` implementation exists** — UDP packet forwarding for Bedrock
5. **`Bedrock` class scrapes versions** — uses Playwright to get download links
6. **`Host` class (Runner.cs) is incomplete** — just starts "obsidian" process (wrong)
7. **Frontend UI is functional** — Servers, ServerDetail pages with auth policies

**Decision Made:** ADR-001 in `.squad/decisions/inbox/morpheus-obsidian-architecture.md`
- Create new `Obsidian.Api` project (ASP.NET Core Web API)
- Add SignalR for real-time log streaming
- Refactor `Host` → `BedrockProcess` with proper stdout capture
- Replace `MockServerService` with `HttpServerService`

**Work Breakdown Created:** Task assignments for Neo (Backend) and Trinity (Frontend)

### 2026-03-25 — Player Tracking Architecture Decision

**Task:** Design architecture for tracking connected players and positions in Minecraft Bedrock worlds.

**Request:** Michael wanted to intercept traffic to track players. Suggested using YARP.

**Analysis:**
1. **YARP not applicable** — YARP is HTTP reverse proxy; Bedrock uses RakNet over UDP
2. **UDP inspection blocked by encryption** — MCPE encrypts after login handshake with AES-256-CFB8
3. **Log parsing is viable** — Server stdout contains player connect/disconnect/spawn messages
4. **Already have infrastructure** — `ServerManager.LogReceived` event provides data stream

**Decision Made:** ADR-004 in `.squad/decisions/inbox/morpheus-player-tracking-architecture.md`

**Recommendation: Approach A (Log-Based Player Tracking)**
- Parse server stdout for player events via regex
- Simple (~50 lines), reliable, no encryption concerns
- Position data on spawn only (not real-time movement)
- 90% value at 5% complexity vs UDP packet inspection

**Key Findings:**
- Bedrock logs: `Player connected: Steve, xuid: 1234567890123456`
- Bedrock logs: `Player Spawned: Steve xance: 123.5, yance: 64.0, zance: -456.2`
- UDP position packets are encrypted — would need full ECDH key exchange + AES implementation
- RCON not available on Bedrock (unlike Java Edition)

**Work Breakdown:**
- **Neo:** PlayerTracker service, PlayerInfo model, /api/servers/{id}/players endpoint, SignalR events
- **Trinity:** PlayerPanel.razor component, wire SignalR player events
- **Tank:** Unit tests for log parsing regex, integration tests for API

**Architecture:**
- `IPlayerTracker` service subscribes to `IServerManager.LogReceived`
- Maintains `ConcurrentDictionary<serverId, Dictionary<xuid, PlayerInfo>>`
- Fires `PlayerStateChanged` events for SignalR broadcast
- UdpProxy kept separate — not needed for log-based tracking

### 2026-03-26 — .NET Aspire AppHost and ServiceDefaults

**Task:** Add .NET Aspire orchestration projects to the Obsidian solution.

**Implementation:**
- Created `Obsidian.ServiceDefaults` at `source\Obsidian.ServiceDefaults\`
  - Provides `AddServiceDefaults()` and `MapDefaultEndpoints()` extensions
  - OpenTelemetry logging, metrics, tracing with OTLP exporter support
  - Health checks at `/health` and `/alive` endpoints
  - HTTP client resilience and service discovery
  - Targets .NET 10.0
- Created `Obsidian.AppHost` at `source\Obsidian.AppHost\`
  - Orchestrates Obsidian.Api with SQLite connection string parameter
  - Does NOT orchestrate Obsidian.Web (pure WASM client-side app)
  - Configured with `appsettings.json` for connection strings
  - Targets .NET 10.0
- Both projects use NuGet packages (Aspire workload is deprecated in .NET 10)
- Both projects added to `Obsidian.sln`
- Build succeeds with warnings (OpenTelemetry.Api 1.10.0 vulnerability — can be upgraded later)

**Decision Made:** ADR in `.squad/decisions/inbox/morpheus-aspire-structure.md`

**Key Notes:**
- Obsidian.Web is pure Blazor WebAssembly — runs entirely in browser, not orchestrated by AppHost
- For local dev, `dotnet run` in Obsidian.Web separately; it calls API via configured base URL
- SQLite connection string configured as parameter resource (no native Aspire integration)
- Removed `IsAspireHost` property to avoid deprecated workload warning

## Learnings

<!-- Append learnings below -->
- The `bedrock_server.exe` is the actual Minecraft Bedrock server executable (not "obsidian")
- `ServerProperties` class has Load/Save methods that parse/write `.properties` files
- `ServerInfo.Status` enum: Stopped, Starting, Running, Stopping, Error
- `ServerLog.Level` enum: Debug, Info, Warning, Error
- Authorization uses Azure AD MSAL with roles: SystemAdmin, Admin, User
- Aspire AppHost orchestrates backend services only; Blazor WASM configured separately via env vars
- Personal Microsoft accounts (consumers tenant) require ValidateIssuer = false for JWT token validation
- ServiceDefaults project provides OpenTelemetry and health check infrastructure for all backend services
