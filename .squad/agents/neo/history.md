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

**TODO:**
- PropertiesController currently hardcodes server install path - should expose this through IServerManager
- Consider implementing SignalR hub for real-time log streaming
- Add server registration endpoint (currently only in-memory demo server)

<!-- Append learnings below -->
