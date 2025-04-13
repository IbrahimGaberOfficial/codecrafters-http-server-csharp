using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static  async Task Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();
        Console.WriteLine("Server started. Waiting for connections...");
        
        while (true) // Keep server running for multiple requests
        {
            using Socket client = await server.AcceptSocketAsync();
            _ =Task.Run(() => HandleClient(client));
        }
    }
    static void HandleClient(Socket client)
    {
        using (client)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = client.Receive(buffer);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var requestLines = request.Split("\r\n");
            if (requestLines.Length == 0) return;

            var requestLineParts = requestLines[0].Split(' ', 3);
            if (requestLineParts.Length < 2) return;
            
            var path = requestLineParts[1];
            byte[] response;

            if (path == "/")
            {
                response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
            }
            else if (path.StartsWith("/echo/"))
            {
                var message = path.Substring(6); // Skip "/echo/"
                response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 200 OK\r\n" +
                    "Content-Type: text/plain\r\n" +
                    $"Content-Length: {message.Length}\r\n" +
                    "\r\n" +
                    $"{message}");
            }
            else if (path == "/user-agent")
            {
                string userAgent = "";
                
                // Search through all headers for User-Agent
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

            client.Send(response);
        }
    }
}