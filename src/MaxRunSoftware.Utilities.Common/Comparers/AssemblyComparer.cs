namespace MaxRunSoftware.Utilities.Common;

public class AssemblyComparer : ComparerBaseDefault<Assembly, AssemblyComparer>
{
    public AssemblyNameComparer AssemblyNameComparer { get; set; } = new();
    
    protected override bool Equals_Internal(Assembly x, Assembly y) => AssemblyNameComparer.Equals(x.GetName(), y.GetName());

    protected override int GetHashCode_Internal(Assembly obj) => AssemblyNameComparer.GetHashCode(obj.GetName());

    protected override int Compare_Internal(Assembly x, Assembly y) => AssemblyNameComparer.Compare(x.GetName(), y.GetName());
}
