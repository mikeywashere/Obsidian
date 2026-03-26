# Architecture Planning Session Summary

**Timestamp:** 2026-03-25T19:04:13Z

## Decision: Obsidian System Architecture (ADR-001)

### What Was Decided

The team approved a three-layer architecture:

1. **Obsidian.Api** (NEW) — ASP.NET Core Web API exposing ServersController, PropertiesController, and ServerLogHub (SignalR)
2. **Obsidian** (Core) — Refactored with BedrockProcess managing server lifecycle and stdout streaming
3. **Obsidian.Web** (Frontend) — Updated with HttpServerService to call Obsidian.Api instead of MockServerService

### Key Technical Choices

- Process management via System.Diagnostics.Process with stdout redirection
- Real-time logs via SignalR hub grouped by serverId
- RESTful API with 8 endpoints for server lifecycle and properties management

### Next Steps

- Neo: Build Obsidian.Api and ServerManager
- Trinity: Wire HttpServerService and DI in Blazor frontend
