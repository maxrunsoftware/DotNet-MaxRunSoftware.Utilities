using System.Diagnostics;
using System.Threading.Tasks;
using EmbedIO;
using MaxRunSoftware.Utilities.Web.Server;
using WebServer = MaxRunSoftware.Utilities.Web.Server.WebServer;
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
