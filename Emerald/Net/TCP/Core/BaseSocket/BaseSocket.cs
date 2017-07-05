using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Emerald.Net.TCP.Core
{

    /** <summary> The socket super-class that contains primitive methods for both client and server. </summary> */
    public class BaseSocket : System.Net.Sockets.Socket
    {
        # region Constructor


        public BaseSocket () : base(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp)
        { }

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
            IPAddress[] ips = await Dns.GetHostAddressesAsync(host);
            IPAddress ipv4 = null;

            foreach (var ip in ips)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) ipv4 = ip; 

            return new IPEndPoint(ipv4, port);
        }

        # endregion Methods
    }
}
