using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivix
{
    public interface IReactivixIOProcessor
    {
        string MIME { get; }

        string ProcessorEncode(object data);
        TOut ProcessorDecode<TOut>(string data) where TOut : new();
        object ProcessorDecode(Type type, string data);
    }
}
