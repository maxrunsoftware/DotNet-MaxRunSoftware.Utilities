using MaxRunSoftware.Utilities.Web.Server;

// ReSharper disable AssignNullToNotNullAttribute

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebUrlPathTests : TestBase
{
    public WebUrlPathTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableTheory]
    [InlineData(true, "")]
    [InlineData(true, "    ")]
    [InlineData(true, "  \t  ")]
    [InlineData(true, "  /  ")]
    [InlineData(true, "  \t  /  ")]
    [InlineData(true, "/")]
    [InlineData(true, "//")]
    [InlineData(true, "///")]
    [InlineData(false, "aaa")]
    [InlineData(false, "aaa/")]
    [InlineData(false, "/aaa")]
    [InlineData(false, "/aaa/")]
    [InlineData(false, "//aaa")]
    [InlineData(false, "/aaa/bbb")]
    [InlineData(false, "/aaa/bbb/")]
    public void IsRoot(bool isRoot, string url)
    {
        var p = new WebUrlPath(url);

        Assert.Equal(isRoot, p.IsRoot);
    }

    [SkippableTheory]
    [InlineData(true, "", "")]
    [InlineData(true, "/", "")]
    [InlineData(true, "aaa", "/aaa/")]
    [InlineData(true, "aaa", "/")]
    [InlineData(true, "aaa/bbb", "/aaa/bbb/")]
    [InlineData(true, "aaa/bbb", "/aaa")]
    [InlineData(true, "aaa/bbb", "/")]
    [InlineData(false, "", "aaa")]
    [InlineData(false, "/", "aaa")]
    [InlineData(false, "aaa/bbb", "/aaa/bbb/ccc")]
    public void StartsWith(bool isMatch, string url, string startsWith)
    {
        var p = new WebUrlPath(url);
        Assert.Equal(isMatch, p.StartsWith(startsWith));
    }

}
