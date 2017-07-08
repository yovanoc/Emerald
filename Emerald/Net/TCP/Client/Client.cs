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

        /** <summary> An SocketAsyncEventArgs used for receiving data. </summary> */
        private SocketAsyncEventArgs _sentEvent;

        #endregion

        #region Properties

        /**
         * <summary> Test if the instance is connected to any host. </summary>
         * <value>  True if connected, false if not. </value>
         */
        new public bool Connected => base.Connected && this.IsConnected();

        #endregion

        #region Events

        /**
         * <summary> Fired when the listening socket is connected to a given host. </summary>
         * <param name="instance"> The listening socket. </param>
         */
        public delegate void ConnectedEventHandler (Client instance);
        public event ConnectedEventHandler ConnectedEvent;

        /**
         * <summary> Fired when data is received. </summary>
         *
         * <param name="instance">  The listening instance. </param>
         * <param name="data">      The data received. </param>
         */
        public delegate void DataReceivedEventHandler (Client instance, byte[] data);
        public event DataReceivedEventHandler DataReceived;

        /**
         * <summary> Fired when the data was successfully sent. </summary>
         *
         * <param name="instance">  The listening instance. </param>
         * <param name="sendEvent"> The SocketAsyncEventArgs that sent the data. </param>
         */
        public delegate void DataSentEventHandler (Client instance, SocketAsyncEventArgs sendEvent);
        public event DataSentEventHandler DataSent;

        #endregion

        #region Public Methods

        /**
         * <summary> Start the connection to a distant host. </summary>
         *
         * <param name="host">  The host to connect. </param>
         * <param name="port">  The port. </param>
         */
        new public async void Connect (string host, int port)
        {
            // Used as an link to distant host.
            var endPoint = await BuildEndPoint(host, port);

            // Create a new SocketAsyncEventArgs that will handle data reception.
            var receivedEvent = new SocketAsyncEventArgs();
            receivedEvent.RemoteEndPoint = endPoint;

            receivedEvent.SetBuffer(CreateBuffer(), 0, BufferSize);
            receivedEvent.Completed += OnReceived;

            // Instantiate a new SocketAsyncEventArgs that will handle data sending.
            _sentEvent = new SocketAsyncEventArgs();
            _sentEvent.RemoteEndPoint = endPoint;
            _sentEvent.Completed += OnSent;

            // Connect to end point.
            await this.ConnectAsync(endPoint);

            // Fire the connection event.
            ConnectedEvent?.Invoke(this);

            // If no receiving process is pending handle received data
            // otherwise, the handler will be called by the receiveEvent's callback.
            if (!ReceiveAsync(receivedEvent))
                ReceivedProcess(receivedEvent);
        }

        /** <summary> Disconnect the client from distant host. </summary> */
        public void Stop ()
        {
            Shutdown(SocketShutdown.Both);
        }

        /**
         * <summary> Send data to distant host. </summary>
         *
         * <param name="data"> The data to send. </param>
         */
        new public void Send (byte[] data)
        {
            if (!Connected)
                return;

            _sentEvent.SetBuffer(data, 0, data.Length);

            // If no send process is pending, handle the sent data
            // otherwise, the handler will be called by _sentEvent's callback.
            if (!SendAsync(_sentEvent))
                    SentProcess(_sentEvent);
        }

        #endregion

        #region Private Methods

        /**
         * <summary> Called by receivedEvent when all data is received. </summary>
         *
         * <param name="sender">            Source of the event. </param>
         * <param name="receiveEventArgs">  Event information to send to registered event handlers. </param>
         */
        private void OnReceived (object sender, SocketAsyncEventArgs receiveEventArgs) => ReceivedProcess(receiveEventArgs);

        /**
         * <summary> Handle the received data. </summary>
         * <param name="receiveEventArgs"> The receiveEventArgs that received the data. </param>
         */
        private void ReceivedProcess (SocketAsyncEventArgs receiveEventArgs)
        {
            if (!Connected) return;

            // Fire the event with a cropped byte array, that contains the received data.
            if (receiveEventArgs.BytesTransferred > 0)
            {
                byte[] data = new byte[receiveEventArgs.BytesTransferred];
                Array.Copy(receiveEventArgs.Buffer, data, receiveEventArgs.BytesTransferred);
                DataReceived?.Invoke(this, data);
            }

            // Prepare to receive new data.
            ReceiveAsync(receiveEventArgs);
        }

        /**
         * <summary> Called by _sentEvent when all data is sent. </summary>
         * <param name="sender">        Source of the event. </param>
         * <param name="sendEventArgs"> The SocketAsyncEventArgs that sent all the data. </param>
         */
        private void OnSent (object sender, SocketAsyncEventArgs sendEventArgs) => SentProcess(sendEventArgs);

        /**
         * <summary> Handle the sent event. </summary>
         * <param name="sendEventArgs"> The SocketAsyncEventArgs that sent all the data. </param>
         */
        private void SentProcess (SocketAsyncEventArgs sendEventArgs)
        {
            DataSent?.Invoke(this, sendEventArgs);
        }

        #endregion
    }
}