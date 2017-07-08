using Emerald.Net.TCP.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Emerald.Examples.Net.TCP.Client
{
    class Client
    {
        public Client()
        {
            var client = new Emerald.Net.TCP.Client.Client();

            client.ConnectedEvent += OnConnectedEvent;
            client.DataReceived += OnDataReceived;

            client.Connect("localhost", 80);

            while (Console.ReadLine() != "q") { /* Stay open. */ }
        }

        public void OnConnectedEvent(Emerald.Net.TCP.Client.Client instance)
        {
            Console.WriteLine($"Connected to {instance.RemoteEndPoint} !");
        }

        public void OnDataReceived(Emerald.Net.TCP.Client.Client instance, byte[] data)
        {
            Console.WriteLine($"[Server] > {Encoding.ASCII.GetString(data)}");

            instance.Send(Encoding.ASCII.GetBytes("You sent: ").Add(data));
;        }
    }
}
