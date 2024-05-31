using GenHTTP.Modules.IO;

namespace MaxRunSoftware.Utilities.Web.Tests;

public class GenHTTPServerSiteAuthTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void StartServerTest()
    {
        LogLevel = LogLevel.Trace;
        var server = new GenHTTPServerSiteAuth(LoggerProvider.CreateLogger<GenHTTPServerSiteAuth>())
        {
            Port = TestConfig.DEFAULT_PORT,
            Theme = new GenHTTP.Themes.NoTheme.NoThemeInstance(),
        };
        server.Run();
    }
    
    
}
