using System.Threading.Tasks;
using EmbedIO;
using MaxRunSoftware.Utilities.Web.Server;
using WebServer = MaxRunSoftware.Utilities.Web.Server.WebServer;

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerTests : TestBase
{
    public WebServerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public void WebServer_IntegrationTests()
    {
        using var ws = new WebServer(LoggerProvider, 8080, Handle);

        while (true)
        {
            Thread.Sleep(100);
        }

        ws.Dispose();

    }

    private async Task Handle(WebServerHttpContext httpContext)
    {
        var context = httpContext!.HttpContext;
        log.LogInformation("Received [{Action}] request for URL {Url}", context.Request.HttpVerb, context.RequestedPath);
        var msg = $"<p>Handling [{context.Request.HttpVerb}] request for {context.RequestedPath}</p>";
        await context.SendStringHtmlSimpleAsync("HttpResponse", msg);
    }
}
