using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reactivix.IOProcessors
{
    public class ReactivixFormIOProcessor : IReactivixIOProcessor
    {
        public string MIME { get { return "application/x-www-form-urlencoded"; } }

        public TOut ProcessorDecode<TOut>(string data)
            where TOut : new()
        {
            TOut output = new TOut();

            return output;
        }

        public object ProcessorDecode(Type type, string data)
        {
            object output = Activator.CreateInstance(type);

            Dictionary<string, string> _params = ReactivixURI.ParseQueryString(data);

            foreach (KeyValuePair<string, string> item in _params)
            {
                PropertyInfo property = type.GetProperty(item.Key);
                property.SetValue(output, item.Value, null);
            }

            return output;
        }

        public string ProcessorEncode(object data)
        {
            if (data == null) return "";

            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties();
            Dictionary<string, string> _params = new Dictionary<string, string>();

            foreach (PropertyInfo property in properties)
            {
                object val = property.GetValue(data, null);

                if (val == null) val = "";

                _params[property.Name] = val.ToString();
            }

            return ReactivixURI.SerializeFormData(_params);
        }
    }
}
