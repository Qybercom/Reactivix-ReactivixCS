using System.Text;
using System.Net.Sockets;

namespace Reactivix.Network.Transports
{
    public class ReactivixNetworkTransportTCP : IReactivixNetworkTransport
    {
        public bool Connected { get { return _socket == null || _socket.Client == null ? false : _socket.Client.Connected; } }

        private TcpClient _socket;

        public ReactivixNetworkTransportTCP()
        {
        }

        public bool Connect(string host, int port)
        {
            _socket = new TcpClient();

            _socket.Connect(host, port);
            _socket.ReceiveTimeout = 0;
            _socket.Client.Blocking = false;

            return true;
        }

        public bool Send(string data)
        {
            if (_socket == null) return false;

            byte[] buffer = Encoding.UTF8.GetBytes(data);

            _socket.Client.Blocking = true;
            _socket.Client.Send(buffer);
            _socket.Client.Blocking = false;

            return true;
        }

        public string Receive()
        {
            if (_socket == null || _socket.Available == 0) return "";

            byte[] buffer = new byte[_socket.Available];

            _socket.Client.Blocking = true;
            _socket.Client.Receive(buffer);
            _socket.Client.Blocking = false;

            return buffer.Length == 0 ? "" : Encoding.UTF8.GetString(buffer);
        }

        public bool Close()
        {
            if (_socket == null) return false;

            _socket.Close();
            _socket = null;

            return true;
        }
    }
}