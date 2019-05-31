using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix.IOProcessors
{
    public class ReactivixJSONIOProcessor : IReactivixIOProcessor {
        public string MIME { get { return "application/json"; } }

        public TOut ProcessorDecode<TOut>(string data)
            where TOut : new()
        {
            return JsonConvert.DeserializeObject<TOut>(data);
        }

        public object ProcessorDecode(Type type, string data) {
            return JsonConvert.DeserializeObject(data, type);
        }

        public string ProcessorEncode(object data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
