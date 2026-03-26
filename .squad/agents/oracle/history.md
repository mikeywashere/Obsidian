# Oracle — History

## Core Context

- **Project:** Upgrade the Obsidian Minecraft Bedrock Server manager from .NET 8 to .NET 9, updating all NuGet packages across the Blazor WASM frontend, EF Core data access, console backend, and unit test projects.
- **Role:** DevOps
- **Joined:** 2026-03-20T23:20:52.214Z

## Work Completed

### 2026-03-20: Add .NET Aspire Workload to CI Pipeline
- **Task:** Update GitHub Actions CI for .NET Aspire workload installation
- **Changes Made:**
  - Added `Cache .NET workloads` step to cache `~/.dotnet/workloads` and `~/.dotnet/packs`
  - Added `Install .NET Aspire workload` step to install aspire workload before `dotnet restore`
  - Positioned both new steps after NuGet caching but before dependency restoration
  - Workload cache uses `**/*.csproj` files as key input to invalidate on project changes
- **CI Workflow:** `.github/workflows/dotnet.yml`
- **Commit:** `d0d621b` on branch `copilot/add-database-access-ef-core`
- **Notes:** The existing CI only runs `dotnet test` on unit tests (which don't require AppHost). The build step (`dotnet build`) compiles the AppHost project, which needs the aspire workload. Integration tests requiring AppHost would be a separate concern.

### 2026-03-26: Aspire Workload Caching in CI Pipeline
- **Task:** Prepare GitHub Actions CI for Aspire workload installation needed by AppHost
- **Changes Made:**
  - Added `Cache .NET workloads` step to cache `~/.dotnet/workloads` and `~/.dotnet/packs`
  - Added `Install .NET Aspire workload` step to install aspire workload before `dotnet restore`
  - Positioned both new steps after NuGet caching but before dependency restoration
  - Workload cache uses `**/*.csproj` files as key input to invalidate on project changes
- **Rationale:** Aspire workload is 200MB+ and required for `dotnet build` to compile AppHost; caching avoids redundant downloads on every CI run
- **Scope Boundary:** Unit test CI doesn't need workload (113 tests pass without it); integration test CI will benefit from cached workload
- **Commit:** d0d621b on branch `copilot/add-database-access-ef-core`
- **Result:** ✅ CI pipeline ready for Aspire workload support

## Learnings

<!-- Append learnings below -->
- Aspire workload caching must happen before `dotnet restore` or `dotnet build` to be effective
- Unit test CI and integration test CI have different workload requirements — can optimize separately
- Cache key should include `**/*.csproj` to auto-invalidate when project structure changes
