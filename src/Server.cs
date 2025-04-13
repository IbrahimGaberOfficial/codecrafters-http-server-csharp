using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();

        try
        {
            while (true)
            {
                var client = await server.AcceptSocketAsync();
                _ = HandleClientAsync(client); // No Task.Run needed here
            }
        }
        finally
        {
            server.Stop();
        }
    }

    static async Task HandleClientAsync(Socket client)
    {
        try
        {
            using (client)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                
                if (bytesRead == 0) return; // Connection closed

                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                string[] requestLines = request.Split("\r\n");
                
                if (requestLines.Length == 0) return;

                string[] requestLineParts = requestLines[0].Split(' ');
                if (requestLineParts.Length < 2) return;
                
                string path = requestLineParts[1];
                byte[] response;

                if (path == "/")
                {
                    response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                }
                else if (path.StartsWith("/echo/"))
                {
                    string message = path.Substring(6);
                    response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 200 OK\r\n" +
                        "Content-Type: text/plain\r\n" +
                        $"Content-Length: {message.Length}\r\n" +
                        "\r\n" +
                        $"{message}");
                }
                else if (path.StartsWith("/files"))
                {
                    // get file byte number
                    string fileName = path.Substring(7);
                    string projectDir = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
                    string filePath = System.IO.Path.Combine(projectDir + "/tmp", fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        string fileContent = System.IO.File.ReadAllText(filePath);
                        string fileSize = fileContent.Length.ToString();

                        response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: application/octet-stream\r\n" +
                            $"Content-Length: {fileSize}\r\n" +
                            "\r\n" +
                            $"{fileContent}");
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
                    }
                   

                }
                else if (path == "/user-agent")
                {
                    string userAgent = "";
                    foreach (var line in requestLines)
                    {
                        if (line.StartsWith("User-Agent:"))
                        {
                            userAgent = line.Substring(11).Trim();
                            break;
                        }
                    }
                    response = Encoding.UTF8.GetBytes(
                        "HTTP/1.1 200 OK\r\n" +
                        "Content-Type: text/plain\r\n" +
                        $"Content-Length: {userAgent.Length}\r\n" +
                        "\r\n" +
                        $"{userAgent}");
                }
                else
                {
                    response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
                }

                await client.SendAsync(response, SocketFlags.None);
            }
        }
        catch (Exception ex)
        {
            // Silent handling - comment out in development if you need debugging
            // Console.WriteLine($"Error: {ex.Message}");
        }
    }
}