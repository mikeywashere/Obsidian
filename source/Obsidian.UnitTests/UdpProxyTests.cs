using System.Net;
using System.Net.Sockets;
using System.Text;
using Shouldly;

namespace Obsidian.UnitTests;

public class UdpProxyTests
{
    [Fact]
    public async Task UdpProxy_ShouldForwardPackets()
    {
        // Arrange
        const int proxyListenPort = 19200;
        const int destinationPort = 19201;
        const string testMessage = "Hello Minecraft!";
        var receivedFromDestination = false;
        var destinationReceivedData = Array.Empty<byte>();

        // Setup destination server (simulates the actual Minecraft server)
        using var destinationServer = new UdpClient(destinationPort);
        var destinationTask = Task.Run(async () =>
        {
            var result = await destinationServer.ReceiveAsync();
            destinationReceivedData = result.Buffer;
            receivedFromDestination = true;
            
            // Send a response back
            await destinationServer.SendAsync(Encoding.UTF8.GetBytes("Response"), result.RemoteEndPoint);
        });

        // Setup proxy
        using var proxy = new UdpProxy(proxyListenPort, "127.0.0.1", destinationPort);
        var proxyTask = Task.Run(async () => await proxy.StartAsync());

        // Give the proxy time to start
        await Task.Delay(100);

        // Act - Send a packet through the proxy
        using var client = new UdpClient();
        var testData = Encoding.UTF8.GetBytes(testMessage);
        await client.SendAsync(testData, testData.Length, new IPEndPoint(IPAddress.Loopback, proxyListenPort));

        // Wait for the destination to receive the packet
        await Task.Delay(500);

        // Assert
        receivedFromDestination.ShouldBeTrue();
        destinationReceivedData.ShouldNotBeEmpty();
        Encoding.UTF8.GetString(destinationReceivedData).ShouldBe(testMessage);

        // Cleanup
        proxy.Stop();
    }

    [Fact]
    public async Task UdpProxy_ShouldRaisePacketReceivedEvent()
    {
        // Arrange
        const int proxyListenPort = 19202;
        const int destinationPort = 19203;
        var eventRaised = false;
        byte[]? receivedData = null;

        using var destinationServer = new UdpClient(destinationPort);
        var destinationTask = Task.Run(async () =>
        {
            try
            {
                await destinationServer.ReceiveAsync();
            }
            catch { }
        });

        using var proxy = new UdpProxy(proxyListenPort, "127.0.0.1", destinationPort);
        proxy.PacketReceived += (sender, args) =>
        {
            eventRaised = true;
            receivedData = args.Data;
        };

        var proxyTask = Task.Run(async () => await proxy.StartAsync());

        // Give the proxy time to start
        await Task.Delay(100);

        // Act
        using var client = new UdpClient();
        var testData = Encoding.UTF8.GetBytes("Test packet");
        await client.SendAsync(testData, testData.Length, new IPEndPoint(IPAddress.Loopback, proxyListenPort));

        // Wait for the event to be raised
        await Task.Delay(500);

        // Assert
        eventRaised.ShouldBeTrue();
        receivedData.ShouldNotBeNull();

        // Cleanup
        proxy.Stop();
    }

    [Fact]
    public void UdpProxy_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var proxy = new UdpProxy(19204, "127.0.0.1", 19205);

        // Assert - if we get here without exception, initialization succeeded
        proxy.ShouldNotBeNull();
    }

    [Fact]
    public void UdpProxy_ShouldStopWithoutError()
    {
        // Arrange
        using var proxy = new UdpProxy(19206, "127.0.0.1", 19207);
        var proxyTask = Task.Run(async () => await proxy.StartAsync());
        
        // Give the proxy time to start
        Thread.Sleep(100);

        // Act & Assert - should not throw
        Should.NotThrow(() => proxy.Stop());
    }
}
