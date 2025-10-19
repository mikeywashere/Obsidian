using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using Obsidian.DataAccess.Entities;
using Shouldly;

namespace Obsidian.UnitTests;

public class DataAccessTests
{
    private static ObsidianDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ObsidianDbContext(options);
    }

    [Fact]
    public async Task CanAddAndRetrieveServerInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var server = new ServerInfo
        {
            Id = "test-server-1",
            Name = "Test Server",
            Status = ServerStatus.Running,
            Version = "1.20.0",
            Port = 19132,
            MaxPlayers = 10,
            CurrentPlayers = 5,
            CreatedDate = DateTime.UtcNow,
            LastStarted = DateTime.UtcNow
        };

        // Act
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        // Assert
        var retrievedServer = await context.Servers.FindAsync("test-server-1");
        retrievedServer.ShouldNotBeNull();
        retrievedServer.Name.ShouldBe("Test Server");
        retrievedServer.Status.ShouldBe(ServerStatus.Running);
        retrievedServer.Port.ShouldBe(19132);
    }

    [Fact]
    public async Task CanAddAndRetrieveServerLog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var log = new ServerLog
        {
            ServerId = "test-server-1",
            Timestamp = DateTime.UtcNow,
            Level = LogLevel.Info,
            Message = "Server started successfully"
        };

        // Act
        context.ServerLogs.Add(log);
        await context.SaveChangesAsync();

        // Assert
        var retrievedLog = await context.ServerLogs.FirstOrDefaultAsync();
        retrievedLog.ShouldNotBeNull();
        retrievedLog.ServerId.ShouldBe("test-server-1");
        retrievedLog.Level.ShouldBe(LogLevel.Info);
        retrievedLog.Message.ShouldBe("Server started successfully");
    }

    [Fact]
    public async Task CanQueryServerLogsByServerId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var serverId = "test-server-1";
        
        context.ServerLogs.AddRange(
            new ServerLog { ServerId = serverId, Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Log 1" },
            new ServerLog { ServerId = serverId, Timestamp = DateTime.UtcNow, Level = LogLevel.Warning, Message = "Log 2" },
            new ServerLog { ServerId = "other-server", Timestamp = DateTime.UtcNow, Level = LogLevel.Error, Message = "Log 3" }
        );
        await context.SaveChangesAsync();

        // Act
        var logs = await context.ServerLogs
            .Where(l => l.ServerId == serverId)
            .ToListAsync();

        // Assert
        logs.Count.ShouldBe(2);
        logs.All(l => l.ServerId == serverId).ShouldBeTrue();
    }

    [Fact]
    public async Task CanUpdateServerInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var server = new ServerInfo
        {
            Id = "test-server-1",
            Name = "Test Server",
            Status = ServerStatus.Stopped,
            Version = "1.20.0",
            Port = 19132,
            MaxPlayers = 10,
            CurrentPlayers = 0,
            CreatedDate = DateTime.UtcNow
        };
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        // Act
        server.Status = ServerStatus.Running;
        server.CurrentPlayers = 5;
        server.LastStarted = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        var retrievedServer = await context.Servers.FindAsync("test-server-1");
        retrievedServer.ShouldNotBeNull();
        retrievedServer.Status.ShouldBe(ServerStatus.Running);
        retrievedServer.CurrentPlayers.ShouldBe(5);
        retrievedServer.LastStarted.ShouldNotBeNull();
    }

    [Fact]
    public async Task CanDeleteServerInfo()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var server = new ServerInfo
        {
            Id = "test-server-1",
            Name = "Test Server",
            Status = ServerStatus.Stopped,
            Version = "1.20.0",
            Port = 19132,
            MaxPlayers = 10,
            CurrentPlayers = 0,
            CreatedDate = DateTime.UtcNow
        };
        context.Servers.Add(server);
        await context.SaveChangesAsync();

        // Act
        context.Servers.Remove(server);
        await context.SaveChangesAsync();

        // Assert
        var retrievedServer = await context.Servers.FindAsync("test-server-1");
        retrievedServer.ShouldBeNull();
    }
}
