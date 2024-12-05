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

using System.Diagnostics;
using System.Numerics;

namespace MaxRunSoftware.Utilities.Common;

public class VersionPartComparer : ComparerBaseDefault<VersionPart, VersionPartComparer>
{
    public override bool Equals(VersionPart? x, VersionPart? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null))
        {
            Debug.Assert(y != null);
            if (y.ValueBigInteger == null) return false;
            return y.ValueBigInteger.Value == BigInteger.Zero;
        }

        if (ReferenceEquals(y, null))
        {
            Debug.Assert(x != null);
            if (x.ValueBigInteger == null) return false;
            return x.ValueBigInteger.Value == BigInteger.Zero;
        }

        if (x.GetHashCode() != y.GetHashCode()) return false;
        if (!x.ValueInt.IsEqual(y.ValueInt)) return false;
        if (!x.ValueLong.IsEqual(y.ValueLong)) return false;
        if (!x.ValueBigInteger.IsEqual(y.ValueBigInteger)) return false;
        if (!x.Value.EqualsOrdinalIgnoreCase(y.Value)) return false;
        return true;
    }
    
    protected override int GetHashCode_Internal(VersionPart obj) => 
        obj.ValueInt?.GetHashCode()
        ?? obj.ValueLong?.GetHashCode()
        ?? obj.ValueBigInteger?.GetHashCode()
        ?? StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Value);

    protected override int Compare_Internal(VersionPart x, VersionPart y)
    {
        if (x.ValueBigInteger != null)
        {
            if (y.ValueBigInteger != null) return x.ValueBigInteger.Value.CompareTo(y.ValueBigInteger.Value);
            return -1;
        }

        if (y.ValueBigInteger != null)
        {
            return 1;
        }

        if (x.ValueDateTime != null)
        {
            if (y.ValueDateTime != null) return x.ValueDateTime.Value.CompareTo(y.ValueDateTime.Value);
            return -1;
        }

        if (y.ValueDateTime != null)
        {
            return 1;
        }
        
        return Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(x.Value, y.Value);
    }
}
