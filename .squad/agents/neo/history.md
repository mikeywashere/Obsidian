# Neo — History

## Core Context

- **Project:** Upgrade the Obsidian Minecraft Bedrock Server manager from .NET 8 to .NET 9, updating all NuGet packages across the Blazor WASM frontend, EF Core data access, console backend, and unit test projects.
- **Role:** Backend Dev
- **Joined:** 2026-03-20T23:20:52.203Z

## Learnings

### 2026-03-20 - Created Obsidian.Api ASP.NET Core Web API

Successfully implemented the backend REST API for the Obsidian Bedrock server manager:

**Project Structure:**
- Created `Obsidian.Api` project targeting .NET 9
- Added dependencies: Microsoft.AspNetCore.OpenApi, Microsoft.AspNetCore.SignalR, Swashbuckle.AspNetCore
- Added project references to Obsidian.Models and Obsidian (core library)
- Successfully added to Obsidian.sln

**Services Layer:**
- Implemented `IServerManager` interface with async methods for server lifecycle management
- Created `ServerManager` singleton service using ConcurrentDictionary for thread-safe server state management
- Server process management: Launches bedrock_server.exe (Windows) or bedrock_server (Linux) with full stdout/stderr capture
- Graceful shutdown: Sends "stop" command to stdin with 10-second timeout before forced kill
- In-memory log buffer with configurable max lines
- Seeded with demo server (id: "server-1", name: "My Bedrock Server", path: "C:\bedrock")

**API Controllers:**
- `ServersController`: GET /api/servers, GET /api/servers/{id}, GET /api/servers/{id}/logs, POST /api/servers/{id}/start, POST /api/servers/{id}/stop
- `PropertiesController`: GET/PUT /api/servers/{id}/properties for server.properties file editing

**Technical Challenges:**
- Changed `ServerProperties` class from `internal` to `public` in Obsidian\ServerProperties.cs for API access
- Resolved namespace collision between `Microsoft.Extensions.Logging.LogLevel` and `Obsidian.Models.LogLevel` by using fully-qualified type name `Models.LogLevel`

### 2026-03-25: Full Solution Build Succeeded

The complete Obsidian solution compiled successfully with 0 errors and 0 warnings across all 6 projects:
- Obsidian.Api (backend, newly created)
- Obsidian.Models
- Obsidian (core)
- Obsidian.Web (frontend, updated)
- Obsidian.Models.Tests
- Obsidian.Tests

Trinity's frontend HTTP client wiring complete and integrated. System ready for end-to-end integration testing.

<!-- Append learnings below -->

### 2026-03-25: Implemented SignalR Real-Time Log Streaming (P1)

Added event-driven architecture for broadcasting server logs to connected clients via SignalR:

**Event System:**
- Added `LogReceived` event to `IServerManager` with `ServerLogEventArgs` record
- `ServerManager.OutputDataReceived` and `ErrorDataReceived` now fire events after logging
- Captured `serverId` in local variable to avoid closure issues in async event handlers

**SignalR Hub Infrastructure:**
- Created `ServerLogHub` with `JoinServer`/`LeaveServer` methods for group-based subscriptions
- Created `ServerLogBroadcaster` hosted service to bridge `IServerManager.LogReceived` → SignalR clients
- Group naming: `server-{serverId}` for targeted broadcasting
- SignalR method: `ReceiveLog` sends `ServerLog` to subscribed clients

**API Improvements:**
- Added `GetInstallPath(string serverId)` to `IServerManager` interface
- Fixed `PropertiesController` hardcoded install path — now uses `_serverManager.GetInstallPath()`
- Updated CORS policy: `WithOrigins("https://localhost:7001", "http://localhost:5002")` with `.AllowCredentials()` (required for SignalR)
- Registered `ServerLogBroadcaster` as hosted service in `Program.cs`
- Mapped hub endpoint: `/hubs/serverlogs`

**Technical Notes:**
- SignalR requires `AllowCredentials()` which conflicts with `AllowAnyOrigin()` — must specify exact origins
- Hosted service pattern ensures event subscription/cleanup on app start/stop
- Fire-and-forget pattern (`_ = _hubContext.Clients...`) used in broadcaster to avoid blocking
- Build succeeded with 0 errors/warnings

