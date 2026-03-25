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
