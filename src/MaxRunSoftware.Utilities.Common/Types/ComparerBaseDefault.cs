namespace MaxRunSoftware.Utilities.Common;

public abstract class ComparerBaseDefault<T, TComparer> : ComparerBase<T>
    where T : class
    where TComparer : ComparerBaseDefault<T, TComparer>, new()
{
    public static TComparer Default { get; } = new();
    
}
