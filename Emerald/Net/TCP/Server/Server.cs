using System.Threading;
using Emerald.Net.TCP.Core.BaseSocket;
using Emerald.Net.TCP.Core.SocketQueue;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Emerald.Net.TCP.Server
{
    public sealed class Server : BaseSocket, IServer
    {
        #region Fields

        /** <summary> The maximum connections waiting for being accepted. </summary> */
        private readonly int _maxQueuedConnections;
        private readonly int _maxConnectedSockets;

        public readonly List<ClientSystem> ConnectedClients;

        /** <summary> Keep the Listen method under one thread. </summary> */
        private static Mutex _listenerMutex;

        /** <summary> Contains several pre created SocketAsyncEventArgs instances </summary> */
        public readonly SocketQueue SocketQueue;

        private readonly SocketAsyncEventArgs _sendEventArgs;

        private IPEndPoint _endPoint;

        # endregion Fields

        # region Events

        /**
         * <summary> Delegate for handling Listening events. </summary>
         * <param name="server"> The listening server. </param>
         */
        public delegate void ListeningEventHandler(Server server);
        /**
         * <summary> Delegate for handling ClientConnected events. </summary>
         * <param name="client"> The client. </param>
         */
        public delegate void ClientConnectedEventHandler(Server server, ClientSystem client);
        /**
         * <summary>    Delegate for handling ClientDeconnected events. </summary>
         * <param name="server">    The server hosting client. </param>
         * <param name="client">    The disconnected client. </param>
         */
        public delegate void ClientDisconnectedEventHandler(Server server, ClientSystem client);
        /**
         * <summary> Delegate for handling DataReceived events. </summary>
         * <param name="client"> The data sender. </param>
         * <param name="data"> The received data. </param>
         */
        public delegate void DataReceivedEventHandler(Server server, ClientSystem client, byte[] data);

        /** <summary> Fired when server starts listening. </summary> */
        public event ListeningEventHandler Listening;
        /** <summary> Fired when a new client connects. </summary> */
        public event ClientConnectedEventHandler ClientConnected;
        /** <summary> Fired when a client disconnect. </summary> */
        public event ClientDisconnectedEventHandler ClientDisconnected;
        /** <summary> Fired when server receives data. </summary> */
        public event DataReceivedEventHandler DataReceived;

        #endregion Events

        #region Public Methods

        /**
         * <summary> Make the server listen to new connections. </summary>
         * <param name="port"> The listening port. </param>
         */
        public new async void Listen (int port)
        {
            // Create a new local end point and listen; ('this' is the listening socket).
            _endPoint = await BuildEndPoint("localhost", port);
            Bind(_endPoint);
            base.Listen(_maxQueuedConnections);

            // Fire the listening event.
            Listening?.Invoke(this);

            // Accept new incoming connections.
            Accept(CreateAcceptSocketArgs());

            // Let the mutex block the function to one thread.
            _listenerMutex.WaitOne();
        }

        public void Send(ClientSystem client, byte[] data)
        {
            _sendEventArgs.SetBuffer(data, 0, data.Length);

            if (!client.AcceptSocket.SendAsync(_sendEventArgs))
                ProcessSent(client);
        }

        #endregion Public Methods  

        #region Private Methods

        /** <summary> Fill the SocketQueue of SocketAsyncEventArgs up to his max capacity </summary> */
        private async void FillSocketQueue()
        {
            for (var i = 0; i < SocketQueue.Capacity; i++)
                SocketQueue.Push(await CreateReadSocket());
        }

        /**
         * <summary> Creates and set a new SocketAsyncEventArgs instance. As this method might be called several
         *           times, better run it asynchronously. </summary>
         * <returns> The created instance. </returns>
         */
        private async Task<SocketAsyncEventArgs> CreateReadSocket()
        {
            return await Task.Run(() =>
            {
                var socket = new SocketAsyncEventArgs();
                socket.SetBuffer(CreateBuffer(), 0, BufferSize);

                return socket;
            });
        }

        private SocketAsyncEventArgs CreateAcceptSocketArgs()
        {
            var socket = new SocketAsyncEventArgs();
            socket.Completed += OnAcceptCompleted;

            return socket;
        }

        private void Accept (SocketAsyncEventArgs acceptArgs)
        {
            // As Accept() is called by ProcessAccept() when data processing is done, making a sort of
            // infinite loop we need to clear the old accept socket who contains the old connection.
            acceptArgs.AcceptSocket = null;

            var isAcceptPending = AcceptAsync(acceptArgs);

            // If the connection is accepted yet, everything is fine let's finish the process.
            // Otherwise, the process may be slow, or no other connections are made, the process will be finished by the socket's Completed callback.
            if (!isAcceptPending) ProcessAccept(acceptArgs);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs acceptArgs) => ProcessAccept(acceptArgs);

        private void ProcessAccept(SocketAsyncEventArgs acceptArgs)
        {
            // Grab the socket from the async event arg.
            var acceptSocket = acceptArgs.AcceptSocket;
            
            // Check if there is a client
            if (!acceptSocket.Connected) return;
            var readSocketArg = SocketQueue.Pop();

            // If every socket in the SocketQueue is used, we can't host more clients.
            // TODO: Check if the client will make a new request after a certain time.

            ClientSystem clientSystem = new ClientSystem(this, acceptSocket, readSocketArg);
            clientSystem.ReadSocketArg.Completed += (object sender, SocketAsyncEventArgs eventArgs) => OnIOComplete(sender, clientSystem);

            ConnectedClients.Add(clientSystem);

            // Fire the client connected event
            ClientConnected?.Invoke(this, clientSystem);
            Console.WriteLine($"{ConnectedClients.Count} clients connected, { SocketQueue.Queued } free places.");

            var isIOPending = acceptSocket.ReceiveAsync(readSocketArg);

            // If no data is being sent and/or everything was intercepted, we "extract" the data.
            // Otherwise, 
            if (!isIOPending) ProcessReceive(clientSystem);
            
            // And loop to accept new connections.
            Accept(acceptArgs);
        }

        private void OnIOComplete(object sender, ClientSystem clientSystem) => ProcessReceive(clientSystem);

        /**
         * <summary> Called when the data is read, we just copy the buffer from the socketArgs, and send it to event. </summary>
         * <param name="readSocketArgs"> Where the data was received. </param>
         */
        private void ProcessReceive(ClientSystem client)
        {
            // If client is no longer connected, proceed to clean closing.
            if (!client.IsConnected)
            {
                client.Dispose();
                ClientDisconnected?.Invoke(this, client);
                return;
            }

            var readSocketArgs = client.ReadSocketArg;
            var bytecount = readSocketArgs.BytesTransferred;

            if (bytecount > 0 || readSocketArgs.SocketError == SocketError.Success)
            {
                byte[] data = new byte[bytecount];
                Buffer.BlockCopy(readSocketArgs.Buffer, readSocketArgs.Offset, data, 0, bytecount);

                DataReceived?.Invoke(this, client, data);
            }

            var isIOPending = client.AcceptSocket.ReceiveAsync(readSocketArgs);
            if (!isIOPending) ProcessReceive(client);
        }

        private void ProcessSent(ClientSystem clientSystem)
        {

        }

        # endregion Private Methods

        # region Constructor

        public Server(int maxConnectedSockets, int maxQueuedConnections)
        {
            _maxQueuedConnections = maxQueuedConnections;
            _maxConnectedSockets = maxConnectedSockets;
            ConnectedClients = new List<ClientSystem>(maxConnectedSockets);
            _listenerMutex = new Mutex();
            SocketQueue = new SocketQueue(maxConnectedSockets);
            _sendEventArgs = new SocketAsyncEventArgs();

            FillSocketQueue();
        }

        public Server(int maxConnectedSockets) : this(maxConnectedSockets, 501)
        {
        }

        public Server() : this(1000, 501)
        {
        }

        #endregion Constructor
    }
}