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

using System.Globalization;

namespace MaxRunSoftware.Utilities.Common;

public class CultureInfoComparer : ComparerBaseDefault<CultureInfo, CultureInfoComparer>
{
    protected override bool Equals_Internal(CultureInfo x, CultureInfo y)
    {
        if (x.ThreeLetterISOLanguageName != y.ThreeLetterISOLanguageName) return false;
        if (x.TwoLetterISOLanguageName != y.TwoLetterISOLanguageName) return false;
        if (x.Name != y.Name) return false;
        if (x.NativeName != y.NativeName) return false;
        if (x.DisplayName != y.DisplayName) return false;
        if (x.IsNeutralCulture != y.IsNeutralCulture) return false;
        return true;
    }

    protected override int GetHashCode_Internal(CultureInfo obj) => obj.ThreeLetterISOLanguageName.GetHashCode();

    protected override int Compare_Internal(CultureInfo x, CultureInfo y) => Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(x.DisplayName, y.DisplayName);
}
