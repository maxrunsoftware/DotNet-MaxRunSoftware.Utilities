namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebServerTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableFact]
    public void SampleTest()
    {
        log.LogInformationMethod(new(), "Some test");
        Assert.True(true);
    }
}
