using System;

namespace Reactivix.Network
{
    public class ReactivixNetworkSocket
    {
        public IReactivixNetworkTransport Transport { get; private set; }

        public string Host { get; set; }
        public int Port { get; set; }


        public delegate void _onConnect(ReactivixNetworkSocket client);
        public event _onConnect OnConnect;

        public delegate void _onData(ReactivixNetworkSocket client, string data);
        public event _onData OnData;

        public delegate void _onClose(ReactivixNetworkSocket client);
        public event _onClose OnClose;

        public delegate void _onError(ReactivixNetworkSocket client, Exception e);
        public event _onError OnError;


        public ReactivixNetworkSocket(IReactivixNetworkTransport transport, string host = "", int port = 0)
        {
            Transport = transport;

            Host = host;
            Port = port;
        }

        public void Pipe()
        {
            try
            {
                if (Transport.Connected && OnData != null)
                    OnData(this, Transport.Receive());
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, e);
            }
        }

        public bool Connect()
        {
            try
            {
                bool connect = Transport.Connect(Host, Port);

                if (connect && OnConnect != null)
                    OnConnect(this);

                return connect;
            }
            catch (Exception e)
            {
                OnError(this, e);
                return false;
            }
        }

        public bool Send(string data)
        {
            try
            {
                return Transport.Send(data);
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, e);

                return false;
            }
        }

        public bool Close()
        {
            try
            {
                bool close = Transport.Connect(Host, Port);

                if (close && OnClose != null)
                    OnClose(this);

                return close;
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, e);

                return false;
            }
        }
    }
}