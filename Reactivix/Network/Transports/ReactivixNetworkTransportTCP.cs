using System.Text;
using System.Net.Sockets;

namespace Reactivix.Network.Transports
{
    public class ReactivixNetworkTransportTCP : IReactivixNetworkTransport
    {
        public bool Connected { get { return _socket == null ? false : _socket.Client.Connected; } }

        private TcpClient _socket;

        public ReactivixNetworkTransportTCP()
        {
            _socket = new TcpClient();
        }

        public bool Connect(string host, int port)
        {
            _socket.Connect(host, port);
            //Socket.ReceiveTimeout = 0;
            //Socket.Client.Blocking = false;

            return true;
        }

        public string Receive()
        {
            if (_socket == null) return "";

            byte[] buffer = new byte[_socket.Available];
            _socket.Client.Receive(buffer);

            return buffer.Length == 0 ? "" : Encoding.UTF8.GetString(buffer);
        }

        public bool Send(string data)
        {
            if (_socket == null) return false;

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            _socket.Client.Send(buffer);

            return true;
        }

        public bool Close()
        {
            if (_socket == null) return false;

            _socket.Close();

            return true;
        }
    }
}