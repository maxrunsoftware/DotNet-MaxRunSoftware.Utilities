// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
