using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class ServerManagerTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSeededServers()
    {
        var manager = new ServerManager();

        var servers = await manager.GetAllAsync();

        Assert.NotEmpty(servers);
        Assert.Contains(servers, s => s.Name == "My Bedrock Server");
    }

    [Fact]
    public async Task GetAsync_ReturnsNullForUnknownId()
    {
        var manager = new ServerManager();

        var server = await manager.GetAsync("unknown-id");

        Assert.Null(server);
    }

    [Fact]
    public void RegisterServer_AddsServerToCollection()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("New Server", "C:\\test", 25565);

        Assert.NotNull(serverInfo);
        Assert.Equal("New Server", serverInfo.Name);
        Assert.Equal(25565, serverInfo.Port);
        Assert.Equal(ServerStatus.Stopped, serverInfo.Status);
        Assert.NotEmpty(serverInfo.Id);
    }

    [Fact]
    public async Task RegisterServer_ServerIsReturnedByGetAll()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("Test Server", "C:\\test", 19133);
        var allServers = await manager.GetAllAsync();

        Assert.Contains(allServers, s => s.Id == serverInfo.Id);
    }

    [Fact]
    public async Task RegisterServer_ServerIsReturnedByGetAsync()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("Test Server", "C:\\test", 19133);
        var retrievedServer = await manager.GetAsync(serverInfo.Id);

        Assert.NotNull(retrievedServer);
        Assert.Equal(serverInfo.Id, retrievedServer.Id);
        Assert.Equal("Test Server", retrievedServer.Name);
    }

    [Fact]
    public async Task GetLogsAsync_ReturnsEmptyForNewServer()
    {
        var manager = new ServerManager();
        var serverInfo = manager.RegisterServer("Test Server", "C:\\test", 19133);

        var logs = await manager.GetLogsAsync(serverInfo.Id);

        Assert.Empty(logs);
    }

    [Fact]
    public async Task GetLogsAsync_ReturnsEmptyForUnknownServer()
    {
        var manager = new ServerManager();

        var logs = await manager.GetLogsAsync("unknown-id");

        Assert.Empty(logs);
    }

    [Fact]
    public async Task GetLogsAsync_RespectsMaxLines()
    {
        var manager = new ServerManager();
        var serverInfo = manager.RegisterServer("Test Server", "C:\\test", 19133);

        var allServers = await manager.GetAllAsync();
        var testServer = allServers.First(s => s.Id == serverInfo.Id);

        var logs = await manager.GetLogsAsync(serverInfo.Id, 5);

        Assert.True(logs.Count() <= 5);
    }

    [Fact]
    public async Task StartAsync_ThrowsForUnknownServer()
    {
        var manager = new ServerManager();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.StartAsync("unknown-id")
        );
    }

    [Fact]
    public async Task StopAsync_ThrowsForUnknownServer()
    {
        var manager = new ServerManager();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.StopAsync("unknown-id")
        );
    }

    [Fact]
    public async Task StopAsync_SetsStatusToStoppedWhenNoProcessExists()
    {
        var manager = new ServerManager();
        var serverInfo = manager.RegisterServer("Test Server", "C:\\test", 19133);

        await manager.StopAsync(serverInfo.Id);
        var updatedServer = await manager.GetAsync(serverInfo.Id);

        Assert.NotNull(updatedServer);
        Assert.Equal(ServerStatus.Stopped, updatedServer.Status);
    }

    [Fact]
    public void RegisterServer_GeneratesUniqueIds()
    {
        var manager = new ServerManager();

        var server1 = manager.RegisterServer("Server 1", "C:\\test1", 19132);
        var server2 = manager.RegisterServer("Server 2", "C:\\test2", 19133);
        var server3 = manager.RegisterServer("Server 3", "C:\\test3", 19134);

        Assert.NotEqual(server1.Id, server2.Id);
        Assert.NotEqual(server2.Id, server3.Id);
        Assert.NotEqual(server1.Id, server3.Id);
    }

    [Fact]
    public async Task RegisterServer_SetsDefaultValues()
    {
        var manager = new ServerManager();

        var serverInfo = manager.RegisterServer("Test Server", "C:\\test");

        Assert.Equal(19132, serverInfo.Port);
        Assert.Equal(ServerStatus.Stopped, serverInfo.Status);
        Assert.Equal("Unknown", serverInfo.Version);
        Assert.Equal(10, serverInfo.MaxPlayers);
        Assert.Equal(0, serverInfo.CurrentPlayers);
        Assert.True((DateTime.UtcNow - serverInfo.CreatedDate).TotalSeconds < 5);
        Assert.Null(serverInfo.LastStarted);
    }
}
