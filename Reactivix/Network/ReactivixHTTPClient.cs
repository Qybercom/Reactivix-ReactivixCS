using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Reactivix.Network.Transports;
using Reactivix.IOProcessors;

namespace Reactivix.Network
{
    public class ReactivixHTTPClient
    {
        public const string SCHEME_HTTP = "http";
        public const string SCHEME_HTTPS = "https";
        public const int PORT_HTTP = 80;
        public const int PORT_HTTPS = 443;

        public ReactivixNetworkSocket Socket { get; private set; }
        //public ReactivixURI URI { get; private set; }

        //public ReactivixHTTPPacket Request { get; set; }
        //public ReactivixHTTPPacket Response { get; set; }

        /*
        public delegate void _onResponse(ReactivixHTTPPacket response);
        public event _onResponse OnResponse;

        public delegate void _onError(Exception e);
        public event _onError OnError;
        */

        private List<ReactivixHTTPClientTask> _tasks;
        private ReactivixHTTPClientTask _currentTask;

        public ReactivixHTTPClient()
        {
            _tasks = new List<ReactivixHTTPClientTask>();
            Socket = new ReactivixNetworkSocket(new ReactivixNetworkTransportTCP());

            Socket.OnConnect += _onSocketConnect;
            Socket.OnData += _onSocketData;
            Socket.OnClose += _onSocketClose;
            Socket.OnError += _onSocketError;
        }

        public void SendRequest<TResponse>(string uri, ReactivixHTTPPacket request, ReactivixHTTPPacket response, Action<ReactivixHTTPPacket> onResponse = null, Action<Exception> onError = null)
        {
            ReactivixURI URI = ReactivixURI.FromURI(uri);

            if (URI.Port == 0)
            {
                if (URI.Scheme == SCHEME_HTTP) URI.Port = PORT_HTTP;
                if (URI.Scheme == SCHEME_HTTPS) URI.Port = PORT_HTTPS;
            }

            if (request.Processor == null) request.Processor = new ReactivixFormIOProcessor();
            if (response.Processor == null) response.Processor = new ReactivixPlainIOProcessor();

            request.URI = URI;
            request.Headers.Add(new KeyValuePair<string, string>("Accept", response.Processor.MIME));

            response.URI = URI;
            response.DataType = typeof(TResponse);

            _tasks.Add(new ReactivixHTTPClientTask {
                OnResponse = onResponse,
                OnError = onError,
                Request = request.Copy(),
                Response = response.Copy()
            });
        }

        public void Pipe()
        {
            int i = 0;

            while (i < _tasks.Count)
            {
                _currentTask = _tasks[0];

                Socket.Host = _currentTask.Request.URI.Host;
                Socket.Port = _currentTask.Request.URI.Port;

                if (!Socket.Transport.Connected)
                    Socket.Connect();

                while (!_currentTask.ResponseGot)
                    Socket.Pipe();

                Socket.Close();
                _tasks.RemoveAt(0);

                i++;
            }
        }

        private void _onSocketConnect(ReactivixNetworkSocket client)
        {
            client.Send(_currentTask.Request.SerializeRequest());
        }

        private void _onSocketData(ReactivixNetworkSocket client, string data)
        {
            if (data == "") return;

            _currentTask.ResponseGot = true;
            _currentTask.Response.UnserializeResponse(data);

            if (_currentTask.OnResponse != null)
                _currentTask.OnResponse(_currentTask.Response);
        }

        private void _onSocketClose(ReactivixNetworkSocket client)
        {
        }

        private void _onSocketError(ReactivixNetworkSocket client, Exception e)
        {
            if (_currentTask.OnError != null)
                _currentTask.OnError(e);
        }
    }

    public class ReactivixHTTPClientTask
    {
        public ReactivixHTTPPacket Request { get; set; }
        public ReactivixHTTPPacket Response { get; set; }
        public Action<ReactivixHTTPPacket> OnResponse { get; set; }
        public Action<Exception> OnError { get; set; }
        public bool ResponseGot { get; set; } = false;
    }
}
