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

using System.Collections.Specialized;
using System.Net;
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server.EmbedIO;

public class WebServerHttpContext
{
    public IHttpContext HttpContext { get; }
    public IHttpException? HttpException { get; }


    public NameValueCollection RequestParameters { get; }
    public NameValueCollection RequestHeaders { get; }

    public WebUrlPath RequestPath { get; }

    public string? MethodRaw { get; }

    public HttpRequestMethod? Method { get; }

    public string? RequestAuthorization => this.GetRequestHeaders(HttpRequestHeader.Authorization).TrimOrNull().WhereNotNull().FirstOrDefault();


    public WebServerHttpContext(IHttpContext httpContext, IHttpException? httpException = null)
    {
        HttpContext = httpContext;
        HttpException = httpException;

        RequestPath = new(HttpContext.RequestedPath);

        RequestParameters = HttpContext.GetRequestQueryData();

        RequestHeaders = HttpContext.Request.Headers;

        MethodRaw = httpContext.Request.HttpMethod.TrimOrNull();
        if (MethodRaw != null && Enum.TryParse<HttpRequestMethod>(MethodRaw, true, out var method)) Method = method;
    }
}
