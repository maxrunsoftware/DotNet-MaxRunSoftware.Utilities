// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

public interface IVfs
{
    public static bool Default_IsPathCaseSensitive { get; set; } = Constant.Path_IsCaseSensitive;
    public static char[] Default_PathDelimiters { get; set; } = Constant.PathDelimiters.ToArray();

    public bool IsPathCaseSensitive { get; }
    public IReadOnlyList<char> PathDelimiters { get; }

}

public interface IVfsFile
{
    public string Name { get; }
    public string Path { get; }
}

public interface IVfsDirectory
{

}


public interface IVfsPath : IReadOnlyList<string>
{
    public string PathRaw { get; }
    public bool StartsWith(IVfsPath path, bool isPathCaseSensitive);
    public bool Equals(IVfsPath? path, bool isPathCaseSensitive);
    public int Compare(IVfsPath? path, bool isPathCaseSensitive);
}

public class VfsPath : IVfsPath
{
    private static StringComparer GetStringComparer(bool isCaseSensitive) => isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

    private readonly string[] pathParts;

    public string PathRaw { get; }

    public VfsPath(string path, char[] pathDelimiters)
    {
        PathRaw = path;

        pathParts = path
            .Split(pathDelimiters)
            .Where(o => o.Length > 0)
            .WhereNotNull()
            .ToArray();
    }

    public bool StartsWith(IVfsPath prefix, bool isPathCaseSensitive)
    {
        if (prefix.Count > Count) return false;
        var sc = GetStringComparer(isPathCaseSensitive);
        for (var i = 0; i < prefix.Count; i++)
        {
            if (!sc.Equals(prefix[i], this[i]))
            {
                return false;
            }
        }

        return true;
    }

    #region IReadOnlyList<string>

    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)pathParts).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => pathParts.Length;
    public string this[int index] => pathParts[index];

    #endregion IReadOnlyList<string>

    #region Overrides

    public override string ToString() => PathRaw;

    #endregion Overrides

}

public class VfsPath2
{
    private static StringComparer GetStringComparer(bool isCaseSensitive) => isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    private readonly string[] pathParts;

    private readonly bool isPathCaseSensitive;
    private readonly char[] pathDelimiters;







    #region Overrides

    public override string ToString() => toString;

    public static bool Equals(VfsPath? x, VfsPath? y, bool isCaseSensitive) => Compare(x, y, isCaseSensitive: isCaseSensitive) == 0;
    public static int GetHashCode(VfsPath obj, bool isCaseSensitive) => isCaseSensitive ? HashOrdinal(obj) : HashOrdinalIgnoreCase(obj);
    public static int Compare(VfsPath? x, VfsPath? y, bool isCaseSensitive) => CompareClassEnumerable(GetStringComparer(isCaseSensitive), x, y) ?? 0;

    public virtual bool Equals(VfsPath? y, bool isCaseSensitive) => Equals(this, y, isCaseSensitive: isCaseSensitive);
    public virtual int GetHashCode(bool isCaseSensitive) => GetHashCode(this, isCaseSensitive: isCaseSensitive);
    public virtual int Compare(VfsPath? y, bool isCaseSensitive) => Compare(this, y, isCaseSensitive: isCaseSensitive);

    protected override bool EqualsInternal(VfsPath x, VfsPath y) => Equals(x, y, isCaseSensitive: Default_Path_IsCaseSensitive);
    protected override int GetHashCodeInternal(VfsPath obj) => GetHashCode(obj, isCaseSensitive: Default_Path_IsCaseSensitive);
    protected override int CompareInternal(VfsPath x, VfsPath y) => Compare(x, y, isCaseSensitive: Default_Path_IsCaseSensitive);

    #endregion Overrides

}
