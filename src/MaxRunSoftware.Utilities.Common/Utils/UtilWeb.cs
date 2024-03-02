// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Net;

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    public static string? WebParseFilename(string url)
    {
        var outputFile = url.Split('/').TrimOrNull().WhereNotNull().LastOrDefault();
        if (outputFile != null)
        {
            outputFile = FilenameSanitize(outputFile, "_");
        }

        return outputFile;
    }

    public sealed class WebResponse
    {
        public string Url { get; }
        public byte[]? Data { get; }
        public IDictionary<string, List<string>> Headers { get; }

        public string? ContentType
        {
            get
            {
                if (Headers.TryGetValue("Content-Type", out var list))
                {
                    if (list.Count > 0) return list[0];
                }

                return null;
            }
        }

        public WebResponse(string url, byte[]? data, WebHeaderCollection? headers)
        {
            Url = url;
            Data = data;
            var d = new SortedDictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (headers != null)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var key = headers.GetKey(i).TrimOrNull();
                    var val = headers.Get(i);
                    if (key != null && val != null && val.TrimOrNull() != null) d.AddToList(key, val);
                }
            }

            Headers = d;
        }

        public override string ToString() => GetType().Name + $"[{Url}] Headers:{Headers.Count} Data:" + (Data == null ? "null" : Data.Length);

        public string? ToStringDetail()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetType().Name);
            sb.AppendLine("\tUrl: " + Url);
            sb.AppendLine("\tData: " + (Data == null ? "null" : Data.Length));
            foreach (var kvp in Headers)
            {
                var key = kvp.Key;
                var valList = kvp.Value;

                if (valList.IsEmpty()) continue;

                if (valList.Count == 1) { sb.AppendLine("\t" + key + ": " + valList[0]); }
                else
                {
                    for (var i = 0; i < valList.Count; i++) sb.AppendLine("\t" + key + "[" + i + "]: " + valList[i]);
                }
            }

            return sb.ToString().TrimOrNull(); // remove trailing newline
        }
    }

    public static WebResponse WebDownload(string url, string? outFilename = null, string? username = null, string? password = null, IDictionary<string, string>? cookies = null)
    {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
        var cli = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete

        using (cli)
        {
            // https://stackoverflow.com/a/7861726
            var sb = new StringBuilder();
            foreach (var kvp in cookies.OrEmpty())
            {
                var name = kvp.Key.TrimOrNull();
                var val = kvp.Value.TrimOrNull();
                if (name == null || val == null) continue;

                if (sb.Length > 0) sb.Append(";");

                sb.Append(kvp.Key + "=" + kvp.Value);
            }

            cli.Headers.Add(HttpRequestHeader.Cookie, sb.ToString());

            if (outFilename != null)
            {
                if (outFilename.ContainsAny(Path.DirectorySeparatorChar.ToString(), Path.AltDirectorySeparatorChar.ToString()))
                {
                    var directoryName = Path.GetDirectoryName(outFilename);
                    if (directoryName != null) Directory.CreateDirectory(directoryName);
                }
            }

            try
            {
                byte[]? data = null;
                if (outFilename == null) { data = cli.DownloadData(url); }
                else { cli.DownloadFile(url, outFilename); }

                return new(url, data, cli.ResponseHeaders);
            }
            catch (WebException we)
            {
                if (username == null || password == null || !we.Message.Contains("(401)")) throw;

                // https://stackoverflow.com/a/26016919
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
                cli.Headers[HttpRequestHeader.Authorization] = $"Basic {credentials}";
                byte[]? data = null;
                if (outFilename == null) { data = cli.DownloadData(url); }
                else { cli.DownloadFile(url, outFilename); }

                return new(url, data, cli.ResponseHeaders);
            }
        }
    }

    public static (string? scheme, string? parameters) WebHeaderParseAuthorization(string? headerValue)
    {
        headerValue = headerValue.TrimOrNull();
        if (headerValue == null) return (null, null);

        var authParts = headerValue.SplitOnWhiteSpace(2).Select(o => o.TrimOrNull()).WhereNotNull().ToArray();
        var scheme = authParts.GetAtIndexOrDefault(0);
        var parameters = authParts.GetAtIndexOrDefault(1);
        return (scheme, parameters);
    }

    public static (string? username, string? password) WebHeaderParseAuthorizationBasic(string? headerValue)
    {
        headerValue = headerValue.TrimOrNull();
        if (headerValue == null) return (null, null);

        var containsWhitespace = headerValue.Any(char.IsWhiteSpace);
        if (containsWhitespace) // We got the full Authorization header
        {
            var auth = WebHeaderParseAuthorization(headerValue);
            if (!auth.scheme.EqualsOrdinalIgnoreCase("basic")) return (null, null);
            headerValue = auth.parameters.TrimOrNull();
            if (headerValue == null) return (null, null);
        }

        var userpassEncoded = Convert.FromBase64String(headerValue);
        var userpass = Constant.Encoding_UTF8_Without_BOM.GetString(userpassEncoded).TrimOrNull();
        if (userpass == null) return (null, null);

        var userpassParts = userpass.Split(':', 2);
        var username = userpassParts.GetAtIndexOrDefault(0).TrimOrNull();
        var password = userpassParts.GetAtIndexOrDefault(1).TrimOrNull();

        return (username, password);
    }
}
