using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Socket client = server.AcceptSocket();

// Read the request from the client
byte[] buffer = new byte[1024];
int bytesRead = client.Receive(buffer);

// Convert the byte array to a string
string request = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
var requestLines = request.Split("\r\n");
var requestLineParts = requestLines[0].Split(" ", 3); // get the path from the request line
var path = requestLineParts[1];

byte[] response;

if(path == "/"){
    // return a 200 OK response
    response = System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
}
else {
    // return a 404 Not Found response
    response = System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
    
}

client.Send(response);

client.Close();
server.Stop();