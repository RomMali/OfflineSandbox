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
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        Console.WriteLine($"[Server] Client connected from {client.Client.RemoteEndPoint}");
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[4096];
                while (true) 
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    var data = buffer[0..bytesRead];
                    var packet = NetworkPacket.FromBytes(data);
                    
                    if (packet == null) 
                    {
                        Console.WriteLine("[Server] Received garbage data.");
                        
                        var errorPacket = new NetworkPacket 
                        { 
                            Command = CommandType.Error, 
                            Payload = "Invalid Packet Format" 
                        };
                        
                        var errBytes = errorPacket.ToBytes();
                        await stream.WriteAsync(errBytes, 0, errBytes.Length);
                        
                        continue; 
                    }

                    NetworkPacket response = new NetworkPacket();
                    string storagePath = "";
                    string fileName = "";
                    
                    switch (packet.Command)
                    {
                        case CommandType.ListFiles:
                            Console.WriteLine("[Server] Received Request: ListFiles");
                            storagePath = Path.Combine(AppContext.BaseDirectory, "../../../Storage/Master");
                            
                            if (!Directory.Exists(storagePath)) Directory.CreateDirectory(storagePath);
                            
                            var files = Directory.GetFiles(storagePath).Select(Path.GetFileName).ToArray();
                            response.Command = CommandType.ListFiles;
                            response.Payload = files.Length > 0 ? string.Join(",", files) : "No files found";
                            
                            break;

                        case CommandType.DownloadFile:
                            Console.WriteLine($"[Server] Request to download: {packet.Payload}");
                        
                            fileName = Path.GetFileName(packet.Payload); 
                            storagePath = Path.Combine(AppContext.BaseDirectory, "../../../Storage/Master");
                            string fullPath = Path.Combine(storagePath, fileName);

                            response.Command = CommandType.DownloadFile;

                            if (File.Exists(fullPath))
                            {
                                byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);
                                string base64Payload = Convert.ToBase64String(fileBytes);

                                response.Payload = base64Payload;
                                Console.WriteLine($"[Server] Sending {fileName} ({fileBytes.Length} bytes)...");
                            }

                            else
                            {
                                response.Command = CommandType.Error;
                                response.Payload = "File not found.";
                                Console.WriteLine($"[Server] File not found: {fileName}");
                            }
                            
                            break;

                            case CommandType.UploadFile:
                                Console.WriteLine("[Server] Receiving upload...");

                                // 1. Split the Payload: "filename|BASE64..."
                                var parts = packet.Payload.Split('|', 2); // Split only on the first pipe

                                if (parts.Length == 2)
                                {
                                    fileName = Path.GetFileName(parts[0]); // Security: Strip folder paths
                                    string base64Content = parts[1];

                                    // 2. Decode the Data
                                    byte[] fileBytes = Convert.FromBase64String(base64Content);
                                    
                                    // 3. Save to Disk
                                    string savePath = Path.Combine(AppContext.BaseDirectory, "../../../Storage/Master", fileName);
                                    await File.WriteAllBytesAsync(savePath, fileBytes);

                                    Console.WriteLine($"[Server] File saved: {fileName} ({fileBytes.Length} bytes)");

                                    response.Command = CommandType.UploadFile; // Acknowledge success
                                    response.Payload = "Upload Successful";
                                }
                                else
                                {
                                    response.Command = CommandType.Error;
                                    response.Payload = "Invalid Upload Format. Expected: 'filename|data'";
                                }
                                break;
                        default:
                            response.Command = CommandType.Handshake;
                            response.Payload = "Pong";
                            
                            break;
                    }

                    var resBytes = response.ToBytes();
                    await stream.WriteAsync(resBytes, 0, resBytes.Length);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Error: {ex.Message}");
        }
    }
}
