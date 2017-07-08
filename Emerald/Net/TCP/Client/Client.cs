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

        protected byte[] ReceiveBuffer;
        private SocketAsyncEventArgs _sentEvent;
        protected object Sender;

        #endregion

        #region Properties

        new public bool Connected => base.Connected && this.IsConnected();

        #endregion

        #region Events

        public delegate void ConnectedEventHandler (Client instance);
        public event ConnectedEventHandler ConnectedEvent;

        public delegate void DataReceivedEventHandler (Client instance, byte[] data);
        public event DataReceivedEventHandler DataReceived;

        public delegate void DataSentEventHandler (Client instance, SocketAsyncEventArgs sendEvent);
        public event DataSentEventHandler DataSent;

        #endregion

        #region Constructors

        public Client () => Initialize();

        #endregion

        #region Public Methods

        new public async void Connect (string host, int port)
        {
            var endPoint = await BuildEndPoint(host, port);

            var receivedEvent = new SocketAsyncEventArgs();
            receivedEvent.RemoteEndPoint = endPoint;

            receivedEvent.SetBuffer(ReceiveBuffer, 0, BufferSize);
            receivedEvent.Completed += OnReceive;

            _sentEvent = new SocketAsyncEventArgs();
            _sentEvent.RemoteEndPoint = endPoint;
            _sentEvent.Completed += OnSend;

            await this.ConnectAsync(endPoint);

            ConnectedEvent?.Invoke(this);

            if (!ReceiveAsync(receivedEvent))
                ReceiveProcess(receivedEvent);
        }

        public void Stop ()
        {
            Shutdown(SocketShutdown.Both);
        }

        new public void Send (byte[] data)
        {
            if (!Connected)
                return;

            _sentEvent.SetBuffer(data, 0, data.Length);

            if (!SendAsync(_sentEvent))
                    SendProcess(_sentEvent);
        }

        #endregion

        #region Protected Methods

        private void OnReceive (object sender, SocketAsyncEventArgs receiveEventArgs) => ReceiveProcess(receiveEventArgs);

        protected virtual void ReceiveProcess (SocketAsyncEventArgs receiveEventArgs)
        {
            if (!Connected) return;

            if (receiveEventArgs.BytesTransferred > 0)
            {
                byte[] data = new byte[receiveEventArgs.BytesTransferred];
                Array.Copy(receiveEventArgs.Buffer, data, receiveEventArgs.BytesTransferred);
                DataReceived?.Invoke(this, data);
            }

            ReceiveAsync(receiveEventArgs);
        }

        private void OnSend (object sender, SocketAsyncEventArgs sendEventArgs) => SendProcess(sendEventArgs);

        protected virtual void SendProcess (SocketAsyncEventArgs sendEventArgs)
        {
            DataSent?.Invoke(this, sendEventArgs);
        }

        #endregion

        #region Private Methods

        private void Initialize ()
        {
            ReceiveBuffer = new byte[BufferSize];
        }

        #endregion
    }
}