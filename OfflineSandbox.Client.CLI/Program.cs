using System.Net.Sockets;
using System.Text;
using OfflineSandbox.Core;

Console.WriteLine("[Client] Connecting to The Vault...");

try 
{
    // 1. Connect to Localhost (The Server on this machine)
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("[Client] Connected!");

    using var stream = client.GetStream();

    // 2. Prepare the Handshake
    var packet = new NetworkPacket 
    { 
        Command = CommandType.Handshake, 
        Payload = "Hello from Client!" 
    };
    var data = packet.ToBytes();

    // 3. Send It
    await stream.WriteAsync(data, 0, data.Length);
    Console.WriteLine("[Client] Sent Handshake. Waiting for reply...");

    // 4. Wait for Reply
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
    Console.WriteLine($"[Error] Connection failed: {ex.Message}");
    Console.WriteLine("Did you forget to start the Server in a separate terminal?");
}
