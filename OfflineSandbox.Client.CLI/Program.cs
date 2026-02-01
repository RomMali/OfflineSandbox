using System.Net.Sockets;
using System.Text;
using OfflineSandbox.Core;
using System.IO;

Console.WriteLine("[Client] Connecting to The Vault...");

try 
{
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("[Client] Connected!");

    using var stream = client.GetStream();

    string targetFile = "app.js"; 

    var packet = new NetworkPacket 
    { 
        Command = CommandType.DownloadFile, 
        Payload = targetFile                
    };
    var data = packet.ToBytes();
    await stream.WriteAsync(data, 0, data.Length);
    Console.WriteLine($"[Client] Asking for: {targetFile}...");

    byte[] buffer = new byte[10_000_000]; 
    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    
    if (bytesRead > 0)
    {
        var responseData = buffer[0..bytesRead];
        var response = NetworkPacket.FromBytes(responseData);

        if (response?.Command == CommandType.DownloadFile)
        {
            byte[] fileBytes = Convert.FromBase64String(response.Payload);
            
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), "Downloaded_" + targetFile);
            await File.WriteAllBytesAsync(savePath, fileBytes);
            
            Console.WriteLine($"[Success] Saved to: {savePath}");
            Console.WriteLine($"[Size] {fileBytes.Length} bytes");
        }
        else
        {
            Console.WriteLine($"[Server Error] {response?.Payload}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] {ex.Message}");
}
