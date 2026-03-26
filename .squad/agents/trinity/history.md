# Trinity — History

## Core Context

- **Project:** Upgrade the Obsidian Minecraft Bedrock Server manager from .NET 8 to .NET 9, updating all NuGet packages across the Blazor WASM frontend, EF Core data access, console backend, and unit test projects.
- **Role:** Frontend Dev
- **Joined:** 2026-03-20T23:20:52.206Z

## Learnings

<!-- Append learnings below -->

### 2026-03-25: Frontend HTTP Client Layer Complete

**Created HTTP Services:**
- `HttpServerService` implements `IServerService` for server CRUD/control operations
- `IServerPropertiesService` + `HttpServerPropertiesService` for server.properties management
- All services use `HttpClient` with JSON serialization via `GetFromJsonAsync` / `PutAsJsonAsync`

**Configuration:**
- API base URL configured via `appsettings.json` with fallback to `https://localhost:5001/`
- Registered dedicated API `HttpClient` in DI container (separate from WASM base client)
- Added SignalR Client package (v9.0.14) for future real-time features

**Architecture Decision - ServerProperties in Models:**
- Copied `ServerProperties` from console Obsidian project to `Obsidian.Models` project
- Updated namespace from `Obsidian` to `Obsidian.Models`
- Removed XML documentation comments per project style guide
- This allows sharing the model between frontend (Blazor WASM) and backend (API)
- Console Obsidian project retains its own copy to avoid executable reference issues

**Services Registered:**
- Replaced `MockServerService` with production `HttpServerService`

### 2026-03-25: Full Solution Build Succeeded

The complete Obsidian solution compiled successfully with 0 errors and 0 warnings across all 6 projects:
- Obsidian.Api (backend, created by Neo)
- Obsidian.Models
- Obsidian (core)
- Obsidian.Web (frontend, updated)
- Obsidian.Models.Tests
- Obsidian.Tests

Frontend HTTP services integrated with Neo's backend API. System ready for end-to-end integration testing and real-time log streaming via SignalR.

### 2026-07-14: PlayerPanel Component with SignalR Player Tracking

**New Files Created:**
- `source/Obsidian.Models/PlayerInfo.cs` — shared `record PlayerInfo(ServerId, Name, Xuid, JoinedAt, LastSeen)`
- `source/Obsidian.Web/Services/IServerPlayerService.cs` — interface with `GetPlayersAsync(serverId)`
- `source/Obsidian.Web/Services/HttpServerPlayerService.cs` — HTTP implementation calling `GET /api/servers/{serverId}/players`
- `source/Obsidian.Web/Components/PlayerPanel.razor` — self-contained component with full SignalR lifecycle

**PlayerPanel Features:**
- Loads initial player list via `IServerPlayerService.GetPlayersAsync` on init
- Connects to `{ApiBaseUrl}/hubs/players`, calls `JoinServer(serverId)` on connect
- Listens to `PlayerJoined` / `PlayerLeft` hub events; deduplicates by `Xuid` on join
- Shows player count badge ("N players online"), player names with relative join times
- Loading state while initial fetch in progress; empty state with em-dash when no players
- Connection indicator dot matching ServerDetail's log hub pattern (Live / Disconnected)
- Implements `IAsyncDisposable` — calls `LeaveServer` and disposes hub on teardown

**Integration:**
- Embedded `<PlayerPanel ServerId="@ServerId" />` in `ServerDetail.razor` between info panel and logs
- Registered `IServerPlayerService` → `HttpServerPlayerService` in `Program.cs`
- `Obsidian.Models` project reference already existed in `Obsidian.Web.csproj`

**Build:** Solution builds successfully with 0 errors (6 pre-existing warnings, none new).



**SignalR Integration in ServerDetail.razor:**
- Added real-time log streaming via SignalR hub at `{apiBaseUrl}/hubs/serverlogs`
- Implemented `IAsyncDisposable` for proper hub cleanup
- Hub connection uses `JoinServer(serverId)` on connect and `LeaveServer(serverId)` on dispose
- Logs pre-populate from REST API (`GetServerLogsAsync`) for history, then SignalR appends live updates
- Added connection indicator UI showing "Live" (green) / "Disconnected" (gray) status
- Removed "Refresh Logs" button — SignalR makes manual refresh obsolete
- Hub configured with automatic reconnection for resilience

**ServerPropertiesEditor Component Created:**
- New component at `Components/ServerPropertiesEditor.razor` for editing server.properties
- Uses Blazor `EditForm` with data binding to `ServerProperties` model
- Form sections: Basic Settings (name, game mode, difficulty, max players), Server Options (online mode, cheats, allowlist, port), World Settings (view distance, level name/seed)
- Includes loading states, error handling, and success/error feedback on save
- Calls `IServerPropertiesService.SavePropertiesAsync` to persist changes

**Configuration & DI:**
- Added `@using Microsoft.AspNetCore.SignalR.Client` to `_Imports.razor` for global SignalR access
- SignalR client created directly in component (standard Blazor WASM pattern, not via DI)
- API base URL read from `IConfiguration["ApiBaseUrl"]` with fallback to `https://localhost:5001/`

**Build Status:**
- Obsidian.Web project builds successfully with 0 errors, 0 warnings
- All SignalR and properties editor features compile cleanly

