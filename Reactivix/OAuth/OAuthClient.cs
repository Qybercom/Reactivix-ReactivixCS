using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Reactivix.Network;
using Reactivix.IOProcessors;

namespace Reactivix.OAuth
{
    public class OAuthClient
    {
        public const string ENDPOINT = "/oauth/token";

        public string AppID { get; set; }
        public string AppSecret { get; set; }
        public ReactivixURI URI { get; set; }
        public ReactivixHTTPPacket Request { get; set; }
        public ReactivixHTTPPacket Response { get; set; }

        public ReactivixHTTPClient Client { get; private set; }

        public OAuthClient(ReactivixURI uri, string endpoint = ENDPOINT)
        {
            URI = uri.Copy();
            URI.Path = endpoint;

            Client = new ReactivixHTTPClient();
            Request = ReactivixHTTPPacket.ForPOST(null);
            Request.Processor = new ReactivixFormIOProcessor();
            Response = new ReactivixHTTPPacket(new ReactivixJSONIOProcessor());
        }

        public void Authorize(Action<OAuthResponse> success, Action<OAuthResponse> error)
        {
            Client.SendRequest<OAuthResponse>(URI.Serialize(), Request, Response, (ReactivixHTTPPacket response) => {
                OAuthResponse data = response.Data as OAuthResponse;

                if (data.error != null) error(data);
                else success(data);
            });

            Client.Pipe();
        }

        public void FlowPasswordCredentials(string username, string password, Action<OAuthResponse> success, Action<OAuthResponse> error)
        {
            Request.Data = new OAuthRequestFlowPasswordCredentials()
            {
                client_id = AppID,
                client_secret = AppSecret,
                username = username,
                password = password,
                grant_type = "password"
            };

            Authorize(success, error);
        }

        public void FlowRefreshToken(string refreshToken, Action<OAuthResponse> success, Action<OAuthResponse> error) { }

        //public void FlowClientCredentials() { }
    }
}
