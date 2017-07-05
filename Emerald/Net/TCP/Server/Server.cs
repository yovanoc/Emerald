using Emerald.Net.TCP.Core.SocketPool;
using System.Threading;

namespace Emerald.Net.TCP.Server
{
    class Server : IServer
    {
        #region Members

        private const int MaxQueueConnections = 501;

        private static Mutex ListenerMutex;

        public int ConnectedClients;

        private SocketPool SocketPool;

        #endregion Members


        public Server ()
        {
        }
    }
}
