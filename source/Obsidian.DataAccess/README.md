# Obsidian.DataAccess

Entity Framework Core database layer for Obsidian Minecraft Bedrock Server Manager.

## Features

- **Database Provider Abstraction**: Seamlessly switch between SQLite and PostgreSQL
- **Entity Framework Core**: Modern ORM with LINQ support
- **Migrations**: Separate migrations for SQLite and PostgreSQL
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

// SQLite (default)
builder.Services.AddObsidianDbContext(
    connectionString: "Data Source=obsidian.db",
    provider: DatabaseProvider.SQLite
);

// PostgreSQL
builder.Services.AddObsidianDbContext(
    connectionString: "Host=localhost;Database=obsidian;Username=postgres;Password=postgres",
    provider: DatabaseProvider.PostgreSQL
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
# SQLite
cd source/Obsidian.DataAccess
dotnet ef database update

# PostgreSQL
cd source/Obsidian.DataAccess
DB_PROVIDER=PostgreSQL dotnet ef database update
```

### Creating New Migrations

When you modify entities, create new migrations:

```bash
# SQLite
cd source/Obsidian.DataAccess
dotnet ef migrations add YourMigrationName --output-dir Migrations/Sqlite

# PostgreSQL
cd source/Obsidian.DataAccess
DB_PROVIDER=PostgreSQL dotnet ef migrations add YourMigrationName --output-dir Migrations/PostgreSQL
```

## Connection Strings

### SQLite
```
Data Source=obsidian.db
```

### PostgreSQL
```
Host=localhost;Database=obsidian;Username=postgres;Password=postgres
```

For production, store connection strings securely using:
- User Secrets (development)
- Environment Variables (production)
- Azure Key Vault or similar secret management

## Package Dependencies

- `Microsoft.EntityFrameworkCore` (9.0.5)
- `Microsoft.EntityFrameworkCore.Design` (9.0.5)
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.5)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (9.0.4)
