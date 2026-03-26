using System.Net;

namespace Obsidian;

/// <summary>
/// Interface for UDP proxy operations.
/// </summary>
public interface IUdpProxy : IDisposable
{
    /// <summary>
    /// Event triggered when a packet is received from a client.
    /// </summary>
    event EventHandler<PacketEventArgs>? PacketReceived;

    /// <summary>
    /// Event triggered when a packet is forwarded to the destination server.
    /// </summary>
    event EventHandler<PacketEventArgs>? PacketForwarded;

    /// <summary>
    /// Event triggered when a response is received from the destination server.
    /// </summary>
    event EventHandler<PacketEventArgs>? ResponseReceived;

    /// <summary>
    /// Event triggered after each packet (both directions) has been parsed by <see cref="RakNetParser"/>.
    /// Only fires if there are subscribers — zero-cost when unused.
    /// </summary>
    event EventHandler<ParsedPacketEventArgs>? PacketParsed;

    /// <summary>
    /// Starts the UDP proxy to begin intercepting and forwarding packets.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token to stop the proxy.</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the UDP proxy.
    /// </summary>
    void Stop();
}

/// <summary>
/// Event arguments carrying a protocol-parsed RakNet/Minecraft packet.
/// </summary>
public class ParsedPacketEventArgs : EventArgs
{
    public IPEndPoint RemoteEndPoint { get; init; } = null!;
    public ParsedPacket Packet { get; init; } = null!;
}
