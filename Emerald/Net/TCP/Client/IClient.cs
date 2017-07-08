using System.Threading.Tasks;

namespace Emerald.Net.TCP.Client
{
    public interface IClient
    {
        /// <summary>
        ///     Start the connection to the server
        /// </summary>
        /// <returns></returns>
        void Connect (string host, int port);


        /// <summary>
        ///     Close the connection
        /// </summary>
        void Stop ();

        /// <summary>
        ///     Dispose all resources used by the client
        /// </summary>
        void Dispose ();

        /// <summary>
        ///     Send data to the server
        /// </summary>
        /// <param name="data"></param>
        void Send (byte[] data);

    }
}
