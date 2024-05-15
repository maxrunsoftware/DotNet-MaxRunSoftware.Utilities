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

using MaxRunSoftware.Utilities.Web.Server.EmbedIO;

// ReSharper disable PossibleNullReferenceException

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void WebServer_Run()
    {
        using var ws = StartWebServer(Handle);

        var startTime = DateTime.Now;
        while ((DateTime.Now - startTime).TotalSeconds < 60d)
        {
            Thread.Sleep(100);
        }

        Assert.True(ws != null);
        return;

        async Task Handle(WebServerHttpContext context)
        {
            log.LogInformation("Received [{Action}] request for URL {Url}", context.Method, context.RequestPath);
            var msg = $"<p>Handling [{context.Method}] request for {context.RequestPath}</p>";
            await context.SendHtmlSimpleAsync("HttpResponse", msg);
        }
    }
}
