using System;
using System.Net.Sockets;
using System.Text;

namespace Emerald.Examples.Net.TCP.Client
{
    class Client
    {
        public Client()
        {
            var client = new Emerald.Net.TCP.Client.Client();

            client.ConnectedEvent += OnConnectedEvent;
            client.DataSent += OnDataSent;
            client.DataReceived += OnDataReceived;

            client.Connect("localhost", 80);

            string input;
            while ( (input = Console.ReadLine()) != "q") { client.Send(Encoding.ASCII.GetBytes(input)); }
        }
        
        public static void OnConnectedEvent(Emerald.Net.TCP.Client.Client instance)
        {
            Console.WriteLine("Press 'q' to exit." + '\n' +
                $"Connected to {instance.RemoteEndPoint} !" + '\n');
        }

        public static void OnDataReceived(Emerald.Net.TCP.Client.Client instance, byte[] data)
        {
            Console.WriteLine($"[Server] > {Encoding.ASCII.GetString(data)}");
        }

        public static void OnDataSent(Emerald.Net.TCP.Client.Client instance, SocketAsyncEventArgs socket)
        {
            Console.WriteLine($"[Client] > {socket.BytesTransferred} bytes were sent");
        }
    }
}
