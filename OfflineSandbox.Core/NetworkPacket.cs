using System.Text;
using System.Text.Json;

namespace OfflineSandbox.Core;

public class NetworkPacket
{
    public CommandType Command { get; set; }
    public string Payload { get; set; } = string.Empty;

    // Serialization: Object -> Bytes
    public byte[] ToBytes()
    {
        var json = JsonSerializer.Serialize(this);
        return Encoding.UTF8.GetBytes(json);
    }

    // Deserialization: Bytes -> Object
    public static NetworkPacket? FromBytes(byte[] data)
    {
        try 
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<NetworkPacket>(json);
        }
        catch 
        { 
            return null; // Malformed packet
        }
    }
}
