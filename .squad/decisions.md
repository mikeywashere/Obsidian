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

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
