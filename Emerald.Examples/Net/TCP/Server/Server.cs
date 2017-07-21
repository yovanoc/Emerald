using System;
using System.Text;
using Emerald.Net.TCP.Server;

namespace Emerald.Examples.Net.TCP.Server
{
    /** <summary> Run an Emerald.Net TCP server, and assign events </summary> */
    internal class Server
    {
        public Server()
        {
            var server = new Emerald.Net.TCP.Server.Server();

            server.Listening += OnListening;
            server.ClientConnected += OnClientConnected;
            server.ClientDisconnected += OnClientDisconnected;
            server.DataReceived += OnDataReceived;
            
            // You can't bind to ports < 1024 without administrative privileges.
            // Use a port >= 1024 for this purpose. This is a general restriction - you would encounter
            // the same problem on any application if running as an ordinary user in most operating systems.
            server.Listen(1234);

            string input;
            while ((input = Console.ReadLine()) != "q")
                server.Send(server.ConnectedClients[int.Parse(input[0].ToString())],
                    Encoding.ASCII.GetBytes(input.Substring(2)));
        }

        private static void OnListening(Emerald.Net.TCP.Server.Server server)
        {
            Console.WriteLine($"Listening on {server.LocalEndPoint}");
            Console.WriteLine("To send data to a client use the command '<ClientNumber> <Data>'\nPress 'q' to exit." +
                              '\n');
        }

        private static void OnClientConnected(Emerald.Net.TCP.Server.Server server, ClientSystem client)
        {
            Console.WriteLine($"[Server] > Client {client.AcceptSocket.RemoteEndPoint} joined.");
        }

        private static void OnClientDisconnected(Emerald.Net.TCP.Server.Server server, ClientSystem client)
        {
            Console.WriteLine($"{client.RemoteEndPoint} have left.");
        }

        private static void OnDataReceived(Emerald.Net.TCP.Server.Server server, ClientSystem client, byte[] data)
        {
            Console.WriteLine($"[Client:{client.AcceptSocket.RemoteEndPoint}] > {Encoding.ASCII.GetString(data)}");
        }
    }
}