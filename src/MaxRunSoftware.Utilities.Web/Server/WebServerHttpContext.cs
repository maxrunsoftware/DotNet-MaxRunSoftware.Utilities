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

using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServerHttpContext
{
    public IHttpContext HttpContext { get; }

    public WebServerHttpContextAuthorization? Authorization => AuthorizationBasic;

    public WebServerHttpContextAuthorizationBasic? AuthorizationBasic { get; }

    public WebServerHttpContext(IHttpContext httpContext)
    {
        HttpContext = httpContext;

        AuthorizationBasic = WebServerHttpContextAuthorizationBasic.Create(httpContext);
    }

}

public abstract class WebServerHttpContextAuthorization
{
    public string Authorization { get; }

    protected WebServerHttpContextAuthorization(string authorization)
    {
        Authorization = authorization;
    }

    protected static string? ParseAuthorization(IHttpContext httpContext, string authorizationScheme)
    {
        var hAuthorization = httpContext.Request.Headers["Authorization"].TrimOrNull();
        if (hAuthorization == null) return null;

        if (!hAuthorization.Contains(' ', StringComparison.Ordinal)) return null;

        var hAuthorizationParts = hAuthorization.Split(' ', 2);
        var authorization = hAuthorizationParts.GetAtIndexOrDefault(0).TrimOrNull();
        var authValue = hAuthorizationParts.GetAtIndexOrDefault(1).TrimOrNull();
        if (authorization == null || authValue == null) return null;

        if (!StringComparer.OrdinalIgnoreCase.Equals(authorizationScheme, authorization)) return null;
        return authValue;
    }
}

public class WebServerHttpContextAuthorizationBasic : WebServerHttpContextAuthorization
{
    public string? Username { get; private set; }
    public string? Password { get; private set; }
    public WebServerHttpContextAuthorizationBasic(string authorization) : base(authorization) { }

    public static WebServerHttpContextAuthorizationBasic? Create(IHttpContext httpContext)
    {
        var authValue = ParseAuthorization(httpContext, "basic");
        if (authValue == null) return null;
        var o = new WebServerHttpContextAuthorizationBasic("basic");
        var upEncoded = Convert.FromBase64String(authValue);
        var up = Constant.Encoding_UTF8_Without_BOM.GetString(upEncoded).TrimOrNull();
        if (up != null)
        {
            var upParts = up.Split(':', 2);
            o.Username = upParts.GetAtIndexOrDefault(0).TrimOrNull();
            o.Password = upParts.GetAtIndexOrDefault(1).TrimOrNull();
        }

        return o;
    }
}
