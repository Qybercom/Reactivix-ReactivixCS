namespace Reactivix.Network
{
    public interface IReactivixNetworkTransport
    {
        bool Connected { get; }

        bool Connect(string host, int port);
        bool Send(string data);
        string Receive();
        bool Close();
    }
}