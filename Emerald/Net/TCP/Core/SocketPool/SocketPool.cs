using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Emerald.Net.TCP.Core.SocketPool
{
    public class SocketPool : ISocketPool
    {
        # region Members

        /** <summary> The object that queues sockets. </summary> */
        private readonly ConcurrentQueue<SocketAsyncEventArgs> SocketQueue;
        private readonly int QueueCapacity;
        public int Queued { get => SocketQueue.Count; }

        # endregion Members

        #region Constructor

        /**
         * <summary> Create a new SocketPool </summary>
         * <param name="capacity"> The max socket capacity, leave blank if infinite
         * </param>
         */
        public SocketPool (int capacity)
        {
            SocketQueue = new ConcurrentQueue<SocketAsyncEventArgs>();
            QueueCapacity = capacity;
        }

        public SocketPool () : this(-1) { }

        #endregion Constructor

        # region Methods

        public bool Push (SocketAsyncEventArgs socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket", "A null item cannot be" +
                    "added in the socket pool !");

            if (SocketQueue.Count < QueueCapacity || QueueCapacity == -1)
            {
                SocketQueue.Enqueue(socket);
                return true;
            }
            else return false;
        }

        public SocketAsyncEventArgs Pop ()
        {
            if (SocketQueue.Count < 0) return null;

            SocketAsyncEventArgs socket;

            if (!SocketQueue.TryDequeue(out socket))
                return null;
            else
                return socket;
        }

        public void Clear ()
        {
            // TODO: Custom IDisposable interface.
        }

        # endregion Methods
    }
}
