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
        // var argv = Environment.GetCommandLineArgs();
        // var currentDirectory = argv[2];
        // Console.WriteLine("currentDir = " + currentDirectory);
        // System.Console.WriteLine(argv[0]);
        // System.Console.WriteLine(argv[1]);
        // System.Console.WriteLine(argv[2]);
        // return;

        try
        {
            while (true)
            {
                var client = await server.AcceptSocketAsync();
                _ = HandleClientAsync(client);
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

                string path = requestLineParts[1]; // remove after final edits
                byte[] response; // remove after final edits

                // get the method type
                string method = requestLineParts[0];

                

                if (method.Equals("GET"))
                {
                    GetRequestHandling.GetRequestHandler.HandleGetRequest(path);
                   
                }
                else if (method.Equals("POST"))
                {
                    // get the body
                    string body = requestLines[requestLines.Length - 1];
                    if (path.StartsWith("/files"))
                    {
                        var argv = Environment.GetCommandLineArgs();
                        var currentDirectory = argv[2];
                        // get the file name
                        string fileName = path.Substring("/files/".Length);
                        string filePath = System.IO.Path.Combine(currentDirectory, fileName);
                        System.IO.File.WriteAllText(filePath, body);

                        response = Encoding.UTF8.GetBytes("HTTP/1.1 201 Created\r\n\r\n");
                    }
                    else
                    {
                        response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
                    }
                    await client.SendAsync(response, SocketFlags.None);

                }
            }

        }
        catch (Exception ex)
        {
            // Silent handling - comment out in development if you need debugging
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}