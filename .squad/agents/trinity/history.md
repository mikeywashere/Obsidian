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

**Build:** Solution builds 0 errors / 0 new warnings.



### 2026-07-14: Login UI + Admin User Management Page

**What already existed (no changes needed):**
- `Authentication.razor` — full MSAL RemoteAuthenticatorView with UX messages
- `LoginDisplay.razor` — AuthorizeView with role display (updated badges below)
- `RedirectToLogin.razor` — NavigateToLogin helper
- `App.razor` — already wrapped in CascadingAuthenticationState + AuthorizeRouteView
- `MainLayout.razor` — already includes `<LoginDisplay />`
- `Servers.razor` / `ServerDetail.razor` — already have `[Authorize(Policy = Policies.RequireUser)]` + AdminView buttons

**New files created:**
- `source/Obsidian.Web/Services/IAdminService.cs` — `GetAdminUsersAsync / GrantAdminAsync / RevokeAdminAsync`
- `source/Obsidian.Web/Services/HttpAdminService.cs` — HTTP impl calling `/api/admin/users`
- `source/Obsidian.Web/Pages/AdminUsers.razor` — SystemAdmin-only page at `/admin/users`; table with grant form + per-row revoke

**Modified files:**
- `LoginDisplay.razor` — replaced custom CSS role classes with Bootstrap badges (`bg-danger` SystemAdmin, `bg-primary` Admin, `bg-secondary` User)
- `NavMenu.razor` — added Admin nav link wrapped in `<AuthorizeView Policy="@Policies.RequireSystemAdmin">`
- `_Imports.razor` — added `@using Obsidian.Web.Authorization` globally
- `Program.cs` — switched all HTTP services from bare `AddScoped<HttpClient>` to typed MSAL-authorized clients via `AddHttpClient<T>().AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()`
- `Obsidian.Web.csproj` — added `Microsoft.Extensions.Http 10.0.0` (required for `AddHttpClient`)

**Build:** Solution builds with 0 errors.




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

### 2026-12-29: Aspire Service Discovery Integration for Blazor WASM

**Context:**
- Blazor WASM runs in the browser and cannot use runtime service discovery like server-side apps
- Solution: Use Aspire's service discovery via environment variable injection from AppHost at build time
- ServiceDefaults project didn't exist yet at time of implementation (Morpheus creating it)

**Changes Made:**

**Program.cs API URL Resolution:**
- Updated API base URL resolution to check Aspire service discovery env vars first:
  1. `services:obsidian-api:https:0` (Aspire HTTPS endpoint)
  2. `services:obsidian-api:http:0` (Aspire HTTP endpoint)
  3. `ApiBaseUrl` from appsettings.json (standalone fallback)
  4. Final fallback: `https://localhost:5001/`
- Added trailing slash normalization for proper URL resolution

**Configuration Structure:**
- Updated `wwwroot/appsettings.json` to include service discovery pattern:
  ```json
  "services": {
    "obsidian-api": {
      "https": ["https://localhost:5001"],
      "http": ["http://localhost:5000"]
    }
  }
  ```
- Created `wwwroot/appsettings.Development.json` for local override

**WASM Limitations Confirmed:**
- ServiceDefaults reference NOT added (project doesn't exist yet; can be added later)
- `AddServiceDiscovery()` is NOT available in pure WASM context (browser-only runtime)
- ServiceDefaults would only be useful if we add a server-side host component later

**Build Status:**
- Obsidian.Web builds successfully in isolation (0 errors, 0 warnings)
- Full solution build failed due to Obsidian.Api missing ServiceDefaults reference (Neo's concern, not mine)

**How It Works:**
- When running in Aspire AppHost, the host injects service URLs via environment variables
- WASM app reads from `IConfiguration` which merges appsettings.json + environment vars
- Standalone development uses appsettings.json values directly
- All HttpClient registrations use the resolved `apiBaseUrl` variable

