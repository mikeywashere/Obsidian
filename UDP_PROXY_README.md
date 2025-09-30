# UDP Proxy for Minecraft Bedrock Server

This feature provides a UDP proxy for intercepting and logging Minecraft Bedrock server UDP packets. The proxy sits between the client and server, allowing you to inspect traffic while maintaining full game functionality.

## Features

- **Packet Interception**: Capture all UDP packets sent between clients and the Minecraft server
- **Packet Logging**: Console logging for all intercepted packets with size and endpoint information
- **Bidirectional Forwarding**: Seamlessly forwards packets between clients and server
- **Event-Driven**: Subscribe to events for custom packet inspection and logging
- **Configuration Support**: Configure via ServerProperties or programmatically

## Configuration

Add the following properties to your `server.properties` file:

```properties
# Enable UDP proxy
enable-udp-proxy=true

# Port for proxy to listen on (clients connect here)
udp-proxy-listen-port=19134

# Destination server host (where to forward packets)
udp-proxy-destination-host=127.0.0.1

# Destination server port (actual Minecraft server)
udp-proxy-destination-port=19132
```

## Usage

### Basic Usage

```csharp
using Obsidian;

// Create a UDP proxy
using var proxy = new UdpProxy(
    listenPort: 19134,           // Port clients connect to
    destinationHost: "127.0.0.1", // Minecraft server address
    destinationPort: 19132        // Minecraft server port
);

// Start the proxy
await proxy.StartAsync();
```

### With Event Logging

```csharp
using Obsidian;

using var proxy = new UdpProxy(19134, "127.0.0.1", 19132);

// Log packets from client to server
proxy.PacketReceived += (sender, args) =>
{
    Console.WriteLine($"Client -> Proxy: {args.Data.Length} bytes from {args.RemoteEndPoint}");
};

// Log when packets are forwarded
proxy.PacketForwarded += (sender, args) =>
{
    Console.WriteLine($"Proxy -> Server: {args.Data.Length} bytes");
};

// Log responses from server
proxy.ResponseReceived += (sender, args) =>
{
    Console.WriteLine($"Server -> Proxy: {args.Data.Length} bytes");
};

await proxy.StartAsync();
```

### From ServerProperties

```csharp
var properties = ServerProperties.Load("server.properties");

if (properties.EnableUdpProxy)
{
    var proxy = new UdpProxy(
        properties.UdpProxyListenPort,
        properties.UdpProxyDestinationHost,
        properties.UdpProxyDestinationPort
    );
    
    await proxy.StartAsync();
}
```

## Use Cases

1. **Debugging**: Inspect packet flow during development
2. **Security Analysis**: Monitor for suspicious traffic patterns
3. **Performance Monitoring**: Track packet sizes and frequencies
4. **Protocol Analysis**: Study Minecraft Bedrock protocol behavior
5. **Custom Logging**: Implement advanced packet filtering and logging

## Architecture

The UDP proxy maintains separate UDP clients for each connecting client endpoint, ensuring proper routing of bidirectional traffic. It operates asynchronously and can handle multiple concurrent clients.

```
[Client] --UDP--> [Proxy:19134] --UDP--> [Server:19132]
                      |
                   [Logging]
```

## Notes

- The proxy creates a separate socket for each client endpoint to maintain proper packet routing
- All communication is asynchronous to handle high throughput
- Console logging is enabled by default but can be customized via events
- The proxy gracefully handles cancellation and cleanup

## Example Output

```
UDP Proxy started listening on port 19134
Forwarding to 127.0.0.1:19132
Received 42 bytes from 192.168.1.100:54321
Forwarded 42 bytes to 127.0.0.1:19132
Received 128 bytes from server
Forwarded 128 bytes to 192.168.1.100:54321
```
