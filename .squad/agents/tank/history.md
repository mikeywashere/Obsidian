# Tank — History

## Core Context

- **Project:** Upgrade the Obsidian Minecraft Bedrock Server manager from .NET 8 to .NET 9, updating all NuGet packages across the Blazor WASM frontend, EF Core data access, console backend, and unit test projects.
- **Role:** Test Engineer
- **Joined:** 2026-03-20T23:20:52.210Z

## Learnings

<!-- Append learnings below -->

### 2026-03-21 - Initial Test Coverage for Server Management

**Task:** Write comprehensive test coverage for Obsidian.Api server management components and controllers.

**What I built:**
- `ServersControllerTests.cs` - 10 tests covering REST API endpoints for server lifecycle management
  - Tested GET endpoints (list, by-id, logs) with OK and 404 responses
  - Tested POST endpoints (start, stop) with success and error conditions
  - Used NSubstitute to mock IServerManager dependencies
- `ServerManagerTests.cs` - 15 tests covering in-memory server management
  - Tested server registration, retrieval, and collection management
  - Tested log retrieval with maxLines parameter
  - Tested error conditions (unknown servers, invalid operations)
  - Verified seeded data and default values

**Test results:**
- Total: 44 tests (includes existing tests from other components)
- New tests: 25 (10 controller + 15 manager)
- All tests PASSED ✅
- Test duration: 52.1s

**Project updates:**
- Added NSubstitute 5.3.0 package reference to Obsidian.UnitTests.csproj
- Added project references: Obsidian.Api, Obsidian.Models

**Testing approach:**
- Used Arrange/Act/Assert pattern with clear separation
- Named tests with MethodName_Scenario_ExpectedResult convention
- Focused on behavior, not implementation
- Covered happy paths, edge cases, and error conditions
- Avoided actual process spawning in ServerManager tests (focused on in-memory state)

**Key insights:**
- ServerManager has a seeded server ("My Bedrock Server") in constructor for demo purposes
- Controller properly returns 404 for not-found scenarios
- Controller catches InvalidOperationException and returns BadRequest
- ServerManager uses ConcurrentDictionary for thread-safe server management
- Log retrieval uses TakeLast() to respect maxLines parameter

### 2026-03-21 - SignalR Real-Time Log Broadcasting Tests

**Task:** Write comprehensive tests for P1 backend SignalR components (ServerLogBroadcaster and related event infrastructure).

**What I built:**
- `ServerLogBroadcasterTests.cs` - 4 tests covering IHostedService lifecycle and event handling
  - StartAsync_SubscribesToLogReceivedEvent - Verifies subscription on startup
  - StopAsync_UnsubscribesFromLogReceivedEvent - Verifies cleanup on shutdown
  - OnLogReceived_SendsToCorrectGroup - Validates group naming (server-{serverId})
  - OnLogReceived_SendsCorrectLogPayload - Confirms log forwarding to SignalR hub
- `ServerManagerLogEventTests.cs` - 3 tests for new IServerManager additions
  - GetInstallPath_ReturnsNullForUnknownServer
  - GetInstallPath_ReturnsPathForRegisteredServer
  - RegisterServer_StoresInstallPath

**Test results:**
- Total: 51 tests (all existing + 7 new)
- New tests: 7 (4 broadcaster + 3 manager)
- All tests PASSED ✅
- Test duration: 27.9s

**Project updates:**
- Added Microsoft.AspNetCore.SignalR 1.1.0 package reference to Obsidian.UnitTests.csproj

**Testing approach:**
- Used NSubstitute event raising with `Raise.Event<EventHandler<ServerLogEventArgs>>()` to test subscriptions
- Mocked SignalR hub infrastructure (IHubContext, IHubClients, IClientProxy)
- Added Task.Delay(100) to wait for fire-and-forget async event handlers
- Used ReceivedWithAnyArgs() for SignalR SendAsync calls due to params object[] signature complexity
- Documented integration test boundary for LogReceived event firing (requires real process)

**Key insights:**
- ServerLogBroadcaster implements IHostedService and subscribes/unsubscribes to LogReceived event
- Event handler uses fire-and-forget pattern (`_ = _hubContext.Clients...`) requiring test delays
- SignalR group naming follows pattern "server-{serverId}" for per-server log isolation
- NSubstitute's Arg matchers don't work well with SignalR's SendAsync extension method due to params array
- GetInstallPath() provides clean access to server installation paths without exposing internal collections
- LogReceived event production-side testing requires integration tests with real BedrockProcess output

### 2026-03-25 - Player Tracking Tests

**Task:** Write comprehensive test coverage for player tracking components (PlayerTracker, PlayersController, PlayerBroadcaster).

