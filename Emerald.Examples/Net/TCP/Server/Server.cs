using System;

namespace Emerald.Examples.Net.TCP.Server
{
    /** <summary> Run an Emerald.Net TCP server, and assign events </summary> */
    internal class Server
    {
        public Server()
        {
            var server = new Emerald.Net.TCP.Server.Server();

            server.Listening += OnListening;

            server.Listen(80);

            while (Console.ReadLine() != "q")
            {
            }
        }

        private static void OnListening(Emerald.Net.TCP.Server.Server server)
        {
            Console.WriteLine($"Listening on {server.LocalEndPoint}");
            Console.WriteLine("Press 'q' to exit.");
        }
    }
}