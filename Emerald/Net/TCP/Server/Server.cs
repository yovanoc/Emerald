using System.Threading;
using Emerald.Net.TCP.Core.SocketPool;

namespace Emerald.Net.TCP.Server
{
    internal class Server : IServer
    {
        #region Members

        private const int MaxQueueConnections = 501;

        private static Mutex _listenerMutex;

        public int ConnectedClients;

        private SocketPool _socketPool;

        #endregion Members
    }
}