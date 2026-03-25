using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Obsidian.Api.Hubs;
using Obsidian.Api.Services;
using Obsidian.Models;

namespace Obsidian.UnitTests;

public class ServerLogBroadcasterTests
{
    [Fact]
    public async Task StartAsync_SubscribesToLogReceivedEvent()
    {
        var serverManager = Substitute.For<IServerManager>();
        var hubContext = Substitute.For<IHubContext<ServerLogHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var broadcaster = new ServerLogBroadcaster(serverManager, hubContext);

        await broadcaster.StartAsync(CancellationToken.None);

        var log = new ServerLog { Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Test" };
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            new ServerLogEventArgs("test-server", log)
        );

        // Wait for async event handler to complete
        await Task.Delay(100);

        await clientProxy.ReceivedWithAnyArgs(1).SendAsync("ReceiveLog");
    }

    [Fact]
    public async Task StopAsync_UnsubscribesFromLogReceivedEvent()
    {
        var serverManager = Substitute.For<IServerManager>();
        var hubContext = Substitute.For<IHubContext<ServerLogHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var broadcaster = new ServerLogBroadcaster(serverManager, hubContext);

        await broadcaster.StartAsync(CancellationToken.None);
        await broadcaster.StopAsync(CancellationToken.None);

        var log = new ServerLog { Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Test" };
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            new ServerLogEventArgs("test-server", log)
        );

        // Wait for any potential async event handler to complete
        await Task.Delay(100);

        await clientProxy.DidNotReceiveWithAnyArgs().SendAsync("ReceiveLog");
    }

    [Fact]
    public async Task OnLogReceived_SendsToCorrectGroup()
    {
        var serverManager = Substitute.For<IServerManager>();
        var hubContext = Substitute.For<IHubContext<ServerLogHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var broadcaster = new ServerLogBroadcaster(serverManager, hubContext);

        await broadcaster.StartAsync(CancellationToken.None);

        var log = new ServerLog { Timestamp = DateTime.UtcNow, Level = LogLevel.Info, Message = "Test" };
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            new ServerLogEventArgs("srv-1", log)
        );

        // Wait for async event handler to complete
        await Task.Delay(100);

        clients.Received(1).Group("server-srv-1");
    }

    [Fact]
    public async Task OnLogReceived_SendsCorrectLogPayload()
    {
        var serverManager = Substitute.For<IServerManager>();
        var hubContext = Substitute.For<IHubContext<ServerLogHub>>();
        var clients = Substitute.For<IHubClients>();
        var clientProxy = Substitute.For<IClientProxy>();
        hubContext.Clients.Returns(clients);
        clients.Group(Arg.Any<string>()).Returns(clientProxy);
        var broadcaster = new ServerLogBroadcaster(serverManager, hubContext);

        await broadcaster.StartAsync(CancellationToken.None);

        var expectedLog = new ServerLog
        {
            Timestamp = new DateTime(2026, 3, 21, 10, 30, 0, DateTimeKind.Utc),
            Level = LogLevel.Warning,
            Message = "Low memory warning"
        };
        serverManager.LogReceived += Raise.Event<EventHandler<ServerLogEventArgs>>(
            serverManager,
            new ServerLogEventArgs("test-server", expectedLog)
        );

        // Wait for async event handler to complete
        await Task.Delay(100);

        await clientProxy.ReceivedWithAnyArgs(1).SendAsync("ReceiveLog");
    }
}
