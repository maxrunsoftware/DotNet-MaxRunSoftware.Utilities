namespace MaxRunSoftware.Utilities.Common.Tests.Comparers;

public class AssemblyNameComparerTests(ITestOutputHelper testOutputHelper) : ComparerTestsBase<
    AssemblyName,
    AssemblyNameComparer,
    AssemblyNameComparerTests.Data
>(testOutputHelper)
{
    public class Data : ComparerTestsData<AssemblyName, AssemblyNameComparer>
    {
        public override IEnumerable<AssemblyNameComparer> Comparers => [AssemblyNameComparer.Default, new(),];

        public override IEnumerable<(AssemblyName, AssemblyName)> Same =>
        [
            (Get<int>(), Get<int>()),
            (Get<Percent>(), Get<Percent>()),
            (Get<ITestOutputHelper>(), Get<ITestOutputHelper>()),
        ];

        private static AssemblyName Get<T>() => typeof(T).Assembly.GetName();
    }
}
