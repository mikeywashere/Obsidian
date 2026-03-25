using NSubstitute;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class PlayerTrackerTests
{
    private IServerManager CreateServerManager() => Substitute.For<IServerManager>();

    private static ServerLogEventArgs MakeLog(string serverId, string message) =>
        new(serverId, new ServerLog
        {
            Timestamp = DateTime.UtcNow,
            Level = LogLevel.Info,
            Message = message
        });

    [Fact]
    public void GetPlayers_ReturnsEmpty_WhenNoPlayersConnected()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        var players = tracker.GetPlayers("server-1");

        Assert.Empty(players);
    }

    [Fact]
    public void OnConnectLog_AddsPlayer_AndFiresPlayerJoined()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        PlayerEventArgs? receivedArgs = null;
        tracker.PlayerJoined += (_, e) => receivedArgs = e;

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 2535416409")
        );

        Assert.NotNull(receivedArgs);
        Assert.Equal("server-1", receivedArgs.ServerId);
        Assert.Equal("Steve", receivedArgs.Player.Name);
        Assert.Equal("2535416409", receivedArgs.Player.Xuid);

        var players = tracker.GetPlayers("server-1").ToList();
        Assert.Single(players);
        Assert.Equal("Steve", players[0].Name);
    }

    [Fact]
    public void OnConnectLog_WithInfoPrefix_AddsPlayer()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        PlayerEventArgs? receivedArgs = null;
        tracker.PlayerJoined += (_, e) => receivedArgs = e;

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "[INFO] Player connected: Alex, xuid: 1234567890")
        );

        Assert.NotNull(receivedArgs);
        Assert.Equal("Alex", receivedArgs.Player.Name);
        Assert.Equal("1234567890", receivedArgs.Player.Xuid);
    }

    [Fact]
    public void OnDisconnectLog_RemovesPlayer_AndFiresPlayerLeft()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 2535416409")
        );

        PlayerEventArgs? leftArgs = null;
        tracker.PlayerLeft += (_, e) => leftArgs = e;

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player disconnected: Steve, xuid: 2535416409")
        );

        Assert.NotNull(leftArgs);
        Assert.Equal("Steve", leftArgs.Player.Name);
        Assert.Empty(tracker.GetPlayers("server-1"));
    }

    [Fact]
    public void OnDisconnectLog_WithNoExistingRecord_DoesNotThrow()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        var ex = Record.Exception(() =>
        {
            serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
                serverManager,
                MakeLog("server-1", "Player disconnected: Ghost, xuid: 9999999999")
            );
        });

        Assert.Null(ex);
    }

    [Fact]
    public void OnDisconnectLog_DoesNotFirePlayerLeft_WhenNoRecord()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        bool fired = false;
        tracker.PlayerLeft += (_, _) => fired = true;

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player disconnected: Ghost, xuid: 9999999999")
        );

        Assert.False(fired);
    }

    [Fact]
    public void GetPlayers_IsScopedToServerId()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 111")
        );
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-2", "Player connected: Alex, xuid: 222")
        );

        var server1Players = tracker.GetPlayers("server-1").ToList();
        var server2Players = tracker.GetPlayers("server-2").ToList();

        Assert.Single(server1Players);
        Assert.Equal("Steve", server1Players[0].Name);
        Assert.Single(server2Players);
        Assert.Equal("Alex", server2Players[0].Name);
    }

    [Fact]
    public void ConnectLog_UpdatesExistingPlayer_IfSameXuid()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 111")
        );
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 111")
        );

        Assert.Single(tracker.GetPlayers("server-1"));
    }

    [Fact]
    public void UnrelatedLog_IsIgnored()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        bool joined = false;
        bool left = false;
        tracker.PlayerJoined += (_, _) => joined = true;
        tracker.PlayerLeft += (_, _) => left = true;

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Server started on port 19132")
        );

        Assert.False(joined);
        Assert.False(left);
        Assert.Empty(tracker.GetPlayers("server-1"));
    }

    [Fact]
    public void GetPlayers_ReturnsSnapshot_NotLiveReference()
    {
        var serverManager = CreateServerManager();
        var tracker = new PlayerTracker(serverManager);

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player connected: Steve, xuid: 111")
        );

        var snapshot = tracker.GetPlayers("server-1").ToList();

        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            MakeLog("server-1", "Player disconnected: Steve, xuid: 111")
        );

        // Snapshot taken before disconnect must not reflect post-disconnect state
        Assert.Single(snapshot);
        Assert.Equal("Steve", snapshot[0].Name);
        Assert.Empty(tracker.GetPlayers("server-1"));
    }
}
