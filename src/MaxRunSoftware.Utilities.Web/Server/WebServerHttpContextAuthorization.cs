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

public abstract class WebServerHttpContextAuthorization
{
    public string AuthorizationName { get; }
    public string? AuthorizationValueRaw { get; }

    protected WebServerHttpContextAuthorization(string authorizationName, string? authorizationValueRaw)
    {
        AuthorizationName = authorizationName;
        AuthorizationValueRaw = authorizationValueRaw;
    }

    protected static (string? name, string? value) ParseAuthorization(IHttpContext httpContext)
    {
        var auth = httpContext.Request.Headers["Authorization"].TrimOrNull();
        if (auth == null) return (null, null);

        if (!auth.Contains(' ', StringComparison.Ordinal)) return (auth, null);

        var parts = auth.Split(' ', 2);
        var name = parts.GetAtIndexOrDefault(0).TrimOrNull();
        var val = parts.GetAtIndexOrDefault(1).TrimOrNull();
        return (name, val);
    }

    protected static string? ParseAuthorization(IHttpContext httpContext, string authorizationName)
    {
        var (name, value) = ParseAuthorization(httpContext);
        return StringComparer.OrdinalIgnoreCase.Equals(name, authorizationName) ? value : null;
    }
}

public class WebServerHttpContextAuthorizationUnknown : WebServerHttpContextAuthorization
{
    public WebServerHttpContextAuthorizationUnknown(string authorizationName, string? authorizationValueRaw) : base(authorizationName, authorizationValueRaw) { }
}

public class WebServerHttpContextAuthorizationBasic : WebServerHttpContextAuthorization
{
    public static bool CanHandle(string authorizationName) => authorizationName.EqualsOrdinalIgnoreCase("basic");

    public string? Username { get; }
    public string? Password { get; }

    public WebServerHttpContextAuthorizationBasic(string authorizationName, string? authorizationValueRaw) : base(authorizationName, authorizationValueRaw)
    {
        string? username = null;
        string? password = null;

        if (authorizationValueRaw != null)
        {
            var userpassEncoded = Convert.FromBase64String(authorizationValueRaw);
            var userpass = Constant.Encoding_UTF8_Without_BOM.GetString(userpassEncoded).TrimOrNull();
            if (userpass != null)
            {
                var upParts = userpass.Split(':', 2);
                username = upParts.GetAtIndexOrDefault(0).TrimOrNull();
                password = upParts.GetAtIndexOrDefault(1).TrimOrNull();
            }
        }

        Username = username;
        Password = password;
    }
}
