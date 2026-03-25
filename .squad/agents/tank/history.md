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
