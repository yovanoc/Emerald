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
            Accept(CreateAcceptSocket());

            // Let the mutex block the function to one thread.
            _listenerMutex.WaitOne();
        }

        private SocketAsyncEventArgs CreateAcceptSocket()
        {
            var socket = new SocketAsyncEventArgs();
            socket.Completed += OnAcceptCompleted;

            return socket;
        }

        private void Accept (SocketAsyncEventArgs acceptArg)
        {
            // As Accept() is called by ProcessAccept() when data processing is done, making a sort of
            // infinite loop we need to clear the old accept socket who contains the old connection.
            acceptArg.AcceptSocket = null;

            var isAcceptPending = AcceptAsync(acceptArg);

            // If the connection is accepted yet, everything is fine let's finish the process.
            // Otherwise, the process will be finished by the socket's Completed callback.
            if (!isAcceptPending) ProcessAccept(acceptArg);
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs acceptArg) => ProcessAccept(acceptArg);

        private void ProcessAccept(SocketAsyncEventArgs acceptArg)
        {
            // Grab the socket from the async event arg.
            var acceptSocket = acceptArg.AcceptSocket;

            // Check that the connection is good to go.
            if (!acceptSocket.Connected) return;

            var readSocket = _socketQueue.Pop();

            // If every socket in the SocketQueue is used, we can't host more clients.
            // TODO: Check if the client will make a new request after a certain time.
            if (readSocket == null) return;

            readSocket.UserToken = new UserToken(owner: acceptSocket);
            var isIOPending = acceptSocket.ReceiveAsync(readSocket);

            // If no data is being sent and/or everything was intercepted, we "extract" the data.
            // Otherwise, 
            if (!isIOPending) ProcessReceive(readSocket);

            // And loop to accept new connections.
            Accept(acceptArg);
        }

        private void OnIOComplete(object sender, SocketAsyncEventArgs readSocket) => ProcessReceive(readSocket);

        private void ProcessReceive(SocketAsyncEventArgs readSocket)
        {
            var bytecount = readSocket.BytesTransferred;
            if (bytecount > 0 || readSocket.SocketError == SocketError.Success)
            {
                Console.WriteLine(Encoding.ASCII.GetString(readSocket.Buffer, readSocket.Offset, bytecount));
            }

            var token = readSocket.UserToken as UserToken;
            var isIOPending = token.OwnerSocket.ReceiveAsync(readSocket);
            if (!isIOPending) ProcessReceive(readSocket);
        }




        # endregion Methods

        # region Events

        /** <summary> Fired when server starts listening </summary> */
        public delegate void ListeningEventHandler(Server server);

        public event ListeningEventHandler Listening;

        # endregion Events

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