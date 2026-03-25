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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
