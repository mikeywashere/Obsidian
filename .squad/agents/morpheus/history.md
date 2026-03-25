# Morpheus — History

## Core Context

- **Project:** Obsidian — .NET backend service to control Minecraft Bedrock servers with a Blazor WASM frontend
- **Role:** Lead / Architect
- **Joined:** 2026-03-20T23:20:52.200Z

## Project Sessions

### 2026-03-21 — Architecture Analysis & Planning

**Task:** Analyzed entire codebase to design the system architecture for Obsidian.

**Key Findings:**
1. **No API layer exists** — Blazor WASM uses `MockServerService` directly
2. **`IServerService` interface is already defined** — good contract for servers
3. **`ServerProperties` model is complete** — 40+ properties with Load/Save
4. **`UdpProxy` implementation exists** — UDP packet forwarding for Bedrock
5. **`Bedrock` class scrapes versions** — uses Playwright to get download links
6. **`Host` class (Runner.cs) is incomplete** — just starts "obsidian" process (wrong)
7. **Frontend UI is functional** — Servers, ServerDetail pages with auth policies

**Decision Made:** ADR-001 in `.squad/decisions/inbox/morpheus-obsidian-architecture.md`
- Create new `Obsidian.Api` project (ASP.NET Core Web API)
- Add SignalR for real-time log streaming
- Refactor `Host` → `BedrockProcess` with proper stdout capture
- Replace `MockServerService` with `HttpServerService`

**Work Breakdown Created:** Task assignments for Neo (Backend) and Trinity (Frontend)

## Learnings

<!-- Append learnings below -->
- The `bedrock_server.exe` is the actual Minecraft Bedrock server executable (not "obsidian")
- `ServerProperties` class has Load/Save methods that parse/write `.properties` files
- `ServerInfo.Status` enum: Stopped, Starting, Running, Stopping, Error
- `ServerLog.Level` enum: Debug, Info, Warning, Error
- Authorization uses Azure AD MSAL with roles: SystemAdmin, Admin, User
