using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Reactivix.Network;
using Reactivix.IOProcessors;
using Reactivix.Quark.Stream;
using Reactivix.OAuth;

namespace Reactivix.Quark
{
    public class QuarkClient
    {
        public ReactivixURI BaseHTTPURI { get; set; }
        public ReactivixURI BaseStreamURI { get; set; }

        public KeyValuePair<string, string> Authorization { get; set; }

        public ReactivixHTTPClient ClientHTTP { get; private set; }
        public QuarkNetworkClient ClientStream { get; private set; }
        public OAuthClient OAuth { get; private set; }
        public ReactivixHTTPPacket ResponsePacket { get; set; }

        public QuarkClient(string uriHTTP, string uriStream, IReactivixNetworkTransport transport)
        {
            BaseHTTPURI = ReactivixURI.FromURI(uriHTTP);
            BaseStreamURI = ReactivixURI.FromURI(uriStream);

            ClientHTTP = new ReactivixHTTPClient();
            ResponsePacket = new ReactivixHTTPPacket(new ReactivixJSONIOProcessor());

            ClientStream = new QuarkNetworkClient(transport, BaseStreamURI.Host, BaseStreamURI.Port);

            OAuth = new OAuthClient(BaseHTTPURI.Copy());
        }

        public void Pipe()
        {
            ClientStream.Pipe();
            ClientHTTP.Pipe();
        }


        public void Authorize(string authKey, string username, string password, Action<OAuthResponse> success = null, Action<OAuthResponse> error = null)
        {
            OAuth.FlowPasswordCredentials(
                username, password,
                (OAuthResponse response) => {
                    Authorization = new KeyValuePair<string, string>(authKey, response.access_token);

                    if (success != null)
                        success(response);
                },
                (OAuthResponse response) => {
                    if (error != null)
                        error(response);
                }
            );
        }



        public void Request<TData>(string uri, ReactivixHTTPPacket request, Action<TData> success)
            where TData : class, new()
        {
            request.Headers.Add(new KeyValuePair<string, string>("Authorization", "Bearer " + Authorization.Value));

            ClientHTTP.SendRequest<TData>(BaseHTTPURI.Serialize() + uri, request, ResponsePacket, (ReactivixHTTPPacket response) => {
                success(response.Data as TData);
            });
        }



        public bool Service(string url, IQuarkNetworkPacketData data = null)
        {
            return ClientStream.Service(url, data);
        }

        public void Response<TData>(string url, QuarkNetworkCallback callback)
            where TData : IQuarkNetworkPacketData, new()
        {
            ClientStream.Response<TData>(url, callback);
        }

        public void Response(IQuarkNetworkPacketData data, string url, QuarkNetworkCallback callback)
        {
            ClientStream.Response(data, url, callback);
        }

        public void Event<TData>(string url, QuarkNetworkCallback callback)
            where TData : IQuarkNetworkPacketData, new()
        {
            ClientStream.Event<TData>(url, callback);
        }

        public void Event(IQuarkNetworkPacketData data, string url, QuarkNetworkCallback callback)
        {
            ClientStream.Event(data, url, callback);
        }

        public void Stream(IQuarkNetworkStreamGeneric stream)
        {
            ClientStream.Stream(stream);
        }
    }
}
