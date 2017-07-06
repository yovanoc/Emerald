using System.Threading;
using Emerald.Net.TCP.Core.BaseSocket;
using Emerald.Net.TCP.Core.SocketQueue;
using System.Net.Sockets;

namespace Emerald.Net.TCP.Server
{
    public class Server : BaseSocket, IServer
    {
        # region Methods

        /** <summary> Fill the SocketQueue of SocketAsyncEventArgs up to his max capacity. </summary> */
        private void FillSocketQueue()
        {
            for (var i = 0; i < _socketQueue.Capacity; i++)
                _socketQueue.Push(CreateSocket());
        }

        /**
         * <summary> Creates and set a new SocketAsyncEventArgs instance. </summary>
         * <returns> The created instance. </returns>
         */
        private SocketAsyncEventArgs CreateSocket()
        {
            var socket = new SocketAsyncEventArgs();
            socket.Completed += OnSocketOperationCompleted;
            socket.SetBuffer(CreateBuffer(), 0, BufferSize);

            return socket;
        }

        private static void OnSocketOperationCompleted(object sender, SocketAsyncEventArgs e)
        {
            // TODO
        }

        /**
         * <summary> Make the server listen to new connections. </summary>
         * <param name="port"> The listening port. </param>
         */
        public new async void Listen(int port)
        {
            // Create a new local end point and listen.
            var endPoint = await BuildEndPoint("localhost", port);
            Bind(endPoint);
            base.Listen(_maxConnectedSockets);

            // Let the mutex block the function to one thread.
            _listenerMutex.WaitOne();

            // Fire the listening event.
            Listening?.Invoke(this);
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