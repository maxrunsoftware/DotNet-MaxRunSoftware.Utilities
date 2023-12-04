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
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public static class HttpContextExtensions
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

    public static WebUrlPath GetRequestPath(this IHttpContext context) => new(context.RequestedPath);

    public static HttpMethod GetRequestMethod(this IHttpContext context) => HttpMethod_Map[context.Request.HttpMethod.Trim()];

    public static Dictionary<string, string?> GetRequestParameters(this IHttpContext context)
    {
        var d = new Dictionary<string, string?>();
        var ps = context.GetRequestQueryData();
        foreach (var key in ps.AllKeys.WhereNotNull())
        {
            var k = key.TrimOrNull();
            if (k == null) continue;

            var v = ps[key];

            d[k] = v;
        }

        return d;
    }

    public static string? GetRequestParameter(this IHttpContext context, string parameterName)
    {
        var ps = context.GetRequestQueryData();
        var keys = ps.AllKeys.WhereNotNull().ToArray();
        foreach (var sc in new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase })
        {
            foreach (var key in keys)
            {
                var k = key.TrimOrNull();
                if (k == null) continue;
                if (!sc.Equals(parameterName, k)) continue;
                return ps[key];
            }
        }

        return null;
    }

    public static T? GetRequestParameter<T>(this IHttpContext context, string parameterName) => Util.ChangeType<T>(context.GetRequestParameter(parameterName));

    public static bool HasRequestParameter(this IHttpContext context, string parameterName)
    {
        var ps = context.GetRequestQueryData();
        var keys = ps.AllKeys.WhereNotNull().ToArray();
        foreach (var sc in new[] { StringComparer.Ordinal, StringComparer.OrdinalIgnoreCase })
        {
            foreach (var key in keys)
            {
                var k = key.TrimOrNull();
                if (k == null) continue;
                if (!sc.Equals(parameterName, k)) continue;
                return true;
            }
        }

        return false;
    }

    public static (string? scheme, string? parameters)? GetRequestHeaderAuthorization(this IHttpContext context)
    {
        // https://stackoverflow.com/a/25855713
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization

        var header = context.Request.Headers["Authorization"].TrimOrNull();

        if (header == null) return null;
        var parts = header.Split(' ', 2);
        var scheme = parts.GetAtIndexOrDefault(0).TrimOrNull();
        var parameters = parts.GetAtIndexOrDefault(1).TrimOrNull();

        return (scheme, parameters);
    }

    public static (string? username, string? password)? GetRequestHeaderAuthorizationBasic(this IHttpContext context)
    {
        // https://stackoverflow.com/a/25855713
        // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization

        var sp = context.GetRequestHeaderAuthorization();
        var scheme = sp?.scheme;
        var parameters = sp?.parameters;

        if (!scheme.IsEqualOrdinalIgnoreCase("basic")) return null;
        if (parameters == null) return (null, null);

        var encoding = Constant.Encoding_UTF8_Without_BOM;  // Encoding.GetEncoding("iso-8859-1");
        var usernamePassword = encoding.GetString(Convert.FromBase64String(parameters));

        var parts = usernamePassword.Split(':', 2);
        return (parts.GetAtIndexOrDefault(0).TrimOrNull(), parts.GetAtIndexOrDefault(1).TrimOrNull());
    }
}
