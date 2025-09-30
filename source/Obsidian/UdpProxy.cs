using System.Net;
using System.Net.Sockets;

namespace Obsidian;

/// <summary>
/// A UDP proxy that intercepts, logs, and forwards UDP packets to a destination server.
/// Designed for intercepting Minecraft Bedrock server UDP traffic.
/// </summary>
public class UdpProxy : IUdpProxy
{
    private readonly UdpClient _listener;
    private readonly IPEndPoint _destinationEndPoint;
    private readonly int _listenPort;
    private readonly Dictionary<IPEndPoint, UdpClient> _clientSockets;
    private bool _isRunning;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Event triggered when a packet is received from a client.
    /// </summary>
    public event EventHandler<PacketEventArgs>? PacketReceived;

    /// <summary>
    /// Event triggered when a packet is forwarded to the destination server.
    /// </summary>
    public event EventHandler<PacketEventArgs>? PacketForwarded;

    /// <summary>
    /// Event triggered when a response is received from the destination server.
    /// </summary>
    public event EventHandler<PacketEventArgs>? ResponseReceived;

    /// <summary>
    /// Initializes a new instance of the UDP proxy.
    /// </summary>
    /// <param name="listenPort">The port to listen on for incoming UDP packets.</param>
    /// <param name="destinationHost">The hostname or IP address of the destination server.</param>
    /// <param name="destinationPort">The port of the destination server.</param>
    public UdpProxy(int listenPort, string destinationHost, int destinationPort)
    {
        _listenPort = listenPort;
        _listener = new UdpClient(listenPort);
        try
        {
            var addresses = Dns.GetHostAddresses(destinationHost);
            if (addresses == null || addresses.Length == 0)
            {
                throw new ArgumentException($"Could not resolve destination host: {destinationHost}");
            }
            _destinationEndPoint = new IPEndPoint(addresses[0], destinationPort);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to resolve destination host '{destinationHost}': {ex.Message}", ex);
        }
        _clientSockets = new Dictionary<IPEndPoint, UdpClient>();
    }

    /// <summary>
    /// Starts the UDP proxy to begin intercepting and forwarding packets.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token to stop the proxy.</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Proxy is already running.");
        }

        _isRunning = true;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Console.WriteLine($"UDP Proxy started listening on port {_listenPort}");
        Console.WriteLine($"Forwarding to {_destinationEndPoint}");

        try
        {
            while (_isRunning && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await _listener.ReceiveAsync(_cancellationTokenSource.Token);
                _ = Task.Run(async () => await HandleClientPacketAsync(result.RemoteEndPoint, result.Buffer), _cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("UDP Proxy stopped.");
        }
    }

    /// <summary>
    /// Stops the UDP proxy.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _cancellationTokenSource?.Cancel();
    }

    private async Task HandleClientPacketAsync(IPEndPoint clientEndPoint, byte[] data)
    {
        // Log the received packet
        PacketReceived?.Invoke(this, new PacketEventArgs(clientEndPoint, data, PacketDirection.ClientToServer));
        Console.WriteLine($"Received {data.Length} bytes from {clientEndPoint}");

        // Get or create a socket for this client
        if (!_clientSockets.TryGetValue(clientEndPoint, out var clientSocket))
        {
            clientSocket = new UdpClient();
            _clientSockets[clientEndPoint] = clientSocket;

            // Start listening for responses from the server for this client
            _ = Task.Run(async () => await ListenForServerResponseAsync(clientEndPoint, clientSocket));
        }

        // Forward the packet to the destination server
        await clientSocket.SendAsync(data, data.Length, _destinationEndPoint);
        PacketForwarded?.Invoke(this, new PacketEventArgs(_destinationEndPoint, data, PacketDirection.ClientToServer));
        Console.WriteLine($"Forwarded {data.Length} bytes to {_destinationEndPoint}");
    }

    private async Task ListenForServerResponseAsync(IPEndPoint clientEndPoint, UdpClient clientSocket)
    {
        try
        {
            while (_isRunning)
            {
                var result = await clientSocket.ReceiveAsync();
                
                // Log the response
                ResponseReceived?.Invoke(this, new PacketEventArgs(result.RemoteEndPoint, result.Buffer, PacketDirection.ServerToClient));
                Console.WriteLine($"Received {result.Buffer.Length} bytes from server");

                // Forward the response back to the client
                await _listener.SendAsync(result.Buffer, result.Buffer.Length, clientEndPoint);
                Console.WriteLine($"Forwarded {result.Buffer.Length} bytes to {clientEndPoint}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listening for server response: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
        _listener?.Dispose();
        _cancellationTokenSource?.Dispose();

        foreach (var socket in _clientSockets.Values)
        {
            socket?.Dispose();
        }
        _clientSockets.Clear();
    }
}

/// <summary>
/// Event arguments for packet events.
/// </summary>
public class PacketEventArgs : EventArgs
{
    public IPEndPoint RemoteEndPoint { get; }
    public byte[] Data { get; }
    public PacketDirection Direction { get; }

    public PacketEventArgs(IPEndPoint remoteEndPoint, byte[] data, PacketDirection direction)
    {
        RemoteEndPoint = remoteEndPoint;
        Data = data;
        Direction = direction;
    }
}

/// <summary>
/// Represents the direction of a packet.
/// </summary>
public enum PacketDirection
{
    ClientToServer,
    ServerToClient
}
