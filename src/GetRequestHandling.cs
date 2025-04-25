using System.Dynamic;
using System.Net.Sockets;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;


namespace GETRequestHandling;
public static class GETRequestHandler
{
    static string[] getCompressionHeaders(string[] requestLines)
    {
        string[] acceptEncoding = null;
        // Check if the request contains compression headers
        foreach (var line in requestLines)
        {
            if (line.StartsWith("Accept-Encoding:"))
            {
                acceptEncoding = line.Substring("Accept-Encoding:".Length).Trim().Split(", ");
                return acceptEncoding;
            }
        }
        return acceptEncoding;
    }

    public static byte[] HandleGETRequest(Socket clinet, string[] requestLines, string[] requestLineParts)
    {
        string path = requestLineParts[1];
        byte[] response;

        if (path == "/")
        {
            response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
        }
        else if (path.StartsWith("/echo/"))
        {
            var CompressionHeaders = getCompressionHeaders(requestLines);
            string message = path.Substring("/echo/".Length);

            if (CompressionHeaders != null && CompressionHeaders.Contains("gzip"))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                using (var compressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(bytes, 0, bytes.Length);
                    }
                    message = Convert.ToBase64String(compressedStream.ToArray());

                    response = Encoding.UTF8.GetBytes(
                   "HTTP/1.1 200 OK\r\n" +
                   "Content-Encoding: gzip\r\n" +
                   "Content-Type: text/plain\r\n" +
                   $"Content-Length: {message.Length}\r\n" +
                   "\r\n" +
                   $"{message}");

                    return response;
                }

            }

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

