using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Reactivix.Quark
{
    public class QuarkNetworkPacket
    {
        public string url { get; set; }
        public string @event { get; set; }
        public string response { get; set; }
        public object data { get; set; }
        public Dictionary<string, string> session { get; set; }

        public string URL { get { return url; } }

        public string Response { get { return response; } }

        public string Event { get { return @event; } }

        public object Data
        {
            get { return JsonConvert.DeserializeObject(Payload(), TypeData); }
            set { data = Data; }
        }

        public Type TypeData { get; set; }

        public string Payload()
        {
            return JsonConvert.SerializeObject(data);
        }

        public TData GetData<TData>()
            where TData : IQuarkNetworkPacketData, new()
        {
            return JsonConvert.DeserializeObject<TData>(Payload());
        }

        public string SetData<TData>(TData data)
            where TData : IQuarkNetworkPacketData, new()
        {
            this.data = data;

            return JsonConvert.SerializeObject(data);
        }
    }
}