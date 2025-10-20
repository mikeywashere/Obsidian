# Obsidian.DataAccess

Entity Framework Core database layer for Obsidian Minecraft Bedrock Server Manager.

## Features

- **SQLite Database**: Lightweight, file-based database for easy deployment
- **Entity Framework Core**: Modern ORM with LINQ support
- **Migrations**: SQLite migrations for database schema management
- **Type-safe entities**: Strongly typed models for server information and logs

## Database Entities

### ServerInfo
Stores server configuration and status:
- `Id` - Unique server identifier
- `Name` - Server display name
- `Status` - Current server state (Stopped, Starting, Running, Stopping, Error)
- `Version` - Minecraft Bedrock version
- `Port` - Server port number
- `MaxPlayers` - Maximum player capacity
- `CurrentPlayers` - Current player count
- `CreatedDate` - When the server was created
- `LastStarted` - Last time the server was started

### ServerLog
Stores server log entries:
- `Id` - Auto-incremented log entry ID
- `ServerId` - Reference to the server
- `Timestamp` - When the log entry was created
- `Level` - Log level (Debug, Info, Warning, Error)
- `Message` - Log message content

## Usage

### ASP.NET Core / Blazor Setup

Add the database context to your service collection:

```csharp
using Obsidian.DataAccess;

builder.Services.AddObsidianDbContext(
    connectionString: "Data Source=obsidian.db"
);
```

### Using the DbContext

Inject `ObsidianDbContext` into your services or pages:

```csharp
public class ServerService
{
    private readonly ObsidianDbContext _context;

    public ServerService(ObsidianDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServerInfo>> GetServersAsync()
    {
        return await _context.Servers.ToListAsync();
    }

    public async Task AddServerAsync(ServerInfo server)
    {
        _context.Servers.Add(server);
        await _context.SaveChangesAsync();
    }
}
```

## Migrations

### Running Migrations

To apply migrations to your database:

```bash
cd source/Obsidian.DataAccess
dotnet ef database update
```

### Creating New Migrations

When you modify entities, create new migrations:

```bash
cd source/Obsidian.DataAccess
dotnet ef migrations add YourMigrationName --output-dir Migrations/Sqlite
```

## Connection Strings

### SQLite
```
Data Source=obsidian.db
```

For production, store connection strings securely using:
- User Secrets (development)
- Environment Variables (production)
- Azure Key Vault or similar secret management

## Package Dependencies

- `Microsoft.EntityFrameworkCore` (9.0.5)
- `Microsoft.EntityFrameworkCore.Design` (9.0.5)
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.5)
