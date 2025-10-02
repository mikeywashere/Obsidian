# Obsidian
Minecraft Bedrock Server runner and manager with web UI

## Features

- **UDP Proxy**: Intercept and log Minecraft Bedrock UDP packets for debugging and analysis
- **Server Management**: Start, stop, and configure Minecraft Bedrock servers
- **Web UI**: Modern Blazor WebAssembly interface for server administration
- **Microsoft Authentication**: Secure access with Microsoft account login
- **Real-time Monitoring**: View server status, player counts, and logs
- **Log Viewer**: Intuitive log viewing with color-coded levels

## UDP Proxy

Obsidian includes a built-in UDP proxy for intercepting and logging Minecraft Bedrock server traffic. This is useful for debugging, protocol analysis, and monitoring network traffic.

For detailed information about the UDP proxy feature, see [UDP_PROXY_README.md](UDP_PROXY_README.md).

### Quick Start

Enable the UDP proxy in your `server.properties`:

```properties
enable-udp-proxy=true
udp-proxy-listen-port=19134
udp-proxy-destination-host=127.0.0.1
udp-proxy-destination-port=19132
```

Then connect your Minecraft client to port 19134 instead of 19132 to have all traffic intercepted and logged.

## Web UI

The web UI provides a modern interface for managing your Minecraft Bedrock servers.

### Quick Start

```bash
cd source/Obsidian.Web
dotnet run
```

Navigate to `https://localhost:7223` or `http://localhost:5276`

### Features

- Microsoft account authentication
- Server list with status indicators
- Individual server details and controls
- Real-time log viewer
- Responsive flat design

For detailed setup instructions including Microsoft authentication configuration, see [source/Obsidian.Web/README.md](source/Obsidian.Web/README.md).

