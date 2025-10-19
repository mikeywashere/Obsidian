# Obsidian.DataAccess - Usage Examples

## Example 1: Console Application

```csharp
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using Obsidian.DataAccess.Entities;

// Create DbContext with SQLite
var optionsBuilder = new DbContextOptionsBuilder<ObsidianDbContext>();
optionsBuilder.UseSqlite("Data Source=obsidian.db");
using var context = new ObsidianDbContext(optionsBuilder.Options);

// Apply migrations
await context.Database.MigrateAsync();

// Add a new server
var server = new ServerInfo
{
    Id = Guid.NewGuid().ToString(),
    Name = "My Bedrock Server",
    Status = ServerStatus.Stopped,
    Version = "1.20.0",
    Port = 19132,
    MaxPlayers = 10,
    CurrentPlayers = 0,
    CreatedDate = DateTime.UtcNow
};

context.Servers.Add(server);
await context.SaveChangesAsync();

Console.WriteLine($"Created server: {server.Name} ({server.Id})");

// Add a log entry
var log = new ServerLog
{
    ServerId = server.Id,
    Timestamp = DateTime.UtcNow,
    Level = LogLevel.Info,
    Message = "Server initialized"
};

context.ServerLogs.Add(log);
await context.SaveChangesAsync();

// Query servers
var servers = await context.Servers
    .Where(s => s.Status == ServerStatus.Stopped)
    .ToListAsync();

foreach (var s in servers)
{
    Console.WriteLine($"Server: {s.Name}, Status: {s.Status}");
}
```

## Example 2: ASP.NET Core Web API

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("ObsidianDb") 
    ?? "Data Source=obsidian.db";
var provider = builder.Configuration.GetValue<string>("DatabaseProvider") == "PostgreSQL"
    ? DatabaseProvider.PostgreSQL
    : DatabaseProvider.SQLite;

builder.Services.AddObsidianDbContext(connectionString, provider);

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ObsidianDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers();
app.Run();
```

```csharp
// Controllers/ServersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using Obsidian.DataAccess.Entities;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly ObsidianDbContext _context;

    public ServersController(ObsidianDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerInfo>>> GetServers()
    {
        return await _context.Servers.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerInfo>> GetServer(string id)
    {
        var server = await _context.Servers.FindAsync(id);
        if (server == null)
        {
            return NotFound();
        }
        return server;
    }

    [HttpPost]
    public async Task<ActionResult<ServerInfo>> CreateServer(ServerInfo server)
    {
        server.Id = Guid.NewGuid().ToString();
        server.CreatedDate = DateTime.UtcNow;
        
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetServer), new { id = server.Id }, server);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServer(string id, ServerInfo server)
    {
        if (id != server.Id)
        {
            return BadRequest();
        }

        _context.Entry(server).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServer(string id)
    {
        var server = await _context.Servers.FindAsync(id);
        if (server == null)
        {
            return NotFound();
        }

        _context.Servers.Remove(server);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<ServerLog>>> GetServerLogs(string id)
    {
        var logs = await _context.ServerLogs
            .Where(l => l.ServerId == id)
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();

        return logs;
    }
}
```

## Example 3: Blazor Server

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure database
builder.Services.AddObsidianDbContext(
    connectionString: "Data Source=obsidian.db",
    provider: DatabaseProvider.SQLite
);

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ObsidianDbContext>();
    db.Database.Migrate();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

```razor
@* Pages/Servers.razor *@
@page "/servers"
@using Microsoft.EntityFrameworkCore
@using Obsidian.DataAccess
@using Obsidian.DataAccess.Entities
@inject ObsidianDbContext DbContext

<h3>Servers</h3>

@if (servers == null)
{
    <p>Loading...</p>
}
else
{
    <table class="table">
        <thead>
            <tr>
                <th>Name</th>
                <th>Status</th>
                <th>Version</th>
                <th>Players</th>
                <th>Port</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var server in servers)
            {
                <tr>
                    <td>@server.Name</td>
                    <td>@server.Status</td>
                    <td>@server.Version</td>
                    <td>@server.CurrentPlayers / @server.MaxPlayers</td>
                    <td>@server.Port</td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private List<ServerInfo>? servers;

    protected override async Task OnInitializedAsync()
    {
        servers = await DbContext.Servers.ToListAsync();
    }
}
```

## Configuration Examples

### appsettings.json (SQLite)
```json
{
  "ConnectionStrings": {
    "ObsidianDb": "Data Source=obsidian.db"
  },
  "DatabaseProvider": "SQLite"
}
```

### appsettings.json (PostgreSQL)
```json
{
  "ConnectionStrings": {
    "ObsidianDb": "Host=localhost;Database=obsidian;Username=postgres;Password=postgres"
  },
  "DatabaseProvider": "PostgreSQL"
}
```

### Environment Variables
```bash
# SQLite
export ConnectionStrings__ObsidianDb="Data Source=obsidian.db"
export DatabaseProvider="SQLite"

# PostgreSQL
export ConnectionStrings__ObsidianDb="Host=localhost;Database=obsidian;Username=postgres;Password=postgres"
export DatabaseProvider="PostgreSQL"
```
