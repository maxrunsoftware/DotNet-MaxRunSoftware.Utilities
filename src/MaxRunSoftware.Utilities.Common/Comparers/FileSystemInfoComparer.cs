using System.Security;

namespace MaxRunSoftware.Utilities.Common;

public class FileSystemInfoComparer : ComparerBaseDefault<FileSystemInfo, FileSystemInfoComparer>
{
    public IEqualityComparer<string> PathEqualityComparer { get; set; } = Constant.Path_StringComparer;
    public bool CompareListsDirectoriesFirst { get; set; }
    
    private static string? GetName(FileSystemInfo? info) => info == null ? null : Util.Catch(() => info.FullName);

    private static (string?, string?) GetNames(FileSystemInfo? x, FileSystemInfo? y)
    {
        var xName = GetName(x);
        var yName = GetName(y);

        if (xName == null && yName == null)
        {
            xName = x?.Name;
            yName = y?.Name;
        }

        return (xName, yName);
    }
    protected override bool Equals_Internal(FileSystemInfo x, FileSystemInfo y)
    {
        if (GetTypeToInt(x) != GetTypeToInt(y)) return false;
        var (xName, yName) = GetNames(x, y);
        return PathEqualityComparer.Equals(xName, yName);
    }

    protected override int GetHashCode_Internal(FileSystemInfo obj) => Constant.Path_StringComparer.GetHashCode(GetName(obj) ?? obj.Name);

    protected override IEnumerable<int> Compare_Internal_Comparisons(FileSystemInfo x, FileSystemInfo y)
    {
        if (CompareListsDirectoriesFirst) yield return GetTypeToInt(x).CompareTo(GetTypeToInt(y));
        var (xName, yName) = GetNames(x, y);
        yield return xName.CompareToOrdinalIgnoreCaseThenOrdinal(yName);
    }
    
    private static int GetTypeToInt(FileSystemInfo info) => info switch
    {
        DirectoryInfo => 1,
        FileInfo => 2,
        _ => throw new NotImplementedException($"Unknown FileSystemInfo type [{info.GetType().FullNameFormatted()}]: {info}"),
    };
}
