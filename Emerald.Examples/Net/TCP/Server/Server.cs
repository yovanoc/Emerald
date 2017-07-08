using System;
using System.Net.Sockets;
using System.Text;

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
            server.DataReceived += OnDataReceived;

            server.Listen(80);

            string input;
            while ( (input = Console.ReadLine()) != "q") { server.Send(Encoding.ASCII.GetBytes(input)); }
        }

        private static void OnListening(Emerald.Net.TCP.Server.Server server)
        {
            Console.WriteLine($"Listening on {server.LocalEndPoint}");
            Console.WriteLine("Press 'q' to exit." + '\n');
        }

        private static void OnClientConnected (Emerald.Net.TCP.Server.Server server, Socket client)
        {
            Console.WriteLine($"[Server] > Client {client.RemoteEndPoint} joined.");
        }

        private static void OnDataReceived (Emerald.Net.TCP.Server.Server server, Socket client, byte[] data)
        {
            Console.WriteLine($"[Client:{client.RemoteEndPoint}] > {Encoding.ASCII.GetString(data)}");
        }   
    }
}