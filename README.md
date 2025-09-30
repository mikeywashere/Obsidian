# Obsidian
Minecraft Bedrock Server runner and manager with web UI

## Features

- **UDP Proxy**: Intercept and log Minecraft Bedrock UDP packets for debugging and analysis
- Server management and configuration
- Web UI for server administration

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

