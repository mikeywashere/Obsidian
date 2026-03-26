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

## Learnings

<!-- Append learnings below -->
