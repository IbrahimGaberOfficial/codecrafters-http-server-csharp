using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace POSTRequestHandling;
public static class POSTRequestHandler
{
    public static byte[] HandlePOSTRequest(Socket clinet, string[] requestLines, string[] requestLineParts)
    {
        byte[] response;
        string path = requestLineParts[1];
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

        return response;



    }
}

