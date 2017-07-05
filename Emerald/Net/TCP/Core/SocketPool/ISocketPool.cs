using System.Net.Sockets;

namespace Emerald.Net.TCP.Core.SocketPool
{
    /**
     * <summary>  Stores SocketAsyncEventArgs, and provides methods to handle them.
     * </summary>
     */
    internal interface ISocketPool
    {
        #region Methods

        /**
         * <summary> Pushes an socket onto the queue. </summary>
         * <exception cref="ArgumentNullException"> Thrown when one or more required arguments are null.
         * </exception>
         * <param name="socket"> The socket to push. </param>
         * <returns> True if it succeeds, false if it fails. </returns>
         */
        bool Push(SocketAsyncEventArgs socket);

        /**
         * <summary> Removes and returns the top-of-stack socket. </summary>
         * <returns> The previous top-of-stack socket. </returns>
         */
        SocketAsyncEventArgs Pop();

        /** <summary> Clears the queue to its blank/initial state. </summary> */
        void Clear();

        # endregion Methods
    }
}