using Emerald.Net.TCP.Core.BaseSocket;
using Emerald.Net.TCP.Core.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Emerald.Net.TCP.Client
{
    public class Client : BaseSocket, IClient, IDisposable
    {
        #region Fields

        private IPEndPoint _endPoint;
        private Socket _socket;
        protected byte[] ReceiveBuffer;
        private SocketAsyncEventArgs _receivedEvent;
        private SocketAsyncEventArgs _sentEvent;
        protected object Sender;

        #endregion

        #region Properties

        new public bool Connected => _socket != null && _socket.Connected && _socket.IsConnected();

        #endregion

        #region Events

        public delegate void DataReceivedEventHandler (byte[] data);
        public event DataReceivedEventHandler DataReceived;

        public delegate void DataSentEventHandler (byte[] data);
        public event DataSentEventHandler DataSent;

        #endregion

        #region Constructors

        public Client () => Initialize();

        #endregion

        #region Public Methods

        new public async void Connect (string host, int port)
        {
            _endPoint = await BuildEndPoint(host, port);

            _receivedEvent = new SocketAsyncEventArgs
            {
                RemoteEndPoint = _endPoint,
                UserToken = _socket
            };
            _receivedEvent.SetBuffer(ReceiveBuffer, 0, BufferSize);
            _receivedEvent.Completed += OnReceived;

            await _socket.ConnectAsync(_endPoint);

            if (!_socket.ReceiveAsync(_receivedEvent))
                OnReceived(_socket, _receivedEvent);
        }

        public void Stop ()
        {
            _socket?.Shutdown(SocketShutdown.Both);
        }

        new public void Dispose ()
        {
            _socket.Dispose();
        }

        new public void Send (byte[] data)
        {
            if (!Connected)
                return;

            lock (Sender)
            {
                _sentEvent = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = _endPoint,
                    UserToken = _socket
                };
                _sentEvent.SetBuffer(data, 0, data.Length);
                _sentEvent.Completed += OnSent;

                if (!_socket.SendAsync(_sentEvent))
                    OnSent(_socket, _sentEvent);
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void OnReceived (object sender, SocketAsyncEventArgs e)
        {
            do
            {
                if (!Connected)
                    break;
                byte[] data = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, data, e.BytesTransferred);
                DataReceived?.Invoke(data);
            } while (!_socket.ReceiveAsync(e));
        }

        protected virtual void OnSent (object sender, SocketAsyncEventArgs e)
        {
            byte[] data = new byte[e.BytesTransferred];
            Array.Copy(e.Buffer, data, e.BytesTransferred);
            DataSent?.Invoke(data);
        }

        #endregion

        #region Private Methods

        private void Initialize ()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReceiveBuffer = new byte[BufferSize];
        }

        #endregion
    }
}