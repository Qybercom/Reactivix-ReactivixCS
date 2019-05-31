using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix.OAuth
{
    public class OAuthRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }

    public class OAuthRequestFlowPasswordCredentials : OAuthRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
}