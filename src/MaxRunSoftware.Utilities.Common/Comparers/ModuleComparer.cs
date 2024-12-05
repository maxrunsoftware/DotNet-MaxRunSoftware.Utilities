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

public class ModuleComparer : ComparerBaseDefault<Module, ModuleComparer>
{
    public AssemblyComparer AssemblyComparer { get; set; } = new();
    
    protected override bool Equals_Internal(Module x, Module y)
    {
        if (x.Equals(y)) return true;
        if (!AssemblyComparer.Default.Equals(Util.Catch(() => x.Assembly), Util.Catch(() => y.Assembly))) return false;
        if (Util.Catch(() => x.FullyQualifiedName) != Util.Catch(() => y.FullyQualifiedName)) return false;
        if (Util.Catch(() => x.Name) != Util.Catch(() => y.Name)) return false;
        if (Util.CatchN(() => x.ModuleVersionId) != Util.CatchN(() => y.ModuleVersionId)) return false;
        if (Util.CatchN(() => x.MDStreamVersion) != Util.CatchN(() => y.MDStreamVersion)) return false;
        if (x.MetadataToken_Safe() != y.MetadataToken_Safe()) return false;
        if (Util.Catch(() => x.ScopeName) != Util.Catch(() => y.ScopeName)) return false;

        return true;
    }

    protected override void GetHashCode_Internal(Module obj, ref HashCode h)
    {
        h.Add(AssemblyComparer.GetHashCode(Util.Catch(() => obj.Assembly)));
        h.Add(Util.Catch(() => obj.FullyQualifiedName));
        h.Add(Util.Catch(() => obj.Name));
        h.Add(Util.CatchN(() => obj.ModuleVersionId));
        h.Add(Util.CatchN(() => obj.MDStreamVersion));
        h.Add(obj.MetadataToken_Safe());
        h.Add(Util.Catch(() => obj.ScopeName));
    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(Module x, Module y)
    {
        yield return AssemblyComparer.Default.Compare(Util.Catch(() => x.Assembly), Util.Catch(() => y.Assembly));
        yield return Util.Catch(() => x.FullyQualifiedName).CompareToOrdinalIgnoreCaseThenOrdinal(Util.Catch(() => y.FullyQualifiedName));
        yield return Util.Catch(() => x.Name).CompareToOrdinalIgnoreCaseThenOrdinal(Util.Catch(() => y.Name));
        yield return Util.CatchN(() => x.ModuleVersionId).CompareToNullable(Util.CatchN(() => y.ModuleVersionId));
    }
}
