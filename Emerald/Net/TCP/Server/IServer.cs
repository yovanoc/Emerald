namespace Emerald.Net.TCP.Server
{
    internal interface IServer
    {
        void Listen(int port);

        void Send(ClientSystem client, byte[] data);
    }
}