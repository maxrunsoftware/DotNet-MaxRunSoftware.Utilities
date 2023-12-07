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

using System.Collections.Specialized;
using System.Net;
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServerHttpContext
{
    public IHttpContext HttpContext { get; }

    public WebUrlPath RequestPath { get; }

    public string? MethodRaw { get; }

    public HttpRequestMethod? Method { get; }

    public string? RequestAuthorization => GetRequestHeaders(HttpRequestHeader.Authorization).TrimOrNull().WhereNotNull().FirstOrDefault();

    public string? RequestAuthorizationScheme => Util.WebHeaderParseAuthorization(RequestAuthorization).scheme;

    public (string? username, string? password) RequestAuthorizationBasic => Util.WebHeaderParseAuthorizationBasic(RequestAuthorization);

    public WebServerHttpContext(IHttpContext httpContext)
    {
        HttpContext = httpContext;
        RequestPath = new(HttpContext.RequestedPath);

        RequestParameters = HttpContext.GetRequestQueryData();

        RequestHeaders = HttpContext.Request.Headers;

        MethodRaw = httpContext.Request.HttpMethod.TrimOrNull();
        if (MethodRaw != null && Enum.TryParse<HttpRequestMethod>(MethodRaw, true, out var method)) Method = method;
    }

    #region RequestParameters

    public NameValueCollection RequestParameters { get; }

    public IEnumerable<string> GetRequestParameters(string parameterName) => RequestParameters.GetValues(parameterName) ?? Array.Empty<string>();

    public string? GetRequestParameter(string parameterName) => GetRequestParameters(parameterName).WhereNotNull().FirstOrDefault();

    public T? GetRequestParameter<T>(string parameterName)
    {
        var value = GetRequestParameter(parameterName);
        return value == null ? default : Util.ChangeType<T>(value);
    }

    public bool HasRequestParameter(string parameterName) => RequestParameters.Keys.Cast<string>().Contains(parameterName, StringComparer.OrdinalIgnoreCase);

    #endregion RequestParameters

    #region RequestHeaders

    public NameValueCollection RequestHeaders { get; }

    public IEnumerable<string> GetRequestHeaders(string headerName) => RequestHeaders.GetValues(headerName) ?? Array.Empty<string>();

    public IEnumerable<string> GetRequestHeaders(HttpRequestHeader header) => GetRequestHeaders(header.GetName());

    public string? GetRequestHeader(string headerName) => GetRequestHeaders(headerName).WhereNotNull().FirstOrDefault();

    public string? GetRequestHeader(HttpRequestHeader header) => GetRequestHeader(header.GetName());

    public T? GetRequestHeader<T>(string headerName)
    {
        var value = GetRequestHeader(headerName);
        return value == null ? default : Util.ChangeType<T>(value);
    }

    public T? GetRequestHeader<T>(HttpRequestHeader header) => GetRequestHeader<T>(header.GetName());

    public bool HasRequestHeader(string headerName) => RequestHeaders.Keys.Cast<string>().Contains(headerName, StringComparer.OrdinalIgnoreCase);

    public bool HasRequestHeader(HttpRequestHeader header) => HasRequestHeader(header.GetName());

    #endregion RequestHeaders

    public void AddResponseHeader(string header, params string[] values)
    {
        HttpContext.Response.Headers.Add(header + ": " + values.ToStringDelimited(", "));
    }

    public void AddResponseHeader(HttpResponseHeader header, params string[] values) => AddResponseHeader(header.GetName(), values);

    public void SendFile(byte[] bytes, string fileName)
    {
        AddResponseHeader(HttpResponseHeader.Pragma, "public");
        AddResponseHeader(HttpResponseHeader.Expires, "0");
        AddResponseHeader(HttpResponseHeader.CacheControl, "must-revalidate", "post-check=0", "pre-check=0");
        AddResponseHeader(HttpResponseHeader.ContentType, "application/octet-stream");
        AddResponseHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        AddResponseHeader("Content-Transfer-Encoding", "binary");
        AddResponseHeader(HttpResponseHeader.ContentLength, bytes.Length.ToString());

        using var stream = HttpContext.OpenResponseStream();
        stream.Write(bytes, 0, bytes.Length);
    }

    public void SendFile(string data, string fileName, Encoding? encoding = null)
    {
        encoding ??= Constant.Encoding_UTF8_Without_BOM;
        var bytes = encoding.GetBytes(data);
        SendFile(bytes, fileName);
    }
}
