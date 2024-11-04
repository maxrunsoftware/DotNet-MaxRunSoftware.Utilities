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
