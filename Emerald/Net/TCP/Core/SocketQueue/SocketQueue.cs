using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Emerald.Net.TCP.Core.SocketQueue
{
    public class SocketQueue : ISocketQueue
    {
        # region Members

        /** <summary> The object that queues sockets. </summary> */
        private readonly ConcurrentQueue<SocketAsyncEventArgs> _socketQueue;
        private readonly int _queueCapacity;

        public int Capacity => _queueCapacity;
        public int Queued => _socketQueue.Count;

        # endregion Members

        #region Constructor

        /**
         * <summary> Create a new SocketPool </summary>
         * <param name="capacity"> The max socket capacity, leave blank if infinite
         * </param>
         */
        public SocketQueue(int capacity)
        {
            _socketQueue = new ConcurrentQueue<SocketAsyncEventArgs>();
            _queueCapacity = capacity;
        }

        public SocketQueue() : this(-1)
        {
        }

        #endregion Constructor

        # region Methods

        public bool Push(SocketAsyncEventArgs socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket), "A null item cannot be" +
                                                                "added in the socket pool !");

            if (Queued >= _queueCapacity && _queueCapacity != -1) return false;
            _socketQueue.Enqueue(socket);
            return true;
        }

        public SocketAsyncEventArgs Pop()
        {
            if (_socketQueue.Count < 0) return null;
            return !_socketQueue.TryDequeue(out SocketAsyncEventArgs socket) ? null : socket;
        }

        public void Clear()
        {
            // TODO: Custom IDisposable interface.
        }

        # endregion Methods
    }
}