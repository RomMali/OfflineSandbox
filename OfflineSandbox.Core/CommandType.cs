namespace OfflineSandbox.Core;

// 1 byte is enough for 255 commands. Efficiency matters.
public enum CommandType : byte
{
    Handshake = 0x01,
    Error = 0xFF
}
