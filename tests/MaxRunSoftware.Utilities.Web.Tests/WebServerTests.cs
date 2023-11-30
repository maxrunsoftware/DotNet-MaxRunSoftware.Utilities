using System.Diagnostics;
using System.Threading.Tasks;
using EmbedIO;
using MaxRunSoftware.Utilities.Web.Server;
using WebServer = MaxRunSoftware.Utilities.Web.Server.WebServer;
// ReSharper disable PossibleNullReferenceException

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerTests : TestBase
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

        async Task Handle(WebServerHttpContext context)
        {
            log.LogInformation("Received [{Action}] request for URL {Url}", context.Method, context.RequestPath);
            var msg = $"<p>Handling [{context.Method}] request for {context.RequestPath}</p>";
            await context.SendStringHtmlSimpleAsync("HttpResponse", msg);
        }
    }

}
