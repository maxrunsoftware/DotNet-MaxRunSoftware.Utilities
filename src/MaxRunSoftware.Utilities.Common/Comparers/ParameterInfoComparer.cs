namespace MaxRunSoftware.Utilities.Common;

public class ParameterInfoComparer : ComparerBaseDefault<ParameterInfo, ParameterInfoComparer>
{
    public TypeComparer TypeComparer { get; set; } = new();

    protected override bool Equals_Internal(ParameterInfo x, ParameterInfo y)
    {
        if (x.Name != y.Name) return false;
        if (x.Position != y.Position) return false;

        if (x.Attributes != y.Attributes) return false;
        // if (x.DefaultValue != y.DefaultValue && x.DefaultValue?.ToStringGuessFormat() != y.DefaultValue?.ToStringGuessFormat()) return false;

        if (!TypeComparer.Equals(x.ParameterType, y.ParameterType)) return false;

        if (x.MetadataToken_Safe() != y.MetadataToken_Safe()) return false;

        if (!TypeComparer.Equals(x.GetModifiedParameterType(), y.GetModifiedParameterType())) return false;
        if (!TypeComparer.Equals(x.GetRequiredCustomModifiers(), y.GetRequiredCustomModifiers())) return false;
        if (!TypeComparer.Equals(x.GetOptionalCustomModifiers(), y.GetOptionalCustomModifiers())) return false;
        if (x.HasDefaultValue != y.HasDefaultValue) return false;
        
        if (x.IsIn() != y.IsIn()) return false;
        if (x.IsOut() != y.IsOut()) return false;
        if (x.IsRef() != y.IsRef()) return false;

        if (x.IsIn != y.IsIn) return false;
        if (x.IsLcid != y.IsLcid) return false;
        if (x.IsOptional != y.IsOptional) return false;
        if (x.IsOut != y.IsOut) return false;
        if (x.IsRetval != y.IsRetval) return false;
        
        return true;
    }

    protected override void GetHashCode_Internal(ParameterInfo obj, ref HashCode h)
    {
        h.Add(obj.Name);
        h.Add(obj.Position);
        
        h.Add(obj.Attributes);
        
        h.Add(TypeComparer.GetHashCode(obj.ParameterType));
        
        h.Add(obj.MetadataToken_Safe());
        
        // h.Add(TypeComparer.GetHashCode(obj.GetModifiedParameterType()));
        h.Add(TypeComparer.GetHashCode(obj.GetRequiredCustomModifiers()));
        h.Add(TypeComparer.GetHashCode(obj.GetOptionalCustomModifiers()));
        h.Add(obj.HasDefaultValue);
        
        h.Add(obj.IsIn());
        h.Add(obj.IsOut());
        h.Add(obj.IsRef());
        
        h.Add(obj.IsIn);
        h.Add(obj.IsLcid);
        h.Add(obj.IsOptional);
        h.Add(obj.IsOut);
        h.Add(obj.IsRetval);
    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(ParameterInfo x, ParameterInfo y)
    {
        yield return x.Position.CompareTo(y.Position);
        yield return x.IsOptional.CompareTo(y.IsOptional);
        yield return x.Name.CompareToOrdinalIgnoreCaseThenOrdinal(y.Name);
    }
}
