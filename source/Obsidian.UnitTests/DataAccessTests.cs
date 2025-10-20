using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using Obsidian.Models;
using Shouldly;

namespace Obsidian.UnitTests;

public class DataAccessTests
{
    [Fact]
    public void DbContext_CanBeInstantiated()
    {
        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new ObsidianDbContext(options);
        
        context.ShouldNotBeNull();
        context.Servers.ShouldNotBeNull();
        context.ServerLogs.ShouldNotBeNull();
    }

    [Fact]
    public void DbContext_CanCreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new ObsidianDbContext(options);
        context.Database.EnsureCreated();
        
        context.Database.CanConnect().ShouldBeTrue();
    }

    [Fact]
    public void DbContext_CanAddAndQueryServerInfo()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new ObsidianDbContext(options);
        context.Database.EnsureCreated();

        var server = new ServerInfo
        {
            Id = "test-server-1",
            Name = "Test Server",
            Status = ServerStatus.Running,
            Version = "1.20.51",
            Port = 19132,
            MaxPlayers = 10,
            CurrentPlayers = 5,
            CreatedDate = DateTime.Now,
            LastStarted = DateTime.Now
        };

        context.Servers.Add(server);
        context.SaveChanges();

        var retrievedServer = context.Servers.FirstOrDefault(s => s.Id == "test-server-1");
        retrievedServer.ShouldNotBeNull();
        retrievedServer.Name.ShouldBe("Test Server");
        retrievedServer.Status.ShouldBe(ServerStatus.Running);
    }

    [Fact]
    public void DbContext_CanAddAndQueryServerLog()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ObsidianDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new ObsidianDbContext(options);
        context.Database.EnsureCreated();

        var log = new ServerLog
        {
            Timestamp = DateTime.Now,
            Level = LogLevel.Info,
            Message = "Server started successfully"
        };

        context.ServerLogs.Add(log);
        context.SaveChanges();

        var retrievedLog = context.ServerLogs.FirstOrDefault(l => l.Message == "Server started successfully");
        retrievedLog.ShouldNotBeNull();
        retrievedLog.Level.ShouldBe(LogLevel.Info);
    }
}
