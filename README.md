# Emerald
Emerald is a C# "framework" who aims to gives a (Node)JS way of doing things by providing utilities such as `TCPClient` which is the only module for now.

# Example
Let's say you want to build a TCP Echo Server, with node.js you would do it as follow:
```js
net = require('net');

net.createServer(function (socket) {
  socket.on('data', function (data) {
    socket.write(data);
  });
}).listen(5000);
```
Right, now, the cleaner way to achieve it with .NET Standard 2.0 is:
```cs
using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Text;  
using System.Threading;  

// State object for reading client data asynchronously  
public class StateObject {  
    public Socket workSocket = null;  
    public const int BufferSize = 1024;  
    public byte[] buffer = new byte[BufferSize];  
    public StringBuilder sb = new StringBuilder();    
}  

public class AsynchronousSocketListener {  
    public static ManualResetEvent allDone = new ManualResetEvent(false);  

    public AsynchronousSocketListener() {  
    }  

    public static void StartListening() {  
        byte[] bytes = new Byte[1024];  

        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());  
        IPAddress ipAddress = ipHostInfo.AddressList[0];  
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5000);  

        Socket listener = new Socket(AddressFamily.InterNetwork,  
            SocketType.Stream, ProtocolType.Tcp );  

        try {  
            listener.Bind(localEndPoint);  
            listener.Listen(100);  

            while (true) {  
                allDone.Reset();  

                listener.BeginAccept(   
                    new AsyncCallback(AcceptCallback),  
                    listener );  

                allDone.WaitOne();  
            }  

        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  

        Console.WriteLine("\nPress ENTER to continue...");  
        Console.Read();  

    }  

    public static void AcceptCallback(IAsyncResult ar) {  
        allDone.Set();  

        Socket listener = (Socket) ar.AsyncState;  
        Socket handler = listener.EndAccept(ar);  

        StateObject state = new StateObject();  
        state.workSocket = handler;  
        handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
            new AsyncCallback(ReadCallback), state);  
    }  

    public static void ReadCallback(IAsyncResult ar) {  
        String content = String.Empty;  

        StateObject state = (StateObject) ar.AsyncState;  
        Socket handler = state.workSocket;  

        int bytesRead = handler.EndReceive(ar);  

        if (bytesRead > 0) {  
            state.sb.Append(Encoding.ASCII.GetString(  
                state.buffer,0,bytesRead));  

            content = state.sb.ToString();  
            if (content.IndexOf("<EOF>") > -1) {  
                    content.Length, content );  
                Send(handler, content);  
            } else {  
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
                new AsyncCallback(ReadCallback), state);  
            }  
        }  
    }  

    private static void Send(Socket handler, String data) {  
        byte[] byteData = Encoding.ASCII.GetBytes(data);  

        handler.BeginSend(byteData, 0, byteData.Length, 0,  
            new AsyncCallback(SendCallback), handler);  
    }  

    private static void SendCallback(IAsyncResult ar) {  
        try {  
            Socket handler = (Socket) ar.AsyncState;  

            int bytesSent = handler.EndSend(ar);  

            handler.Shutdown(SocketShutdown.Both);  
            handler.Close();  

        } catch (Exception e) {  
            Console.WriteLine(e.ToString());  
        }  
    }  

    public static int Main(String[] args) {  
        StartListening();  
        return 0;  
    }  
} 
```
Exhaustive isn't it ? Now let's take a look at Emerald TCP module ;)

```cs
using System;
using System.Text;
using Emerald.Net.TCP.Server;
using Emerald.Net.TCP.Server.Server as EmeraldServer;

namespace Emerald.Examples.Net.TCP.Server
{
    internal class Server
    {
        public Server()
        {
            var server = new EmeraldServer();

            server.DataReceived += OnDataReceived;
            
            server.Listen(5000);
        }

        private static void OnDataReceived(EmeraldServer server, ClientSystem client, byte[] data)
        {
            client.send(data);
        }
    }
}
```
As you can see, it's much cleaner, readable, and maintenanble !

# Contribution
Missing JS non verbose API, and you want that API back ?! Just open a new issue describing the module you want to add.  
Want to improve existing code ? open a pull request.