**What I built:**
- `PlayerTrackerTests.cs` — 10 tests (9 from Neo's initial commit + 1 added)
  - `GetPlayers_ReturnsEmpty_WhenNoPlayersConnected`
  - `OnConnectLog_AddsPlayer_AndFiresPlayerJoined`
  - `OnConnectLog_WithInfoPrefix_AddsPlayer`
  - `OnDisconnectLog_RemovesPlayer_AndFiresPlayerLeft`
  - `OnDisconnectLog_WithNoExistingRecord_DoesNotThrow`
  - `OnDisconnectLog_DoesNotFirePlayerLeft_WhenNoRecord`
  - `GetPlayers_IsScopedToServerId`
  - `ConnectLog_UpdatesExistingPlayer_IfSameXuid`
  - `UnrelatedLog_IsIgnored`
  - `GetPlayers_ReturnsSnapshot_NotLiveReference` ← Tank added
- `PlayersControllerTests.cs` — 2 tests (written by Neo, matched spec exactly)
  - `GetPlayers_ReturnsOkWithPlayerList`
  - `GetPlayers_ReturnsEmptyList_WhenNoPlayersOnline`
- `PlayerBroadcasterTests.cs` — 9 tests (5 from Neo + 4 added, + 3 bugs fixed)
  - Fixed: `SendCoreAsync` arg mixing bug (literal + `Arg.Any` requires `Arg.Is` for literal)
  - Added: `StartAsync_SubscribesToPlayerJoinedEvent`
  - Added: `StartAsync_SubscribesToPlayerLeftEvent`
  - Added: `PlayerJoined_BroadcastsToCorrectGroup`
  - Added: `PlayerLeft_BroadcastsToCorrectGroup`

**Test results:**
- Total: 72 tests (prior 51 + 21 new player tracking)
- All tests PASSED ✅

**Testing approach:**
- `PlayerTracker` tested synchronously — log events fire inline (no async needed)
- `GetPlayers` returns `ToList()` snapshot → verified immutability against live state
- Regex-based parser tested with both plain and `[INFO]`-prefixed log formats
- `PlayerBroadcaster` uses same fire-and-forget pattern as `ServerLogBroadcaster` → `Task.Delay(100)` needed
- `SendCoreAsync` with mixed literal + matchers must use `Arg.Is<string>(...)` for the literal

**Key insights:**
- NSubstitute: mixing literal args with `Arg.Any` matchers causes "non-bound argument specifications" error — all args must use matchers when any one does (`Arg.Is` for literals)
- `IClientProxy.SendAsync` is an extension method over `SendCoreAsync(string, object[], CancellationToken)` — use `ReceivedWithAnyArgs` OR `Arg.Is` pattern for `SendCoreAsync`
- `PlayerTracker` uses `ConcurrentDictionary<string, ConcurrentDictionary<string, PlayerInfo>>` keyed by serverId then xuid
- `GetPlayers` returns `.ToList()` copy — confirmed by snapshot isolation test
- Disconnect with no prior connect silently no-ops (PlayerLeft not fired)

### 2026-03-25 - RakNetParser Unit Tests

**Task:** Write comprehensive unit tests for the RakNetParser static class added by Neo.

**What I built:**
- `RakNetParserTests.cs` — 28 tests covering all parsing scenarios
  - Packet type identification for all 12 known types (Ping, Pong, OCR1/2, OCReply1/2, NewIncomingConnection, Disconnect, Ack, Nack, GamePacket, Unknown)
  - DataPacket range (0x80–0x8f all map to DataPacket): 3 tests covering boundary + midpoint
  - DataPacket sequence number extraction: 2 tests (normal and large LE sequence numbers)
  - UnconnectedPong MOTD parsing: 5 tests (PlayerCount, MaxPlayers, WorldName, GameMode, ServerMotd)
  - Direction preservation: 2 tests (ClientToServer, ServerToClient)
  - Raw data preservation: 1 test
  - Defensive parsing (never throws): 4 tests (empty array, truncated pong, truncated DataPacket, malformed MOTD)

**Test results:**
- Total: 101 tests (prior 72 + 28 new + 1 existing rounding difference)
- All tests PASSED ✅
- Duration: ~25s

**Testing approach:**
- Used private `BuildPongPacket(string motd)` helper to construct valid 0x1c packets with proper layout: [0]=0x1c [1-8]=sendTime [9-16]=serverGuid [17-32]=magic [33-34]=strLen BE [35+]=MOTD UTF-8
- Used `BinaryPrimitives.WriteInt64BigEndian` and `WriteUInt16BigEndian` for correct byte layout
- Verified defensive parsing by truncating packets mid-stream — parser must never throw
- Added null assertion before `ShouldContain` to satisfy nullable analysis (CS8604 warning)

**Key insights:**
- `RakNetParser.Parse()` catches all exceptions internally — outer `Parse` wraps `ParseInternal` in try/catch
- DataPacket sequence number requires at least 4 bytes; with only 1–3 bytes, `SequenceNumber` is null
- Pong MOTD parsing requires `data.Length >= 35` AND `data.Length >= 35 + motdLength`; truncation yields null fields
- `PacketDirection` enum lives in `UdpProxy.cs` (not `IUdpProxy.cs`) in the `Obsidian` namespace

### 2026-03-26 - RakNetParser Tests Session Complete

**Status:** ✅ COMPLETED  
**Duration:** Async background work + scribe finalization

**Session Summary:**
Completed comprehensive unit test expansion for RakNetParser. All 101 tests passing (28 new + 73 existing). Tests committed and pushed to `copilot/add-database-access-ef-core` branch.

**Artifacts Created:**
- `.squad/orchestration-log/2026-03-26T04-43-00Z-tank.md` — Spawn result summary
- `.squad/log/2026-03-26T04-43-00Z-raknet-tests.md` — Detailed session execution log

**Next Steps:** Awaiting coordinator instructions for subsequent work assignments.

### 2026-03-26 - Auth & Admin Role Management Tests

**Task:** Write auth tests for API endpoints and admin role management.

**What I built:**
- `AdminControllerTests.cs` — 7 tests covering Neo's AdminController
  - `GetAdminUsers_RequiresSystemAdminRole` — reflection check: class-level [Authorize(Policy="RequireSystemAdmin")]
  - `GetAdminUsers_ReturnsListFromDb` — in-memory SQLite: populates overrides, calls GetUsers(), asserts returned list
  - `GrantAdmin_RequiresSystemAdminRole` — reflection check: class-level [Authorize] covers all endpoints
  - `GrantAdmin_AddsOverrideToDb` — calls AddUser() with valid body, verifies override persisted + GrantedBy set
  - `RevokeAdmin_RequiresSystemAdminRole` — reflection check (same class attr)
  - `RevokeAdmin_RemovesOverrideFromDb` — seeds override, calls DeleteUser(), verifies null after removal
  - `RevokeAdmin_Returns404_WhenNotFound` — calls DeleteUser("nonexistent"), asserts NotFoundObjectResult

- `AdminOverrideClaimsTransformationTests.cs` — 4 tests covering Neo's AdminOverrideClaimsTransformation
  - `TransformAsync_AddsRoleClaim_WhenUserHasAdminOverride` — override with Role="Admin", asserts IsInRole("Admin")
  - `TransformAsync_AddsSystemAdminClaim_WhenUserHasSystemAdminOverride` — override with Role="SystemAdmin"
  - `TransformAsync_DoesNotAddClaim_WhenNoOverrideExists` — no DB entry → Assert.Same(principal, result)
  - `TransformAsync_ReturnsPrincipalUnchanged_WhenNoOidClaim` — no "oid"/"sub" → Assert.Same(principal, result)

- `AuthorizationTests.cs` — +1 test (only missing test from existing suite)
  - `SystemAdminPolicy_ShouldDenyUser` — verifies SystemAdmin policy rejects User role

**Test results:**
- Total: 113 tests (101 prior + 12 new)
- All tests PASSED ✅

**Testing approach:**
- Used SQLite in-memory with `SqliteConnection("DataSource=:memory:")` + `EnsureCreated()` for controller and transformation tests — consistent with DataAccessTests.cs pattern
- Used `IDisposable` pattern for connection/context cleanup per test class
- Used `DefaultHttpContext` + `ControllerContext` to inject User claims into controller (no NSubstitute needed for HttpContext)
- Authorization attribute tests use `GetCustomAttributes(typeof(AuthorizeAttribute), true)` reflection — verifies class-level [Authorize] covers all endpoints
- `Assert.Same` to verify principal reference unchanged when no-op path taken

**Key insights:**
- Neo's AdminController uses ObsidianDbContext directly (no IAdminService interface) — test against DbContext, not a mock
- IAdminService.cs stub WAS previously committed on this branch (Trinity added it); restored via `git checkout HEAD` after accidentally deleting it
- Neo already grouped my test files into his feat(api) commit — commit fceae6f
- Controller method names differ from task spec: `GetUsers`, `AddUser`, `DeleteUser` (not `GetAdminUsers`/`GrantAdmin`/`RevokeAdmin`)
- `DeleteUser` returns 204 NoContent (not 200 Ok) on success — test uses `NoContentResult`
- AdminOverrideClaimsTransformation uses short claim name "oid" (not full URL), falls back to "sub"
- Roles.cs and Policies.cs duplicated in Obsidian.Models.Authorization (for API use) vs Obsidian.Web.Authorization (for Blazor WASM)
- 5 of the 6 requested AuthorizationTests extensions already existed with "Should" prefix naming convention

