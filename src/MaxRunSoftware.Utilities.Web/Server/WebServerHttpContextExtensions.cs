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
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public static class WebServerHttpContextExtensions
{
    #region Send

    public static Task SendHtmlSimpleAsync(this WebServerHttpContext context, string? title, string? msg, string? css = null)
    {
        static string ConvertToHtml(string prefix, string? content, string suffix) => content == null ? string.Empty : prefix + content + suffix;
        const string n = Constant.NewLine_Unix;
        var titleHtml = ConvertToHtml("<title>", title, "</title>");
        var h1Html = ConvertToHtml("<h1>", title, "</h1>");
        var cssHtml = ConvertToHtml($"<style>{n}", css, $"{n}</h1>");
        var msgHtml = ConvertToHtml("", msg, "");

        var html =
            $"""
             <html>
               <head>
                 <meta charset="utf-8">
                 {titleHtml}
                 {cssHtml}
               </head>
               <body>
                 {h1Html}
                 {msgHtml}
               </body>
             </html>
             """;


        return context.SendHtmlAsync(html);
    }

    public static Task SendHtmlAsync(this WebServerHttpContext context, string html) => context.HttpContext.SendStringAsync(html, "text/html", Constant.Encoding_UTF8_Without_BOM);

    public static void SendFile(this WebServerHttpContext context, byte[] bytes, string fileName)
    {
        context.AddResponseHeader(HttpResponseHeader.Pragma, "public");
        context.AddResponseHeader(HttpResponseHeader.Expires, "0");
        context.AddResponseHeader(HttpResponseHeader.CacheControl, "must-revalidate", "post-check=0", "pre-check=0");
        context.AddResponseHeader(HttpResponseHeader.ContentType, "application/octet-stream");
        context.AddResponseHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        context.AddResponseHeader("Content-Transfer-Encoding", "binary");
        context.AddResponseHeader(HttpResponseHeader.ContentLength, bytes.Length.ToString());

        using var stream = context.HttpContext.OpenResponseStream();
        stream.Write(bytes, 0, bytes.Length);
    }

    public static void SendFile(this WebServerHttpContext context, string data, string fileName, Encoding? encoding = null)
    {
        encoding ??= Constant.Encoding_UTF8_Without_BOM;
        var bytes = encoding.GetBytes(data);
        context.SendFile(bytes, fileName);
    }

    #endregion Send

    #region Request

    #region RequestParameters

    public static IEnumerable<string> GetRequestParameters(this WebServerHttpContext context, string parameterName) => context.RequestParameters.GetValues(parameterName) ?? Array.Empty<string>();

    public static string? GetRequestParameter(this WebServerHttpContext context, string parameterName) => context.GetRequestParameters(parameterName).WhereNotNull().FirstOrDefault();

    public static T? GetRequestParameter<T>(this WebServerHttpContext context, string parameterName)
    {
        var value = context.GetRequestParameter(parameterName);
        return value == null ? default : Util.ChangeType<T>(value);
    }

    public static bool HasRequestParameter(this WebServerHttpContext context, string parameterName) => context.RequestParameters.Keys.Cast<string>().Contains(parameterName, StringComparer.OrdinalIgnoreCase);

    #endregion RequestParameters

    #region RequestHeaders

    public static IEnumerable<string> GetRequestHeaders(this WebServerHttpContext context, string headerName) => context.RequestHeaders.GetValues(headerName) ?? Array.Empty<string>();

    public static IEnumerable<string> GetRequestHeaders(this WebServerHttpContext context, HttpRequestHeader header) => context.GetRequestHeaders(header.GetName());

    public static string? GetRequestHeader(this WebServerHttpContext context, string headerName) => context.GetRequestHeaders(headerName).WhereNotNull().FirstOrDefault();

    public static string? GetRequestHeader(this WebServerHttpContext context, HttpRequestHeader header) => context.GetRequestHeader(header.GetName());

    public static T? GetRequestHeader<T>(this WebServerHttpContext context, string headerName)
    {
        var value = context.GetRequestHeader(headerName);
        return value == null ? default : Util.ChangeType<T>(value);
    }

    public static T? GetRequestHeader<T>(this WebServerHttpContext context, HttpRequestHeader header) => context.GetRequestHeader<T>(header.GetName());

    public static bool HasRequestHeader(this WebServerHttpContext context, string headerName) => context.RequestHeaders.Keys.Cast<string>().Contains(headerName, StringComparer.OrdinalIgnoreCase);

    public static bool HasRequestHeader(this WebServerHttpContext context, HttpRequestHeader header) => context.HasRequestHeader(header.GetName());

    #endregion RequestHeaders

    #endregion Request

    #region Response

    #region ResponseHeaders

    public static void AddResponseHeader(this WebServerHttpContext context, string header, params string[] values) => context.HttpContext.Response.Headers.Add(header, values.ToStringDelimited(", "));

    public static void AddResponseHeader(this WebServerHttpContext context, HttpResponseHeader header, params string[] values) => context.AddResponseHeader(header.GetName(), values);

    public static void SetResponseHeader(this WebServerHttpContext context, string header, params string[] values) => context.HttpContext.Response.Headers.Set(header, values.Length == 0 ? null : values.ToStringDelimited(", "));

    public static void SetResponseHeader(this WebServerHttpContext context, HttpResponseHeader header, params string[] values) => context.AddResponseHeader(header.GetName(), values);

    #endregion ResponseHeaders

    public static void SetResponseStatus(this WebServerHttpContext context, int code)
    {
        context.HttpContext.Response.StatusCode = code;
        if (Constant.HttpStatusCodeInt_HttpStatusDescription.TryGetValue(code, out var desc))
        {
            context.HttpContext.Response.StatusDescription = desc;
        }
    }

    #endregion Response

    #region Authorization

    public static string? GetRequestAuthorizationScheme(this WebServerHttpContext context) => Util.WebHeaderParseAuthorization(context.RequestAuthorization).scheme;

    public static string? GetRequestAuthorizationParameters(this WebServerHttpContext context) => Util.WebHeaderParseAuthorization(context.RequestAuthorization).parameters;

    public static (string? username, string? password) GetRequestAuthorizationBasic(this WebServerHttpContext context) => Util.WebHeaderParseAuthorizationBasic(context.RequestAuthorization);

    public static string? GetRequestAuthorizationBearer(this WebServerHttpContext context)
    {
        var sp = Util.WebHeaderParseAuthorization(context.RequestAuthorization);
        return sp.scheme.EqualsOrdinalIgnoreCase("bearer") ? sp.parameters : null;
    }

    #endregion Authorization
}
