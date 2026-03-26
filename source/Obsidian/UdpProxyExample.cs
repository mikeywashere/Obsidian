namespace Obsidian;

/// <summary>
/// Example class demonstrating how to use the UDP proxy for intercepting Minecraft packets.
/// </summary>
public static class UdpProxyExample
{
    /// <summary>
    /// Example of how to start a UDP proxy with custom packet logging.
    /// </summary>
    public static async Task RunProxyExample()
    {
        // Create a UDP proxy that listens on port 19134 and forwards to localhost:19132
        using var proxy = new UdpProxy(
            listenPort: 19134,
            destinationHost: "127.0.0.1",
            destinationPort: 19132
        );

        // Subscribe to packet events for logging/inspection
        proxy.PacketReceived += (sender, args) =>
        {
            Console.WriteLine($"[CLIENT -> PROXY] Received {args.Data.Length} bytes from {args.RemoteEndPoint}");
            // You can inspect packet contents here
            // Example: LogPacketHex(args.Data);
        };

        proxy.PacketForwarded += (sender, args) =>
        {
            Console.WriteLine($"[PROXY -> SERVER] Forwarded {args.Data.Length} bytes to {args.RemoteEndPoint}");
        };

        proxy.ResponseReceived += (sender, args) =>
        {
            Console.WriteLine($"[SERVER -> PROXY] Received {args.Data.Length} bytes from server");
        };

        // Start the proxy (this will run until cancelled)
        var cts = new CancellationTokenSource();
        
        // Handle Ctrl+C to gracefully shutdown
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        Console.WriteLine("UDP Proxy starting... Press Ctrl+C to stop.");
        
        try
        {
            await proxy.StartAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Proxy stopped.");
        }
    }

    /// <summary>
    /// Example of creating a proxy from ServerProperties configuration.
    /// </summary>
    internal static IUdpProxy? CreateProxyFromConfig(ServerProperties properties)
    {
        if (!properties.EnableUdpProxy)
        {
            return null;
        }

        var proxy = new UdpProxy(
            properties.UdpProxyListenPort,
            properties.UdpProxyDestinationHost,
            properties.UdpProxyDestinationPort
        );

        // Add default logging
        proxy.PacketReceived += (sender, args) =>
        {
            Console.WriteLine($"Packet intercepted: {args.Data.Length} bytes from {args.RemoteEndPoint}");
        };

        return proxy;
    }

    private static void LogPacketHex(byte[] data)
    {
        Console.WriteLine($"Packet data (hex): {BitConverter.ToString(data).Replace("-", " ")}");
    }
}
