using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Reactivix.Network;

namespace Reactivix.Quark
{
    public class QuarkNetworkClient
    {
        public ReactivixNetworkSocket Socket { get; private set; }

        private class Callback
        {
            public string URL { get; set; }
            public QuarkNetworkCallback Worker { get; set; }
            public Type TypeData { get; set; }
        }

        private List<Callback> _responses { get; set; }
        private List<Callback> _events { get; set; }


        public delegate void _onConnect(QuarkNetworkClient client);
        public event _onConnect OnConnect;

        public delegate void _onClose(QuarkNetworkClient client);
        public event _onClose OnClose;

        public delegate void _onError(QuarkNetworkClient client, Exception e);
        public event _onError OnError;


        public QuarkNetworkClient(IReactivixNetworkTransport transport, string host, int port)
        {
            _responses = new List<Callback>();
            _events = new List<Callback>();

            Socket = new ReactivixNetworkSocket(transport, host, port);

            Socket.OnConnect += _socketOnConnect;
            Socket.OnData += _socketOnData;
            Socket.OnClose += _socketOnClose;
            Socket.OnError += _socketOnError;
        }

        private void _socketOnConnect(ReactivixNetworkSocket client)
        {
            OnConnect(this);
        }

        private void _socketOnData(ReactivixNetworkSocket client, string data)
        {
            if (data == "") return;

            string[] separator = new string[] { "}{" };
            List<string> queue = new List<string>(data.Replace("}{", "}}{{").Split(separator, StringSplitOptions.None));

            foreach (string raw in queue)
            {
                QuarkNetworkPacket packet = JsonConvert.DeserializeObject<QuarkNetworkPacket>(raw.Trim());

                if (packet == null) continue;

                if (packet.Response != "") _trigger(_responses, packet, packet.Response);
                if (packet.Event != "") _trigger(_events, packet, packet.Event);
            }
        }

        private void _socketOnClose(ReactivixNetworkSocket client)
        {
            OnClose(this);
        }

        private void _socketOnError(ReactivixNetworkSocket client, Exception e)
        {
            OnError(this, e);
        }


        public bool Connect()
        {
            return Socket.Connect();
        }

        public void Pipe()
        {
            Socket.Pipe();
        }

        public bool Close()
        {
            return Socket.Close();
        }

        private void _subscribe<TData>(List<Callback> callbacks, string url, QuarkNetworkCallback callback)
            where TData : IQuarkNetworkPacketData, new()
        {
            TData data = new TData();

            callbacks.Add(new Callback
            {
                URL = url,
                Worker = callback,
                TypeData = data.GetType()
            });
        }

        private void _trigger(List<Callback> callbacks, QuarkNetworkPacket packet, string url)
        {
            foreach (Callback callback in callbacks)
            {
                if (callback.URL != url) continue;

                packet.TypeData = callback.TypeData;

                callback.Worker(packet);
            }
        }

        public bool Service(string url, IQuarkNetworkPacketData data = null)
        {
            return Socket.Send(JsonConvert.SerializeObject(new QuarkNetworkPacket()
            {
                url = url,
                data = data
            }));
        }

        public void Response<TData>(string url, QuarkNetworkCallback callback)
            where TData : IQuarkNetworkPacketData, new()
        {
            _subscribe<TData>(_responses, url, callback);
        }

        public void Event<TData>(string url, QuarkNetworkCallback callback)
            where TData : IQuarkNetworkPacketData, new()
        {
            _subscribe<TData>(_events, url, callback);
        }
    }
}