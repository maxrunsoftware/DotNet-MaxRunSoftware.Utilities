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

using System.Threading.Tasks;
using EmbedIO;

namespace MaxRunSoftware.Utilities.Web.Server;

public class WebServerBasicAuthentication : WebModuleBase
{
    private readonly string wwwAuthenticateHeaderValue;

    public override bool IsFinalHandler => false;

    public string Realm { get; }

    public WebServerBasicAuthentication(string baseRoute, string? realm = null) : base(baseRoute)
    {
        Realm = realm.TrimOrNull() ?? BaseRoute;
        wwwAuthenticateHeaderValue = $"Basic realm=\"{Realm}\" charset=UTF-8";
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        context.Response.Headers.Set("WWW-Authenticate", wwwAuthenticateHeaderValue);
        if (!await IsAuthenticatedAsync(context).ConfigureAwait(false))
        {
            throw HttpException.Unauthorized();
        }
    }

    private async Task<bool> IsAuthenticatedAsync(IHttpContext context)
    {
        try
        {
            var up = GetCredentials(context.Request);
            var username = up?.UserName;
            var password = up?.Password;
            return await VerifyCredentialsAsync(context, username, password, context.CancellationToken).ConfigureAwait(false);
        }
        catch (FormatException ex)
        {
            // TODO: log exception
            return false;
        }
    }

    protected virtual Task<bool> VerifyCredentialsAsync(
        IHttpContext context,
        string? userName,
        string? password,
        CancellationToken cancellationToken) =>
        Task.FromResult(VerifyCredentialsInternal(userName, password));

    private bool VerifyCredentialsInternal(string? userName, string? password) =>
        //string b;
        //return userName != null && this.Accounts.TryGetValue(userName, out b) && string.Equals(password, b, StringComparison.Ordinal);
        true;

    private static (string? UserName, string? Password)? GetCredentials(IHttpRequest request)
    {
        try
        {
            var header = request.Headers["Authorization"];
            if (header == null) return null;
            if (!header.StartsWith("basic ", StringComparison.OrdinalIgnoreCase)) return null;


            var upBase64 = header.Substring("basic ".Length).TrimOrNull();
            if (upBase64 == null) return null;
            var upEncoded = Convert.FromBase64String(upBase64);
            var up = EmbedIO.WebServer.DefaultEncoding.GetString(upEncoded).TrimOrNull();
            if (up == null) return null;

            var upParts = up.Split(':', 2);
            return upParts.Length switch
            {
                0 => null,
                1 => (upParts[0].Trim(), string.Empty),
                _ => (upParts[0].Trim(), upParts[1].Trim()),
            };
        }
        catch (Exception ex)
        {
            // TODO: log exception
            return null;
        }
    }
}
