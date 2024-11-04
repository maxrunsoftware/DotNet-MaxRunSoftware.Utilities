using System.Globalization;

namespace MaxRunSoftware.Utilities.Common.Tests.Comparers;

public class CultureInfoComparerTests(ITestOutputHelper testOutputHelper) : ComparerTestsBase<
    CultureInfo,
    CultureInfoComparer,
    CultureInfoComparerTests.Data
>(testOutputHelper)
{
    public class Data : ComparerTestsData<CultureInfo, CultureInfoComparer>
    {
        public override IEnumerable<CultureInfoComparer> Comparers => [CultureInfoComparer.Default, new(),];

        public override IEnumerable<(CultureInfo, CultureInfo)> Same =>
        [
            (Get<int>(), Get<int>()),
            (Get<Percent>(), Get<Percent>()),
            (Get<ITestOutputHelper>(), Get<ITestOutputHelper>()),
        ];

        private static CultureInfo Get<T>() => typeof(T).Assembly.GetName().CultureInfo;
    }
}
