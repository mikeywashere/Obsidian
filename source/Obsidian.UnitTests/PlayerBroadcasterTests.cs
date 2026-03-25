using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Obsidian.Api.Hubs;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class PlayerBroadcasterTests
{
    private static (IPlayerTracker, IHubContext<PlayerHub>, IClientProxy, PlayerBroadcaster) BuildBroadcaster()
    {
        var playerTracker = Substitute.For<IPlayerTracker>();
        var hubContext = Substitute.For<IHubContext<PlayerHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var broadcaster = new PlayerBroadcaster(playerTracker, hubContext);
        return (playerTracker, hubContext, clientProxy, broadcaster);
    }

    private static PlayerEventArgs MakeEvent(string serverId, string name = "Steve", string xuid = "111") =>
        new(serverId, new PlayerInfo(serverId, name, xuid, DateTime.UtcNow, DateTime.UtcNow));

    [Fact]
    public async Task StartAsync_SubscribesToPlayerJoined()
    {
        var (playerTracker, _, clientProxy, broadcaster) = BuildBroadcaster();

        await broadcaster.StartAsync(CancellationToken.None);

        playerTracker.PlayerJoined += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("server-1"));
        await Task.Delay(100);

        await clientProxy.Received(1).SendCoreAsync("PlayerJoined", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_SubscribesToPlayerLeft()
    {
        var (playerTracker, _, clientProxy, broadcaster) = BuildBroadcaster();

        await broadcaster.StartAsync(CancellationToken.None);

        playerTracker.PlayerLeft += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("server-1"));
        await Task.Delay(100);

        await clientProxy.Received(1).SendCoreAsync("PlayerLeft", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StopAsync_UnsubscribesEvents()
    {
        var (playerTracker, _, clientProxy, broadcaster) = BuildBroadcaster();

        await broadcaster.StartAsync(CancellationToken.None);
        await broadcaster.StopAsync(CancellationToken.None);

        playerTracker.PlayerJoined += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("server-1"));
        playerTracker.PlayerLeft += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("server-1"));
        await Task.Delay(100);

        await clientProxy.DidNotReceive().SendCoreAsync("PlayerJoined", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
        await clientProxy.DidNotReceive().SendCoreAsync("PlayerLeft", Arg.Any<object[]>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnPlayerJoined_SendsToCorrectGroup()
    {
        var (playerTracker, hubContext, _, broadcaster) = BuildBroadcaster();
        var clients = hubContext.Clients;

        await broadcaster.StartAsync(CancellationToken.None);

        playerTracker.PlayerJoined += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("srv-42"));
        await Task.Delay(100);

        clients.Received(1).Group("server-srv-42");
    }

    [Fact]
    public async Task OnPlayerLeft_SendsToCorrectGroup()
    {
        var (playerTracker, hubContext, _, broadcaster) = BuildBroadcaster();
        var clients = hubContext.Clients;

        await broadcaster.StartAsync(CancellationToken.None);

        playerTracker.PlayerLeft += Raise.Event<EventHandler<PlayerEventArgs>>(
            playerTracker, MakeEvent("srv-42"));
        await Task.Delay(100);

        clients.Received(1).Group("server-srv-42");
    }
}

