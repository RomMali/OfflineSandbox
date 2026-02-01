using System.Net.Sockets;
using System.Text;
using OfflineSandbox.Core;

Console.WriteLine("[Client] Connecting to The Vault...");

try 
{
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("[Client] Connected!");

    using var stream = client.GetStream();

    var packet = new NetworkPacket 
    { 
        Command = CommandType.ListFiles, 
        Payload = "" 
    };
    var data = packet.ToBytes();

    await stream.WriteAsync(data, 0, data.Length);
    Console.WriteLine("[Client] Sent Request: ListFiles");

    byte[] buffer = new byte[4096];
    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    
    if (bytesRead > 0)
    {
        var responseBytes = buffer[0..bytesRead];
        var response = NetworkPacket.FromBytes(responseBytes);
        Console.WriteLine($"[Client] Received: {response?.Payload}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] {ex.Message}");
}