**Files Modified:**
- `IServerManager.cs` — Added event + `GetInstallPath` method
- `ServerManager.cs` — Implemented event firing + `GetInstallPath`
- `PropertiesController.cs` — Removed hardcoded path
- `Program.cs` — CORS fix, broadcaster registration, hub mapping

**Files Created:**
- `Hubs\ServerLogHub.cs` — SignalR hub for client connections
- `Hubs\ServerLogBroadcaster.cs` — Event bridge to SignalR

### 2026-05-28: Upgraded entire solution to .NET 10

Upgraded all projects from net9.0 to net10.0 and updated all Microsoft.* packages to 10.x equivalents.

**Projects upgraded:**
- `Obsidian` — net10.0, Microsoft.Extensions.Http 10.0.5, Microsoft.Playwright 1.58.0 (removed System.IO.Pipelines — now in BCL)
- `Obsidian.Api` — net10.0, Microsoft.AspNetCore.OpenApi 10.0.5
- `Obsidian.DataAccess` — net10.0, EF Core 10.0.5, EF Core Sqlite 10.0.5, Npgsql 10.0.1
- `Obsidian.Models` — net10.0 (no package changes)
- `Obsidian.UnitTests` — net10.0, coverlet.collector 8.0.1, Auth 10.0.5, Http 10.0.5, TestSdk 18.3.0, xunit.runner 3.1.5
- `Obsidian.Web` — net10.0, all Blazor WASM + SignalR.Client + MSAL packages to 10.0.5

**CI workflow (dotnet.yml):**
- `dotnet-version: 9.0.x` → `10.0.x`
- Playwright install path: `net9.0` → `net10.0`

**Result:** `dotnet build` succeeded (0 errors). All 51 tests passed.

### 2026-05-28: Implemented Player Tracking — Backend

Added log-based player tracking using Minecraft Bedrock server stdout event parsing.

**Architecture:**
- Log-based (NOT UDP packet inspection — Bedrock encrypts after login)
- Singleton `PlayerTracker` subscribes to `IServerManager.LogReceived` at construction
- `ConcurrentDictionary<string, ConcurrentDictionary<string, PlayerInfo>>` — outer key: serverId, inner key: xuid

**New Files:**
- `Obsidian.Models/PlayerInfo.cs` — `record PlayerInfo(ServerId, Name, Xuid, JoinedAt, LastSeen)`
- `Obsidian.Api/Services/IPlayerTracker.cs` — interface with `PlayerJoined`/`PlayerLeft` events and `GetPlayers(serverId)`
- `Obsidian.Api/Services/PlayerTracker.cs` — regex parsing of connect/disconnect log lines (handles optional `[INFO]` prefix)
- `Obsidian.Api/Hubs/PlayerHub.cs` — SignalR hub with `JoinServer`/`LeaveServer` group methods
- `Obsidian.Api/Hubs/PlayerBroadcaster.cs` — `IHostedService` bridging tracker events → `IHubContext<PlayerHub>`
- `Obsidian.Api/Controllers/PlayersController.cs` — `GET /api/servers/{serverId}/players`
- `Obsidian.UnitTests/PlayerTrackerTests.cs` — 8 unit tests (connect, disconnect, INFO prefix, cross-server isolation, edge cases)
- `Obsidian.UnitTests/PlayerBroadcasterTests.cs` — 5 unit tests (subscribe/unsubscribe, group routing, method names)

**SignalR Contract:**
- Hub URL: `/hubs/players`
- Client → Server: `JoinServer(serverId)`, `LeaveServer(serverId)`
- Server → Client: `PlayerJoined(PlayerInfo)`, `PlayerLeft(PlayerInfo)`
- Groups: `server-{serverId}`

**REST Contract:**
- `GET /api/servers/{serverId}/players` → `200 OK` with `PlayerInfo[]`

**Regex patterns:**
- Connect: `(?:\[INFO\]\s+)?Player connected:\s+(?<name>[^,]+),\s+xuid:\s+(?<xuid>\d+)`
- Disconnect: `(?:\[INFO\]\s+)?Player disconnected:\s+(?<name>[^,]+),\s+xuid:\s+(?<xuid>\d+)`

**DI Registration (Program.cs):**
- `AddSingleton<IPlayerTracker, PlayerTracker>()`
- `AddHostedService<PlayerBroadcaster>()`
- `MapHub<PlayerHub>("/hubs/players")`

**Result:** Build 0 errors. All 68 tests pass (51 existing + 13 new + 4 PlayersController).

