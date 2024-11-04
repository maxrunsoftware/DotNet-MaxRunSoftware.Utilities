namespace MaxRunSoftware.Utilities.Common;

public class MethodInfoComparer : ComparerBaseDefault<MethodInfo, MethodInfoComparer>
{
    public MethodBaseComparer MethodBaseComparer { get; set; } = new();
    public TypeComparer TypeComparer { get; set; } = new();
    public ParameterInfoComparer ParameterInfoComparer { get; set; } = new();

    protected override bool Equals_Internal(MethodInfo x, MethodInfo y)
    {
        if (!MethodBaseComparer.Equals(x, y)) return false;
        if (!TypeComparer.Equals(x.ReturnType, y.ReturnType)) return false;
        if (!ParameterInfoComparer.Equals(x.ReturnParameter, y.ReturnParameter)) return false;

        return true;
    }

    protected override void GetHashCode_Internal(MethodInfo obj, ref HashCode h)
    {
        h.Add(MethodBaseComparer.GetHashCode(obj));
        h.Add(TypeComparer.GetHashCode(obj.ReturnType));
        h.Add(ParameterInfoComparer.GetHashCode(obj.ReturnParameter));
    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(MethodInfo x, MethodInfo y)
    {
        yield return MethodBaseComparer.Compare(x, y);
        yield return TypeComparer.Compare(x.ReturnType, y.ReturnType);
        yield return ParameterInfoComparer.Compare(x.ReturnParameter, y.ReturnParameter);
    }
}
