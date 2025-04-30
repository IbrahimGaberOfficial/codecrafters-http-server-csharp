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

            // if (CompressionHeaders != null && CompressionHeaders.Contains("gzip"))
            // {
            //     byte[] bytes = Encoding.UTF8.GetBytes(message);
            //     using (var compressedStream = new MemoryStream())
            //     {
            //         using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            //         {
            //             gzipStream.Write(bytes, 0, bytes.Length);
            //         }
            //         var compressedBytes = compressedStream.ToArray();
            //         string hexString = BitConverter.ToString(compressedBytes);//.Replace("-", " ");


            //         response = Encoding.UTF8.GetBytes(
            //        "HTTP/1.1 200 OK\r\n" +
            //        "Content-Encoding: gzip\r\n" +
            //        "Content-Type: text/plain\r\n" +
            //        $"Content-Length: {compressedBytes.Length}\r\n" +
            //        "\r\n" +
            //        $"{hexString}");

            //         return response;
            //     }

            // }

            if (CompressionHeaders != null && CompressionHeaders.Contains("gzip"))
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] compressedBytes;

                using (var compressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(messageBytes, 0, messageBytes.Length);
                    }
                    compressedBytes = compressedStream.ToArray();
                }

                // Create the HTTP header as a string
                string responseHeaders =
                    "HTTP/1.1 200 OK\r\n" +
                    "Content-Encoding: gzip\r\n" +
                    "Content-Type: text/plain\r\n" +
                    $"Content-Length: {compressedBytes.Length}\r\n" +
                    "\r\n";

                // Convert the headers to bytes
                byte[] headerBytes = Encoding.UTF8.GetBytes(responseHeaders);

                // Combine the header bytes and the compressed body bytes
                response = new byte[headerBytes.Length + compressedBytes.Length];
                Buffer.BlockCopy(headerBytes, 0, response, 0, headerBytes.Length);
                Buffer.BlockCopy(compressedBytes, 0, response, headerBytes.Length, compressedBytes.Length);

                return response;
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

