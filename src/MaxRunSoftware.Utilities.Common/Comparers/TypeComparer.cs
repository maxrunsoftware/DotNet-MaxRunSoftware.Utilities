namespace MaxRunSoftware.Utilities.Common;

public class TypeComparer : ComparerBaseDefault<Type, TypeComparer>
{
    public AssemblyComparer AssemblyComparer { get; set; } = new();

    protected override bool Equals_Internal(Type x, Type y) => x == y;

    protected override int GetHashCode_Internal(Type obj) => obj.GetHashCode();

    protected override IEnumerable<int> Compare_Internal_Comparisons(Type x, Type y)
    {
        yield return AssemblyComparer.Compare(x.Assembly, y.Assembly);
        yield return x.FullNameFormatted().CompareToOrdinalIgnoreCaseThenOrdinal(y.FullNameFormatted());
    }
}
