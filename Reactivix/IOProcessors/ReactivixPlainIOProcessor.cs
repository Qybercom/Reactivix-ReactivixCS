using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix.IOProcessors
{
    public class ReactivixPlainIOProcessor : IReactivixIOProcessor
    {
        public string MIME { get { return "text/plain"; } }

        public TOut ProcessorDecode<TOut>(string data)
            where TOut : new()
        {
            return new TOut();
        }

        public object ProcessorDecode(Type type, string data)
        {
            return data;
        }

        public string ProcessorEncode(object data)
        {
            return data.ToString();
        }
    }
}
