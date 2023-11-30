// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

using System.Net.Http;
using System.Threading.Tasks;
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServerHttpContext
{
    private static ImmutableDictionary<string, HttpMethod> HttpMethod_Map_Build()
    {
        var d = ImmutableDictionary.CreateBuilder<string, HttpMethod>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in typeof(HttpMethod).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (!p.CanRead) continue;
            if (!p.PropertyType.IsAssignableTo<HttpMethod>()) continue;

            var mo = p.GetValue(null);
            if (mo == null) continue;
            var m = mo as HttpMethod;
            if (m == null) continue;
            d.Add(m.Method, m);
        }

        return d.ToImmutable();
    }

    private static readonly ImmutableDictionary<string, HttpMethod> HttpMethod_Map = HttpMethod_Map_Build();

    public IHttpContext HttpContext { get; }

    public WebUrlPath RequestPath { get; }

    private readonly Lzy<IReadOnlyDictionary<string, string?>> parameters;
    public IReadOnlyDictionary<string, string?> Parameters => parameters.Value;
    private IReadOnlyDictionary<string, string?> Parameters_Create()
    {
        var d = new Dictionary<string, string?>();
        var ps = HttpContext.GetRequestQueryData();
        foreach (var key in ps.AllKeys)
        {
            var k = key.TrimOrNull();
            if (k == null) continue;

            var v = ps[key];
            if (v == null) continue;

            d[k] = v;
        }

        return new DictionaryReadOnlyStringCaseInsensitive<string?>(d);
    }

    private readonly Lzy<WebServerHttpContextAuthorization?> authorization;
    public WebServerHttpContextAuthorization? Authorization => authorization.Value;
    private WebServerHttpContextAuthorization? Authorization_Create()
    {
        static (string? name, string? value) ParseAuthorization(IHttpContext httpContext)
        {
            var auth = httpContext.Request.Headers["Authorization"].TrimOrNull();
            if (auth == null) return (null, null);

            if (!auth.Contains(' ', StringComparison.Ordinal)) return (auth, null);

            var parts = auth.Split(' ', 2);
            var name = parts.GetAtIndexOrDefault(0).TrimOrNull();
            var val = parts.GetAtIndexOrDefault(1).TrimOrNull();
            return (name, val);
        }

        var (auth, value) = ParseAuthorization(HttpContext);
        if (auth == null) return null;

        if (WebServerHttpContextAuthorizationBasic.CanHandle(auth)) return new WebServerHttpContextAuthorizationBasic(auth, value);

        return new WebServerHttpContextAuthorizationUnknown(auth, value);
    }

    public WebServerHttpContextAuthorizationUnknown? AuthorizationUnknown => Authorization as WebServerHttpContextAuthorizationUnknown;
    public WebServerHttpContextAuthorizationBasic? AuthorizationBasic => Authorization as WebServerHttpContextAuthorizationBasic;

    public HttpMethod Method { get; }

    public WebServerHttpContext(IHttpContext httpContext)
    {
        HttpContext = httpContext;
        RequestPath = new WebUrlPath(HttpContext.RequestedPath);
        parameters = Lzy.Create(Parameters_Create);
        authorization = Lzy.Create(Authorization_Create);

        var method = httpContext.Request.HttpMethod.Trim();
        Method = HttpMethod_Map[method];
    }

    public T? GetParameter<T>(string parameterName) => Parameters.TryGetValue(parameterName, out var value)
        ? Util.ChangeType<T>(value)
        : default;

    public bool HasParameter(string parameterName) => Parameters.TryGetValue(parameterName, out _);


}

public static class WebServerHttpContextExtensions
{
    public static WebServerHttpContext AddHeader(this WebServerHttpContext context, string name, params string[] values)
    {
        context.HttpContext.Response.Headers.Add(name + ": " + values.ToStringDelimited("; "));
        return context;
    }

    public static WebServerHttpContext SendFile(this WebServerHttpContext context, byte[] bytes, string fileName)
    {
        context.AddHeader("Content-Disposition", "attachment", "filename=\"" + fileName + "\"");

        using var stream = context.HttpContext.OpenResponseStream();
        stream.Write(bytes, 0, bytes.Length);

        return context;
    }

    public static WebServerHttpContext SendFile(this WebServerHttpContext context, string data, string fileName, Encoding? encoding = null)
    {
        encoding ??= Constant.Encoding_UTF8_Without_BOM;
        var bytes = encoding.GetBytes(data);
        return SendFile(context, bytes, fileName);
    }

    public static WebServerHttpContext SetStatus(this WebServerHttpContext context, int httpStatusCode)
    {
        context.HttpContext.Response.StatusCode = httpStatusCode;
        return context;
    }

    public static async Task SendStringHtmlSimpleAsync(this WebServerHttpContext context, string? title, string? msg, string? css = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <meta charset=\"utf-8\">");
        if (title != null) sb.AppendLine($"    <title>{title}</title>");

        if (css != null)
        {
            sb.AppendLine("    <style>");
            sb.AppendLine($"    {css}");
            sb.AppendLine("    </style>");
        }

        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");
        if (title != null) sb.AppendLine($"    <h1>{title}</h1>");

        if (msg != null) sb.AppendLine($"    {msg}");

        sb.AppendLine("  </body>");
        sb.AppendLine("</html>");

        var s = sb.ToString();

        await context.HttpContext.SendStringAsync(s, "text/html", Constant.Encoding_UTF8_Without_BOM);


    }
}
