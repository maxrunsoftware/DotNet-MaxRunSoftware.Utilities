using System.Security;

namespace MaxRunSoftware.Utilities.Common;

public class AssemblyNameComparer : ComparerBaseDefault<AssemblyName, AssemblyNameComparer>
{
    public CultureInfoComparer CultureInfoComparer { get; set; } = new();
    
    protected override bool Equals_Internal(AssemblyName x, AssemblyName y)
    {
        if (x.FullName != y.FullName) return false;
        if (x.ContentType != y.ContentType) return false;
        if (!CultureInfoComparer.Equals(x.CultureInfo, y.CultureInfo)) return false;
        if (x.Flags != y.Flags) return false;
        
        ReadOnlySpan<byte> xPublicKey;
        
        try
        {
            xPublicKey = x.GetPublicKey() ?? [];
        }
        catch (SecurityException)
        {
            xPublicKey = [];
        }
        
        ReadOnlySpan<byte> yPublicKey;
        try
        {
            yPublicKey = y.GetPublicKey() ?? [];
        }
        catch (SecurityException)
        {
            yPublicKey = [];
        }

        if (!xPublicKey.IsEqual(yPublicKey)) return false;

        var xPublicKeyToken = x.GetPublicKeyToken() ?? [];
        var yPublicKeyToken = y.GetPublicKeyToken() ?? [];

        if (!xPublicKeyToken.IsEqual(yPublicKeyToken)) return false;

        if (!x.Version.IsEqual(y.Version)) return false;
        
        return true;
    }

    protected override void GetHashCode_Internal(AssemblyName obj, ref HashCode h)
    {
        h.Add(obj.FullName);
        h.Add(obj.ContentType);
        h.Add(CultureInfoComparer.GetHashCode(obj.CultureInfo));
        h.Add(obj.Flags);

        ReadOnlySpan<byte> publicKey;
        try
        {
            publicKey = obj.GetPublicKey() ?? [];
        }
        catch (SecurityException)
        {
            publicKey = [];
        }
        h.AddBytes(publicKey);
        
        h.AddBytes(obj.GetPublicKeyToken() ?? []);
        h.Add(obj.Version);
        
    }

    protected override IEnumerable<int> Compare_Internal_Comparisons(AssemblyName x, AssemblyName y)
    {
        yield return x.FullName.CompareOrdinalIgnoreCase(y.FullName);
        yield return x.Version.Compare(y.Version);
        yield return x.Flags.CompareTo(y.Flags);
        
        yield return x.FullName.CompareOrdinal(y.FullName);

    }
}