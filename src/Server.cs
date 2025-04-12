using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

Socket client = server.AcceptSocket();
var response = System.Text.Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
client.Send(response);
