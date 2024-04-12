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

public class WebServerAuthenticationBasicModule : WebModuleBase
{
    private readonly ILogger log;
    private readonly Func<IWebServerAuthenticationBasicHandler?> getAuthenticationHandler;
    public override bool IsFinalHandler => false;

    public WebServerAuthenticationBasicModule(
        ILoggerFactory loggerProvider,
        Func<IWebServerAuthenticationBasicHandler?> getAuthenticationHandler,
        string baseRoute = "/"
    ) : base(baseRoute)
    {
        log = loggerProvider.CreateLogger<WebServerAuthenticationBasicModule>();
        this.getAuthenticationHandler = getAuthenticationHandler;
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        var authHandler = getAuthenticationHandler();
        if (authHandler != null)
        {
            var httpContext = new WebServerHttpContext(context);
            if (authHandler.IsPathRequiresAuthentication(httpContext))
            {
                log.LogTrace("Access to {Url} requires BASIC authentication", httpContext.RequestPath);

                var realm = authHandler.GetRealm(httpContext) ?? BaseRoute;
                httpContext.SetResponseHeader(HttpResponseHeader.WwwAuthenticate, $"Basic realm=\"{realm}\" charset=UTF-8");

                var isAuthenticated = await Task.FromResult(authHandler.IsAuthenticationValid(httpContext));

                if (!isAuthenticated)
                {
                    log.LogTrace("Access to {Url} failed because of failed authentication", httpContext.RequestPath);
                    throw HttpException.Unauthorized();
                }
            }
        }
    }
}

[PublicAPI]
public interface IWebServerAuthenticationBasicHandler
{
    public string? GetRealm(WebServerHttpContext context);

    public bool IsPathRequiresAuthentication(WebServerHttpContext context);

    public bool IsAuthenticationValid(WebServerHttpContext context);
}

[PublicAPI]
public class WebServerAuthenticationBasicHandler : IWebServerAuthenticationBasicHandler
{
    public required Func<WebServerHttpContext, string?> GetRealmFunc { get; init; }
    public string? GetRealm(WebServerHttpContext context) => GetRealmFunc(context);

    public required Func<WebServerHttpContext, bool> IsPathRequiresAuthenticationFunc { get; init; }
    public bool IsPathRequiresAuthentication(WebServerHttpContext context) => IsPathRequiresAuthenticationFunc(context);

    public required Func<WebServerHttpContext, bool> IsAuthenticationValidFunc { get; init; }
    public bool IsAuthenticationValid(WebServerHttpContext context) => IsAuthenticationValidFunc(context);
}
