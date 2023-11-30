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
    private readonly ILogger log;
    private string AuthenticateHeaderValue => "Basic realm=\"" + (Realm ?? BaseRoute) + "\" charset=UTF-8";

    public override bool IsFinalHandler => false;

    public string? Realm { get; set; }

    public Func<WebServerHttpContext, bool>? HandlerAuthenticate { get; set; }

    public WebServerBasicAuthentication(ILoggerProvider loggerProvider, string baseRoute = "/") : base(baseRoute)
    {
        log = loggerProvider.CreateLogger<WebServerBasicAuthentication>();
    }

    protected override async Task OnRequestAsync(IHttpContext context)
    {
        context.Response.Headers.Set("WWW-Authenticate", AuthenticateHeaderValue);

        var httpContext = new WebServerHttpContext(context);
        var handler = HandlerAuthenticate;
        if (handler == null)
        {
            log.LogError("{Property} not defined", nameof(HandlerAuthenticate));
            throw HttpException.InternalServerError();
        }

        var isAuthenticated = await Task.FromResult(handler(httpContext));
        if (!isAuthenticated)
        {
            throw HttpException.Unauthorized();
        }
    }


}
