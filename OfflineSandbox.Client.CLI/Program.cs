using System.Net.Sockets;
using System.Text;
using OfflineSandbox.Core;
using System.IO; // Required for File and Path operations

Console.WriteLine("[Client] Connecting to The Vault...");

try 
{
    using var client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 5000);
    Console.WriteLine("[Client] Connected!");

    using var stream = client.GetStream();

    // --- UPLOAD LOGIC STARTS HERE ---

    // 1. Prepare a file to upload
    // We create a dummy file right now so you have something to test with.
    string fileName = "mission_report.txt";
    if (!File.Exists(fileName))
    {
        await File.WriteAllTextAsync(fileName, "Status: Mission Accomplished.\nTarget: Server Storage.");
        Console.WriteLine($"[Client] Created test file: {fileName}");
    }

    Console.WriteLine($"[Client] Uploading {fileName}...");

    // 2. Read the file from disk
    byte[] fileBytes = await File.ReadAllBytesAsync(fileName);
    
    // 3. Encode to Base64 (Binary -> Text)
    string base64Content = Convert.ToBase64String(fileBytes);

    // 4. Create the Payload: "filename|content"
    // We use the pipe '|' character as a separator.
    string payload = $"{fileName}|{base64Content}";

    // 5. Send the Packet
    var packet = new NetworkPacket 
    { 
        Command = CommandType.UploadFile, 
        Payload = payload 
    };
    
    var packetBytes = packet.ToBytes();
    await stream.WriteAsync(packetBytes, 0, packetBytes.Length);
    Console.WriteLine("[Client] Data sent. Waiting for confirmation...");

    // 6. Receive Confirmation from Server
    byte[] buffer = new byte[1024]; // Small buffer is fine for simple "OK" messages
    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
    
    if (bytesRead > 0)
    {
        var responseData = buffer[0..bytesRead];
        var response = NetworkPacket.FromBytes(responseData);

        if (response?.Command == CommandType.UploadFile)
        {
            Console.WriteLine($"[Success] Server says: {response.Payload}");
        }
        else if (response?.Command == CommandType.Error)
        {
            Console.WriteLine($"[Error] Server rejected upload: {response.Payload}");
        }
        else
        {
            Console.WriteLine($"[Client] Received unknown response: {response?.Payload}");
        }
    }
    // --- UPLOAD LOGIC ENDS HERE ---
}
catch (Exception ex)
{
    Console.WriteLine($"[Error] {ex.Message}");
}