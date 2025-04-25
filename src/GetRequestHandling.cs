using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace GETRequestHandling;
public static class GETRequestHandler
{
    public static byte[] HandleGETRequest(Socket clinet, string[] requestLines, string[] requestLineParts)
    {
        string path = requestLineParts[1]; // remove after final edits
        byte[] response; // remove after final edits


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
            // get file name
            string fileName = path.Substring("/files/".Length);
            var argv = Environment.GetCommandLineArgs();
            var currentDirectory = argv[2];
            string filePath = System.IO.Path.Combine(currentDirectory, fileName);

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
        return response;



    }
}

