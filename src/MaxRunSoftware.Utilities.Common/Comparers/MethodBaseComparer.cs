namespace MaxRunSoftware.Utilities.Common;

public class MethodBaseComparer : ComparerBaseDefault<MethodBase, MethodBaseComparer>
{
    public MemberInfoComparer MemberInfoComparer { get; set; } = new();
    public TypeComparer TypeComparer { get; set; } = new();
    public ParameterInfoComparer ParameterInfoComparer { get; set; } = new();

    private static IEnumerable<bool> GetBools(MethodBase x)
    {
        yield return x.IsAbstract;
        yield return x.IsAnonymous();
        yield return x.IsAssembly;
        yield return x.IsConstructedGenericMethod;
        yield return x.IsConstructor;
        yield return x.IsFamily;
        yield return x.IsFamilyAndAssembly;
        yield return x.IsFamilyOrAssembly;
        yield return x.IsFinal;
        yield return x.IsGenericMethod;
        yield return x.IsGenericMethodDefinition;
        yield return x.IsHideBySig;
        yield return x.IsInner();
        yield return x.IsPrivate;
        yield return x.IsPublic;
        yield return x.IsSecurityCritical;
        yield return x.IsSecuritySafeCritical;
        yield return x.IsSecurityTransparent;
        yield return x.IsSpecialName;
        yield return x.IsStatic;
        yield return x.IsVirtual;
    }
    
    protected override bool Equals_Internal(MethodBase x, MethodBase y)
    {
        if (!MemberInfoComparer.Equals(x, y)) return false;
        
        if (x.Attributes != y.Attributes) return false;
        
        if (x.CallingConvention != y.CallingConvention) return false;

        if (x.GetDeclarationType() != y.GetDeclarationType()) return false;
        if (!TypeComparer.Equals(x.GetGenericArguments_Safe() ?? [], y.GetGenericArguments_Safe() ?? [])) return false;

        var xMethodBody = Util.Catch(x.GetMethodBody);
        var yMethodBody = Util.Catch(y.GetMethodBody);
        if (xMethodBody != null && yMethodBody != null)
        {
            var xLocalSignatureMetadataToken = Util.CatchN(() => xMethodBody.LocalSignatureMetadataToken);
            var yLocalSignatureMetadataToken = Util.CatchN(() => yMethodBody.LocalSignatureMetadataToken);
            
            if (xLocalSignatureMetadataToken != null && yLocalSignatureMetadataToken != null)
            {
                if (xLocalSignatureMetadataToken != yLocalSignatureMetadataToken) return false;
            }
        }
        
        if (x.GetMethodImplementationFlags() != y.GetMethodImplementationFlags()) return false;
        
        if (!ParameterInfoComparer.Equals(x.GetParameters(), y.GetParameters())) return false;

        foreach (var (bx, by) in GetBools(x).Zip(GetBools(y)))
        {
            if (bx != by) return false;
        }
        
        
        return true;
    }

    protected override void GetHashCode_Internal(MethodBase obj, ref HashCode h)
    {
        h.Add(MemberInfoComparer.GetHashCode(obj));
        
        h.Add(obj.Attributes);
        
        h.Add(obj.CallingConvention);

        h.Add(obj.GetDeclarationType());
        h.Add(TypeComparer.GetHashCode(obj.GetGenericArguments_Safe() ?? []));

        h.Add(obj.GetMethodImplementationFlags());
        
        h.Add(ParameterInfoComparer.GetHashCode(obj.GetParameters()));

        foreach (var b in GetBools(obj))
        {
            h.Add(b);
        }

    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(MethodBase x, MethodBase y)
    {
        yield return MemberInfoComparer.Compare(x, y);
        yield return TypeComparer.Compare(x.GetGenericArguments_Safe() ?? [], y.GetGenericArguments_Safe() ?? []);
        yield return ParameterInfoComparer.Compare(x.GetParameters(), y.GetParameters());
        yield return x.IsAnonymous().CompareTo(y.IsAnonymous());
        yield return ((int)x.Attributes).CompareTo((int)y.Attributes);
    }
}
