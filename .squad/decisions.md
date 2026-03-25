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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
