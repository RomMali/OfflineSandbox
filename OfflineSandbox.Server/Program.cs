using System.Net;
using System.Net.Sockets;
using System.Text;
using OfflineSandbox.Core;

class Program
{
    private const int Port = 5000;

    static async Task Main(string[] args)
    {
        Console.WriteLine("[Server] Starting The Vault...");
        
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();

        Console.WriteLine($"[Server] Listening on port {Port}...");

        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("[Server] Client connected!");
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    var data = buffer[0..bytesRead];
                    var packet = NetworkPacket.FromBytes(data);

                    if (packet?.Command == CommandType.Handshake)
                    {
                        Console.WriteLine($"[Server] Received Handshake: {packet.Payload}");
                        
                        var response = new NetworkPacket 
                        { 
                            Command = CommandType.Handshake, 
                            Payload = "Pong" 
                        };
                        var resBytes = response.ToBytes();
                        await stream.WriteAsync(resBytes, 0, resBytes.Length);
                        Console.WriteLine("[Server] Sent Pong.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
    }
}
