using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reactivix.Network
{
    public class ReactivixHTTPPacket {
        public const string HTTP_VERSION_1_0 = "HTTP/1.0";
        public const string HTTP_VERSION_1_1 = "HTTP/1.1";

        public const string HTTP_PROTOCOL_REQUEST = "#^([^\r\n]*) ([^\r\n]*) ([^\r\n]*)\r?\n((?:[^\r\n]+\r?\n)*)\r?\n(.*?)#Uis";
        public const string HTTP_PROTOCOL_RESPONSE = "^([^\r\n\\s]+) ([^\r\n]*)\r?\n((?:[^\r\n]+\r?\n)*)\r?\n(.*)";

        public const string METHOD_GET = "GET";
        public const string METHOD_POST = "POST";
        public const string METHOD_PUT = "PUT";
        public const string METHOD_PATCH = "PATCH";
        public const string METHOD_DELETE = "DELETE";

        public const string STATUS_101_SWITCHING_PROTOCOLS = "101 Switching Protocols";
        public const string STATUS_200_OK = "200 OK";
        public const string STATUS_201_CREATED = "201 Created";
        public const string STATUS_202_ACCEPTED = "202 Accepted";
        public const string STATUS_302_FOUND = "302 Found";
        public const string STATUS_400_BAD_REQUEST = "400 Bad Request";
        public const string STATUS_401_UNAUTHORIZED = "401 Unauthorized";
        public const string STATUS_403_FORBIDDEN = "403 Forbidden";
        public const string STATUS_404_NOT_FOUND = "404 Not Found";
        public const string STATUS_500_SERVER_ERROR = "500 Server Error";

        public ReactivixURI URI { get; set; }
        public string Method { get; set; } = METHOD_GET;
        public string Status { get; set; } = STATUS_200_OK;
        public string Version { get; set; } = HTTP_VERSION_1_0;
        public List<KeyValuePair<string, string>> Headers { get; set; } = new List<KeyValuePair<string, string>>();

        public string Raw { get; set; }
        public string RawData { get; set; }

        public IReactivixIOProcessor Processor { get; set; }
        public Type DataType { get; set; }
        public object Data { get; set; }

        public ReactivixHTTPPacket() { }

        public ReactivixHTTPPacket(IReactivixIOProcessor processor)
        {
            Processor = processor;
        }

        public ReactivixHTTPPacket Copy()
        {
            return new ReactivixHTTPPacket()
            {
                URI = URI.Copy(),
                Method = Method,
                Status = Status,
                Version = Version,
                Headers = Headers,
                Raw = Raw,
                RawData = RawData,
                Processor = Processor,
                DataType = DataType,
                Data = Data
            };
        }

        public string SerializeRequest()
        {
            string output = Method.ToUpper() + " " + URI.Query + " " + Version + "\r\n";

            List<KeyValuePair<string, string>> headers = Headers;
            headers.Add(new KeyValuePair<string, string>("Host", URI.Host));

            if (Processor != null)
            {
                RawData = Processor.ProcessorEncode(Data);

                headers.Add(new KeyValuePair<string, string>("Content-Type", Processor.MIME));
                headers.Add(new KeyValuePair<string, string>("Content-Length", RawData.Length.ToString()));
            }

            foreach (KeyValuePair<string, string> header in headers)
                output += header.Key + ": " + header.Value + "\r\n";

            Raw = output + "\r\n" + RawData;

            return Raw;
        }

        public ReactivixHTTPPacket UnserializeResponse(string raw)
        {
            Raw = raw;

            Regex regex = new Regex(HTTP_PROTOCOL_RESPONSE);
            Match match = regex.Match(Raw);

            Version = match.Groups[1].Value;
            Status = match.Groups[2].Value;

            string[] headers = match.Groups[3].Value.Split("\r\n".ToArray<char>());

            foreach (string header in headers)
            {
                if (header.Length == 0) continue;

                string[] pair = header.Split(':');

                Headers.Add(new KeyValuePair<string, string>(
                    pair[0].Trim(),
                    pair.Length == 1 ? "" : pair[1].Trim()
                ));
            }

            RawData = match.Groups[4].Value;

            if (DataType != null)
                Data = Processor.ProcessorDecode(DataType, RawData);

            return this;
        }

        public static ReactivixHTTPPacket ForGET()
        {
            ReactivixHTTPPacket packet = new ReactivixHTTPPacket();
            packet.Method = METHOD_GET;

            return packet;
        }

        public static ReactivixHTTPPacket ForPOST(object data)
        {
            ReactivixHTTPPacket packet = new ReactivixHTTPPacket();
            packet.Method = METHOD_POST;
            packet.Data = data;

            return packet;
        }

        public static ReactivixHTTPPacket ForMethod(string method, object data = null)
        {
            ReactivixHTTPPacket packet = new ReactivixHTTPPacket();
            packet.Method = method;
            packet.Data = data;

            return packet;
        }
    }
}
