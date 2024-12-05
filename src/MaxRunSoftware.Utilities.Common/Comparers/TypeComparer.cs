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
