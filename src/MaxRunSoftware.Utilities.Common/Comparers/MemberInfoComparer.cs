namespace MaxRunSoftware.Utilities.Common;

public class MemberInfoComparer : ComparerBaseDefault<MemberInfo, MemberInfoComparer>
{
    public TypeComparer TypeComparer { get; set; } = new();
    public ModuleComparer ModuleComparer { get; set; } = new();
    
    protected override bool Equals_Internal(MemberInfo x, MemberInfo y)
    {
        if (x.Equals(y)) return true;
        
        if (x.MemberType != y.MemberType) return false;
        if (x.Name != y.Name) return false;
        if (!TypeComparer.Equals(x.DeclaringType, y.DeclaringType)) return false;
        if (!ModuleComparer.Equals(x.Module_Safe(), y.Module_Safe())) return false;

        var hasSameMetadataDefinitionAs = x.HasSameMetadataDefinitionAs_Safe(y) ?? true;
        if (!hasSameMetadataDefinitionAs) return false;
        
        if (x.IsCollectible != y.IsCollectible) return false;
        if (x.MetadataToken_Safe() != y.MetadataToken_Safe()) return false;
    
        if (x.DeclarationFlags() != y.DeclarationFlags()) return false;
        
        return true;
    }

    protected override void GetHashCode_Internal(MemberInfo obj, ref HashCode h)
    {
        h.Add(obj.MemberType);
        h.Add(obj.Name);
        h.Add(TypeComparer.GetHashCode(obj.DeclaringType));
        h.Add(ModuleComparer.GetHashCode(obj.Module_Safe()));
        h.Add(obj.IsCollectible);
        h.Add(obj.MetadataToken_Safe());
        h.Add(obj.DeclarationFlags());
    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(MemberInfo x, MemberInfo y)
    {
        yield return ModuleComparer.Compare(x.Module_Safe(), y.Module_Safe());
        yield return TypeComparer.Compare(x.DeclaringType, y.DeclaringType);
        yield return ((int)x.MemberType).CompareTo((int)y.MemberType);
        yield return x.Name.CompareToOrdinalIgnoreCaseThenOrdinal(y.Name);
    }
}
