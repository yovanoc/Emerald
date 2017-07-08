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

            client.DataReceived += OnDataReceived;

            client.Connect("localhost", 80);

            while (Console.ReadLine() != "q") { /* Stay open. */ }
        }

        public static void OnDataReceived(byte[] data)
        {
            Console.WriteLine($"[Server] > {Encoding.ASCII.GetString(data)}");
;        }
    }
}
