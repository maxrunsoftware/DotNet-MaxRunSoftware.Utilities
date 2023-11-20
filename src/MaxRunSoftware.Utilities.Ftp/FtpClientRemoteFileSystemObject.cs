// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Ftp;

public enum FtpClientRemoteFileSystemObjectType
{
    Unknown,
    Directory,
    File,
    Link
}

public class FtpClientRemoteFileSystemObjectComparer : IComparer<FtpClientRemoteFileSystemObject>
{
    public string DirectorySeparator { get; }

    public FtpClientRemoteFileSystemObjectComparer(string directorySeparator) => DirectorySeparator = directorySeparator;

    public int Compare(FtpClientRemoteFileSystemObject? x, FtpClientRemoteFileSystemObject? y)
    {
        if (ReferenceEquals(x, null))
        {
            return ReferenceEquals(y, null) ? 0 : -1;
        }

        if (ReferenceEquals(y, null)) return 1;

        static string?[] Split(string name, string separator)
        {
            var parts = name.Split(separator).TrimOrNull();
            while (parts.Length > 0 && parts[0] == null) parts = parts.RemoveHead();
            while (parts.Length > 0 && parts[^1] == null) parts = parts.RemoveTail();
            return parts;
        }

        var xParts = Split(x.NameFull, DirectorySeparator);
        var yParts = Split(y.NameFull, DirectorySeparator);

        int c;
        var len = Math.Min(xParts.Length, yParts.Length);
        for (var i = 0; i < len; i++)
        {
            var xPart = xParts[i];
            var yPart = yParts[i];
            c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(xPart, yPart);
            if (c != 0) return c;
        }

        c = xParts.Length.CompareTo(yParts.Length);
        if (c != 0) return c;

        return x.Type.CompareTo(y.Type);

    }
}

public class FtpClientRemoteFileSystemObject : IEquatable<FtpClientRemoteFileSystemObject>
{
    private readonly int getHashCode;
    public string Name { get; }
    public string NameFull { get; }
    public FtpClientRemoteFileSystemObjectType Type { get; }

    public FtpClientRemoteFileSystemObject(string name, string nameFull, FtpClientRemoteFileSystemObjectType type)
    {
        Name = name;
        NameFull = nameFull;
        Type = type;
        getHashCode = Util.Hash(Type, NameFull);
    }


    /// <summary>
    /// Checks to see if our FullName matches a pathOrPattern value.
    /// <br />if FullName=/dir1/file1.txt  pathOrPattern=/*/file?.txt  isCaseSensitive=true  IsMatch=true
    /// <br />if FullName=/dir1/file1.txt  pathOrPattern=/*/FILE?.TXT  isCaseSensitive=true  IsMatch=false
    /// </summary>
    /// <param name="pathOrPattern"></param>
    /// <param name="isCaseSensitive"></param>
    /// <returns></returns>
    public bool IsMatch(string pathOrPattern, bool isCaseSensitive)
    {
        pathOrPattern = pathOrPattern.CheckNotNullTrimmed(nameof(pathOrPattern));
        var source = pathOrPattern.StartsWith("/") ? NameFull : Name;
        return source.EqualsWildcard(pathOrPattern, !isCaseSensitive);
    }

    public override int GetHashCode() => getHashCode;
    public override bool Equals(object? obj) => Equals(obj as FtpClientRemoteFileSystemObject);
    public bool Equals(FtpClientRemoteFileSystemObject? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;

        if (GetHashCode() != other.GetHashCode()) return false;
        if (Type != other.Type) return false;
        if (!StringComparer.Ordinal.Equals(NameFull, other.NameFull)) return false;

        return true;
    }

    public override string ToString()
    {
        return $"[{Type.ToString()[0]}] {NameFull}";
    }
}
