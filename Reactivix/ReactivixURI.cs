using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reactivix
{
    public class ReactivixURI {
        public const string PATTERN_URI = "([a-zA-Z0-9\\-\\+\\.]+)\\:\\/\\/(([^\\s]*?)(\\:([^\\s]*?))?\\@)?([^\\s\\/\\?\\:]+)(\\:([\\d]+))?([^\\s\\?]+)?(\\?([^\\s\\#]*))?(\\#([^\\s]*))?";
        public const string PATTERN_PARAM = "([a-zA-Z0-9_]+(\\[[a-zA-Z0-9_]+\\])*)+=(.*)";
        public const string PATTERN_SPECIAL = "&([a-zA-Z0-9]+);";

        public string Raw { get; set; }
        public string Scheme { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Path { get; set; }
        public string ParamsRaw { get; set; }
        public Dictionary<string, string> Params { get; set; }
        public string FragmentRaw { get; set; }
        public Dictionary<string, string> Fragment { get; set; }

        public ReactivixURI(string raw)
        {
            Raw = raw;
        }

        public string Serialize(bool port = true)
        {
            return Scheme + "://"
                + (Username != "" ? (Username + (Password != "" ? ":" + Password : "") + "@") : "")
                + Host + (port ? ":" + Port : "")
                + (Path == "" ? "/" : Path)
                + SerializeParams()
                + SerializeFragment();
        }

        public string Query { get { return (Path == "" ? "/" : Path) + SerializeParams(); } }

        public string SerializeParams()
        {
            string _params = SerializeFormData(Params);
            return _params == "" ? "" : "?" + _params;
        }

        public string SerializeFragment() {
            string _params = SerializeFormData(Fragment);
            return _params == "" ? "" : "#" + _params;
        }

        public ReactivixURI Copy()
        {
            return new ReactivixURI(Raw) {
                Scheme = Scheme,
                Username = Username,
                Password = Password,
                Host = Host,
                Port = Port,
                Path = Path,
                Params = Params,
                ParamsRaw = ParamsRaw,
                Fragment = Fragment,
                FragmentRaw = FragmentRaw
            };
        }

        public static ReactivixURI FromURI(string raw)
        {
            Regex regex = new Regex(PATTERN_URI, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            Match match = regex.Match(raw);

            if (match.Groups.Count != 14) return null;

            ReactivixURI uri = new ReactivixURI(raw);

            uri.Scheme = match.Groups[1].Value.ToLower();
            uri.Username = match.Groups[3].Value;
            uri.Password = match.Groups[5].Value;
            uri.Host = match.Groups[6].Value.ToLower();
            uri.Port = match.Groups[8].Value == "" ? 0 : int.Parse(match.Groups[8].Value);
            uri.Path = match.Groups[9].Value;
            uri.ParamsRaw = match.Groups[11].Value;
            uri.Params = ParseQueryString(uri.ParamsRaw);
            uri.FragmentRaw = match.Groups[13].Value;
            uri.Fragment = ParseQueryString(uri.FragmentRaw);

            return uri;
        }

        public static string SerializeFormData(Dictionary<string, string> data)
        {
            List<string> _params = new List<string>();
            foreach (KeyValuePair<string, string> param in data)
                _params.Add(param.Key + "=" + param.Value);

            return String.Join("&", _params.ToArray<string>());
        }

        public static Dictionary<string, string> ParseQueryString(string query) {
            string guid = "---" + Guid.NewGuid().ToString() + "---";
            query = Regex.Replace(query, PATTERN_SPECIAL, guid + "$1" + guid);
            string[] _params = query.Split('&');

            Regex regex = new Regex(PATTERN_PARAM, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            Match match = null;
            Dictionary<string, string> matches = new Dictionary<string, string>();

            foreach (string param in _params)
            {
                match = regex.Match(param);

                if (match.Groups.Count == 4)
                {
                    if (matches.ContainsKey(match.Groups[1].Value)) matches[match.Groups[1].Value] = match.Groups[3].Value;
                    else matches.Add(match.Groups[1].Value, match.Groups[3].Value);
                }
            }

            return matches;
        }

        public static NameValueCollection ParseQueryString1(string query)
        {
            string guid = "---" + Guid.NewGuid().ToString() + "---";
            query = Regex.Replace(query, PATTERN_SPECIAL, guid + "$1" + guid);
            string[] _params = query.Split('&');

            NameValueCollection collection = new NameValueCollection();

            Regex regex = new Regex(PATTERN_PARAM, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            Match match = null;
            List<KeyValuePair<string, string>> matches = new List<KeyValuePair<string, string>>();

            foreach (string param in _params)
            {
                match = regex.Match(param);

                if (match.Groups.Count == 3)
                {
                    matches.Add(new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value));
                }
            }

            return collection;
        }
    }
}
