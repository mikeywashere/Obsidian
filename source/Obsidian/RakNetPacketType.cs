namespace Obsidian;

public enum RakNetPacketType
{
    Unknown = -1,
    UnconnectedPing = 0x01,
    UnconnectedPong = 0x1c,
    OpenConnectionRequest1 = 0x05,
    OpenConnectionReply1 = 0x06,
    OpenConnectionRequest2 = 0x07,
    OpenConnectionReply2 = 0x08,
    NewIncomingConnection = 0x13,
    DisconnectNotification = 0x15,
    Ack = 0xc0,
    Nack = 0xa0,
    DataPacket = 0x84,  // representative value; 0x80–0x8f all map to this
    GamePacket = 0xfe,
}
