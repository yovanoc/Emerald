using System.Threading;
using Emerald.Net.TCP.Core.BaseSocket;
using Emerald.Net.TCP.Core.SocketQueue;

namespace Emerald.Net.TCP.Server
{
    public class Server : BaseSocket, IServer
    {
        # region Methods

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

        private readonly int _maxConnectedSockets;


        /** <summary> Keep the Listen method under one thread. </summary> */
        private static Mutex _listenerMutex;

        public int ConnectedClients;

        /** <summary> Contains several pre created SocketAsyncEventArgs instances </summary> */
        private SocketQueue _socketQueue;

        # endregion Members

        # region Constructor

        public Server(int maxConnectedSockets, int maxQueuedSockets)
        {
            _maxConnectedSockets = maxConnectedSockets;
            _listenerMutex = new Mutex();
            _socketQueue = new SocketQueue(maxQueuedSockets);
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