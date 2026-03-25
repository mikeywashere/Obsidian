# Trinity — History

## Core Context

- **Project:** Upgrade the Obsidian Minecraft Bedrock Server manager from .NET 8 to .NET 9, updating all NuGet packages across the Blazor WASM frontend, EF Core data access, console backend, and unit test projects.
- **Role:** Frontend Dev
- **Joined:** 2026-03-20T23:20:52.206Z

## Learnings

<!-- Append learnings below -->

### 2026-03-20: Frontend HTTP Client Layer Complete

**Created HTTP Services:**
- `HttpServerService` implements `IServerService` for server CRUD/control operations
- `IServerPropertiesService` + `HttpServerPropertiesService` for server.properties management
- All services use `HttpClient` with JSON serialization via `GetFromJsonAsync` / `PutAsJsonAsync`

**Configuration:**
- API base URL configured via `appsettings.json` with fallback to `https://localhost:5001/`
- Registered dedicated API `HttpClient` in DI container (separate from WASM base client)
- Added SignalR Client package (v9.0.14) for future real-time features
- Added project reference to core `Obsidian` project for `ServerProperties` model access

**Architecture:**
- Replaced `MockServerService` with production `HttpServerService`
- All MSAL auth and authorization policies preserved unchanged
- HTTP services ready for Neo's API endpoints at `/api/servers/*`
