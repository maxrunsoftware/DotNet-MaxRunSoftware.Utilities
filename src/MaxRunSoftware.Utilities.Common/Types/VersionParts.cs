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

public sealed class VersionParts : IReadOnlyList<VersionPart>, IEquatable<VersionParts>, IComparable<VersionParts>
{
    private static readonly FrozenSet<char> CHARS = Constant.Chars_A_Z_Upper.Concat(Constant.Chars_A_Z_Lower).ToFrozenSet();
    private static readonly FrozenSet<char> NUMS = Constant.Chars_0_9.ToFrozenSet();

    private readonly ImmutableArray<VersionPart> parts;
    private readonly string toString;
    
    public VersionParts(string value)
    {
        toString = value;
        const int isChar = 1, isNum = 2, isOther = 3;

        var list = new List<VersionPart>();
        var sb = new StringBuilder(50);
        var lastCharType = isOther;

        foreach (var c in value)
        {
            var charType = CHARS.Contains(c) ? isChar : NUMS.Contains(c) ? isNum : isOther;
            if (charType == isOther || charType != lastCharType)
            {
                list.Add(new(sb.ToString()));
                sb.Clear();
            }

            if (charType != isOther) sb.Append(c);
            lastCharType = charType;
        }

        list.Add(new(sb.ToString()));
        parts = [..list.Where(o => !string.IsNullOrWhiteSpace(o.Value))];
        if (parts.Length == 0) throw new ArgumentException($"'{value}' is not a valid version", nameof(value));
        getHashCode = Util.HashEnumerable(parts);
    }

    public override string ToString() => toString;

    #region Other

    public VersionPart? Major => this.GetAtIndexOrDefault(0);
    public VersionPart? Minor => this.GetAtIndexOrDefault(1);
    public VersionPart? Build => this.GetAtIndexOrDefault(2);
    public VersionPart? Revision => this.GetAtIndexOrDefault(3);

    public Version ToSystemVersion()
    {
        var (a, b, c, d) = this.Take4Nullable();
        var exceptionMessage = $"Could not convert '{0}' to int for {typeof(Version).FullNameFormatted()}.{1}";
        if (a != null && a.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(Version.Major), a.Value, string.Format(exceptionMessage, a.Value, nameof(Version.Major)));
        if (b != null && b.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(Version.Minor), b.Value, string.Format(exceptionMessage, b.Value, nameof(Version.Minor)));
        if (c != null && c.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(Version.Build), c.Value, string.Format(exceptionMessage, c.Value, nameof(Version.Build)));
        if (d != null && d.ValueInt == null) throw new ArgumentOutOfRangeException(nameof(Version.Revision), d.Value, string.Format(exceptionMessage, d.Value, nameof(Version.Revision)));

        return new(a?.ValueInt ?? 0, b?.ValueInt ?? 0, c?.ValueInt ?? 0, d?.ValueInt ?? 0);
    }

    public bool IsPreRelease => Count > 0 && this.Any(o => o.IsPreRelease);

    #endregion Other

    #region IReadOnlyList

    public IEnumerator<VersionPart> GetEnumerator() => ((IReadOnlyList<VersionPart>)parts).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => parts.Length;

    public VersionPart this[int index] => parts[index];

    #endregion IReadOnlyList

    #region Equals

    private readonly int getHashCode;
    public override int GetHashCode() => getHashCode;

    public bool Equals(VersionParts? other) => Equals(this, other);
    public override bool Equals(object? obj) => Equals(obj as VersionParts);
    public static bool operator ==(VersionParts? left, VersionParts? right) => Equals(left, right);
    public static bool operator !=(VersionParts? left, VersionParts? right) => !Equals(left, right);

    public static bool Equals(VersionParts? left, VersionParts? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
        var c = Math.Max(left.Count, right.Count);
        for (var i = 0; i < c; i++)
        {
            if (!VersionPart.Equals(left.GetAtIndexOrDefault(i), right.GetAtIndexOrDefault(i))) return false;
        }

        return true;
    }

    #endregion Equals

    #region CompareTo

    public int CompareTo(VersionParts? other) => CompareTo(this, other);
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        if (obj is VersionParts other) return CompareTo(other);
        throw new ArgumentException($"Object must be of type {nameof(VersionParts)}");
    }

    private static int CompareTo(VersionParts? left, VersionParts? right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null) ? 0 : -1;
        }

        if (ReferenceEquals(right, null))
        {
            return ReferenceEquals(left, null) ? 0 : 1;
        }

        var max = Math.Max(left.Count, right.Count);
        for (var i = 0; i < max; i++)
        {
            var x = left.GetAtIndexOrDefault(i);
            var y = right.GetAtIndexOrDefault(i);
            var c = VersionPart.CompareTo(x, y);
            if (c != 0) return c;
        }

        return left.Count.CompareTo(right.Count);
    }

    public static bool operator <(VersionParts? left, VersionParts? right) => CompareTo(left, right) < 0;
    public static bool operator >(VersionParts? left, VersionParts? right) => CompareTo(left, right) > 0;
    public static bool operator <=(VersionParts? left, VersionParts? right) => CompareTo(left, right) <= 0;
    public static bool operator >=(VersionParts? left, VersionParts? right) => CompareTo(left, right) >= 0;

    #endregion CompareTo
}
