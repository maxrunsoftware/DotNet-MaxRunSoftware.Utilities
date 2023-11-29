using System.Diagnostics;
using System.Threading.Tasks;
using EmbedIO;
using MaxRunSoftware.Utilities.Web.Server;
using WebServer = MaxRunSoftware.Utilities.Web.Server.WebServer;
// ReSharper disable PossibleNullReferenceException

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerHttpContextTests : WebServerTestBase
{
    public WebServerHttpContextTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableFact]
    public void Context_Not_Null()
    {
        LogLevel = LogLevel.Trace;
        var list = new List<WebServerHttpContext>();
        using var s = StartWebServer(Handle);

        Assert.Empty(list);

        var _ = httpIO.GetAsync(Constants.URL_BASE).Result;

        Assert.NotEmpty(list);

        async Task Handle(WebServerHttpContext httpContext)
        {
            var context = httpContext;
            list.Add(context);
            await Task.CompletedTask;
        }




    }

}
