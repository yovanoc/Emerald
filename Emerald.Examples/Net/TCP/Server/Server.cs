using System;

namespace Emerald.Examples.Net.TCP.Server
{

    /** <summary> Run an Emerald.Net TCP server, and assign events </summary> */
    class Server
    {
        public Server()
        {
            Emerald.Net.TCP.Server.Server server = new Emerald.Net.TCP.Server.Server();

            server.Listening += OnListening;

            server.Listen(80);

            string input;
            while ((input = Console.ReadLine()) != "q") ;
        }

        private static void OnListening (Emerald.Net.TCP.Server.Server server)
        {
            Console.WriteLine($"Listening on {server.LocalEndPoint.ToString()}");
            Console.WriteLine("Press 'q' to exit.");
        }
    }
}
