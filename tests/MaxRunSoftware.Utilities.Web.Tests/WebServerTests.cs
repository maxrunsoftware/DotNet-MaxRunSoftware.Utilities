using System.Diagnostics;
using System.Threading.Tasks;
using EmbedIO;
using MaxRunSoftware.Utilities.Web.Server;
using WebServer = MaxRunSoftware.Utilities.Web.Server.WebServer;

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerTests : WebServerTestBase
{
    public WebServerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

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

        async Task Handle(WebServerHttpContext httpContext)
        {
            var context = httpContext!.HttpContext;
            log.LogInformation("Received [{Action}] request for URL {Url}", context.Request.HttpVerb, context.RequestedPath);
            var msg = $"<p>Handling [{context.Request.HttpVerb}] request for {context.RequestedPath}</p>";
            await context.SendStringHtmlSimpleAsync("HttpResponse", msg);
        }
    }

}
