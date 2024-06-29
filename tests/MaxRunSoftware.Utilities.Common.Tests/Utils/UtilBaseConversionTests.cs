using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

public class UtilBaseConversionTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableTheory]
    [InlineData(new byte[] {0}, "00")]
    [InlineData(new byte[] {1}, "01")]
    [InlineData(new byte[] {15}, "0F")]
    [InlineData(new byte[] {16}, "10")]
    [InlineData(new byte[] {255}, "FF")]
    [InlineData(new byte[] {0, 0}, "0000")]
    [InlineData(new byte[] {0, 1}, "0001")]
    [InlineData(new byte[] {1, 0}, "0100")]
    [InlineData(new byte[] {1, 1}, "0101")]
    [InlineData(new byte[] {255, 1}, "FF01")]
    [InlineData(new byte[] {255, 255}, "FFFF")]
    [InlineData(new byte[] {1, 2, 3}, "010203")]
    public void Base16_Encode(byte[] bytes, string expected)
    {
        Assert.Equal(expected, Util.Base16(bytes));
    }

    [SkippableFact]
    public void Base16_Encode_Benchmark()
    {
        var random = new Random(42);
        var bytes = random.NextBytes((int)Constant.Bytes_Mega * 10);
        var stopwatch = new Stopwatch();
        var ts = TimeSpan.Zero;
        var loopCount = 10;
        for (var i = 0; i < loopCount; i++)
        {
            stopwatch.Restart();
            _ = Util.Base16(bytes);
            stopwatch.Stop();
            ts = ts + stopwatch.Elapsed;
        }
        
        log.LogInformation("Time: {Time} ms", ts.TotalMilliseconds);
    }

}
