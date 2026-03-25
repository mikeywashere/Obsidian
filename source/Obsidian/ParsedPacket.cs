namespace Obsidian;

public class ParsedPacket
{
    public RakNetPacketType PacketType { get; init; }
    public PacketDirection Direction { get; init; }
    public byte[] RawData { get; init; } = [];
    public bool IsEncrypted { get; init; }

    // Populated for UnconnectedPong
    public string? ServerMotd { get; init; }
    public int? PlayerCount { get; init; }
    public int? MaxPlayers { get; init; }
    public string? WorldName { get; init; }
    public string? GameMode { get; init; }

    // Populated for DataPacket (0x80–0x8f)
    public long? SequenceNumber { get; init; }

    // Populated for connection packets
    public string? ClientAddress { get; init; }
}
