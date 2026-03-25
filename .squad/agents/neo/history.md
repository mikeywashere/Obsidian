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

