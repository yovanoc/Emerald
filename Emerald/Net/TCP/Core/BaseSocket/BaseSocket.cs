using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Emerald.Net.TCP.Core.BaseSocket
{
    /** <summary> The socket super-class that contains primitive methods for both client and server. </summary> */
    public class BaseSocket : Socket
    {
        # region Constructor

        public BaseSocket() : base(AddressFamily.InterNetwork, SocketType.Stream,
            ProtocolType.Tcp)
        {
        }

        #endregion Constructor

        # region Methods

        /**
         * <summary>    Builds an socket end point. </summary>
         *
         * <param name="host">  The EndPoint's host. </param>
         * <param name="port">  The EndPoint's port. </param>
         *
         * <returns>    The asynchronous result that yields an IPEndPoint. </returns>
         */
        protected async Task<IPEndPoint> BuildEndPoint(string host, int port)
        {
            var ips = await Dns.GetHostAddressesAsync(host);
            IPAddress ipv4 = null;

            foreach (var ip in ips)
                if (ip.AddressFamily == AddressFamily.InterNetwork) ipv4 = ip;

            return new IPEndPoint(ipv4, port);
        }

        # endregion Methods
    }
}