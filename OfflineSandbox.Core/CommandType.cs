namespace OfflineSandbox.Core;

// 1 byte is enough for 255 commands. Efficiency matters.
public enum CommandType : byte
{
    Handshake = 0x01,
    ListFiles = 0x02,
    DownloadFile = 0x03,
    Error = 0xFF
}
