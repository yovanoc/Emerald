using Emerald.Net.TCP.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Emerald.Net.TCP.Server
{
    public class ClientSystem : IDisposable
    {
        private readonly Server _server;
        public readonly Socket AcceptSocket;
        public readonly SocketAsyncEventArgs ReadSocketArg;

        public bool IsConnected => AcceptSocket.Connected && AcceptSocket.IsBound && AcceptSocket.IsConnected();
        public EndPoint RemoteEndPoint => AcceptSocket.RemoteEndPoint;

        public ClientSystem (Server server, Socket acceptSocket, SocketAsyncEventArgs readSocketArg)
        {
            _server = server;
            AcceptSocket = acceptSocket;
            ReadSocketArg = readSocketArg;
        }

        public void Send (byte[] data)
        {
            _server.Send(this, data);
        }

        public void Dispose ()
        {
            AcceptSocket.Shutdown(SocketShutdown.Both);
            _server.SocketQueue.Push(ReadSocketArg);
            _server.ConnectedClients.Remove(this);
        }
    }
}
