var builder = DistributedApplication.CreateBuilder(args);

// Database — SQLite doesn't have a native Aspire integration; use a parameter resource
// The API manages its own SQLite file; pass the connection string as a parameter
var connectionString = builder.AddConnectionString("obsidian-db");

// Backend API
var api = builder.AddProject<Projects.Obsidian_Api>("obsidian-api")
    .WithReference(connectionString);

// Frontend — Blazor WebAssembly
// Note: Obsidian.Web is a pure client-side WASM app that runs entirely in the browser.
// It cannot be orchestrated as a traditional server project. The API can serve the static
// WASM files, or the WASM app can be deployed separately to a CDN/static host.
// For local development, run `dotnet run` in Obsidian.Web separately on its own port.

builder.Build().Run();
