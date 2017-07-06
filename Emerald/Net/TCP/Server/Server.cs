using System.Threading;
using Emerald.Net.TCP.Core.BaseSocket;
using Emerald.Net.TCP.Core.SocketQueue;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Text;

namespace Emerald.Net.TCP.Server
{
    public sealed class Server : BaseSocket, IServer
    {
        # region Methods

        /** <summary> Fill the SocketQueue of SocketAsyncEventArgs up to his max capacity </summary> */
        private async void FillSocketQueue()
        {
            for (var i = 0; i < _socketQueue.Capacity; i++)
                _socketQueue.Push(await CreateReadSocket());
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
                socket.Completed += OnIOComplete;
                socket.SetBuffer(CreateBuffer(), 0, BufferSize);

                return socket;
            });
        }

        /**
         * <summary> Make the server listen to new connections. </summary>
         * <param name="port"> The listening port. </param>
         */
        public new async void Listen(int port)
        {
            // Create a new local end point and listen; ('this' is the listening socket).
            var endPoint = await BuildEndPoint("localhost", port);
            Bind(endPoint);
            base.Listen(_maxConnectedSockets);

            // Fire the listening event.
            Listening?.Invoke(this);

            // Accept new incoming connections.
            Accept(CreateAcceptSocketArgs());

            // Let the mutex block the function to one thread.
            _listenerMutex.WaitOne();
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
            var readSocketArg = _socketQueue.Pop();

            // If every socket in the SocketQueue is used, we can't host more clients.
            // TODO: Check if the client will make a new request after a certain time.
            if (readSocketArg == null) return;

            // Fire the client connected event
            ClientConnected?.Invoke(acceptSocket);

            readSocketArg.UserToken = new UserToken(owner: acceptSocket);
            var isIOPending = acceptSocket.ReceiveAsync(readSocketArg);

            // If no data is being sent and/or everything was intercepted, we "extract" the data.
            // Otherwise, 
            if (!isIOPending) ProcessReceive(readSocketArg);

            // And loop to accept new connections.
            Accept(acceptArgs);
        }

        private void OnIOComplete(object sender, SocketAsyncEventArgs readSocketArgs) => ProcessReceive(readSocketArgs);

        /**
         * <summary> Called when the data is read, we just copy the buffer from the socketArgs, and send it to event. </summary>
         * <param name="readSocketArgs"> Where the data was received. </param>
         */
        private void ProcessReceive(SocketAsyncEventArgs readSocketArgs)
        {
            var token = readSocketArgs.UserToken as UserToken;
            var bytecount = readSocketArgs.BytesTransferred;

            if (bytecount > 0 || readSocketArgs.SocketError == SocketError.Success)
            {
                byte[] data = new byte[bytecount];
                Buffer.BlockCopy(readSocketArgs.Buffer, readSocketArgs.Offset, data, 0, bytecount);

                DataReceived?.Invoke(token.OwnerSocket, data);
            }

            var isIOPending = token.OwnerSocket.ReceiveAsync(readSocketArgs);
            if (!isIOPending) ProcessReceive(readSocketArgs);
        }

        # endregion Methods

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
        public delegate void ClientConnectedEventHandler(Socket client);
        /**
         * <summary> Delegate for handling DataReceived events. </summary>
         * <param name="client"> The data sender. </param>
         * <param name="data"> The received data. </param>
         */
        public delegate void DataReceivedEventHandler(Socket client, byte[] data);

        /** <summary> Fired when server starts listening. </summary> */
        public event ListeningEventHandler Listening;
        /** <summary> Fired when a new client connects. </summary> */
        public event ClientConnectedEventHandler ClientConnected;
        /** <summary> Fired when server receives data. </summary> */
        public event DataReceivedEventHandler DataReceived;

        #endregion Events

        #region Members

        /** <summary> The maximum connections waiting for being accepted. </summary> */
        private readonly int _maxQueuedConnections;
        private readonly int _maxConnectedSockets; 

        /** <summary> Keep the Listen method under one thread. </summary> */
        private static Mutex _listenerMutex;

        public int ConnectedClients;

        /** <summary> Contains several pre created SocketAsyncEventArgs instances </summary> */
        private readonly SocketQueue _socketQueue;

        # endregion Members

        # region Constructor

        public Server(int maxConnectedSockets, int maxQueuedConnections)
        {
            _maxQueuedConnections = maxQueuedConnections;
            _maxConnectedSockets = maxConnectedSockets;
            _listenerMutex = new Mutex();
            _socketQueue = new SocketQueue(maxQueuedConnections);

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