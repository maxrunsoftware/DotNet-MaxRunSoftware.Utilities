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

using MaxRunSoftware.Utilities.Web.Server;

// ReSharper disable PossibleNullReferenceException

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerHttpContextTests : TestBase
{
    public WebServerHttpContextTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableFact]
    public void Context_Not_Null()
    {
        var list = new List<WebServerHttpContext>();
        using var s = StartWebServer(Handle);

        Assert.Empty(list);

        _ = httpIO.GetAsync(Constants.URL_BASE).Result;

        Assert.NotEmpty(list);

        async Task Handle(WebServerHttpContext httpContext)
        {
            var context = httpContext;
            list.Add(context);
            await Task.CompletedTask;
        }
    }

    [SkippableFact]
    public void Context_RequestPath()
    {
        var list = new List<WebServerHttpContext>();
        using var s = StartWebServer(Handle);

        list.Clear();
        Assert.Empty(list);
        _ = httpIO.GetAsync(Constants.URL_BASE + "").Result;
        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.RequestPath.StartsWith("/"));

        list.Clear();
        Assert.Empty(list);
        _ = httpIO.GetAsync(Constants.URL_BASE + "/hello/world").Result;
        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.RequestPath.StartsWith("hello", "world"));

        list.Clear();
        Assert.Empty(list);
        _ = httpIO.GetAsync(Constants.URL_BASE + "/hello/world?a=b").Result;
        Assert.NotEmpty(list);
        Assert.Contains(list, o => o.RequestPath.StartsWith("hello", "world"));

        async Task Handle(WebServerHttpContext httpContext)
        {
            var context = httpContext;
            list.Add(context);
            await Task.CompletedTask;
        }
    }
}
