using System.Threading;
using Emerald.Net.TCP.Core.SocketPool;
using Emerald.Net.TCP.Core;
using System.Net;

namespace Emerald.Net.TCP.Server
{
    public class Server : BaseSocket, IServer
    {
        # region Events

        /** <summary> Fired when server starts listening </summary> */
        public delegate void ListeningEventHandler(Server server);
        public event ListeningEventHandler Listening;

        # endregion Events

        #region Members

        private readonly int _maxConnectedSockets = 1000;


        /** <summary> Keep the Listen method under one thread. </summary> */
        private static Mutex _listenerMutex;

        public int ConnectedClients;

        /** <summary> Contains several pre created SocketAsyncEventArgs instances </summary> */
        private SocketPool _socketPool;

        # endregion Members

        # region Constructor

        public Server(int maxConnectedSockets, int maxQueuedSockets)
        {
            _maxConnectedSockets = maxConnectedSockets;
            _listenerMutex = new Mutex();
            _socketPool = new SocketPool(maxQueuedSockets);
        }

        public Server(int maxConnectedSockets) : this(maxConnectedSockets, 501) { }

        public Server() : this(1000, 501) { }

        #endregion Constructor

        # region Methods


        new public async void Listen(int port)
        {
            // Create a new local end point and listen.
            IPEndPoint endPoint = await BuildEndPoint("localhost", port);
            Bind(endPoint);
            base.Listen(_maxConnectedSockets);

            // Let the mutex block the function to one thread.
            _listenerMutex.WaitOne();

            // Fire the listening event.
            Listening(this);
        }

        # endregion Methods
    }
}