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

public static class ExtensionsEquatable
{
    public static bool EqualsNullable<T>(this T? obj, T? other) where T : struct, IEquatable<T>
    {
        if (!obj.HasValue) return !other.HasValue;
        if (!other.HasValue) return false;
        return obj.Value.Equals(other.Value);
    }

    public static bool EqualsNullable<T>(this T? obj, T other) where T : struct, IEquatable<T> => obj.HasValue && obj.Value.Equals(other);

    public static bool EqualsNullable<T>(this T obj, T? other) where T : struct, IEquatable<T> => other.HasValue && obj.Equals(other.Value);

    public static bool EqualsOrdinal(this string? obj, string? other) => StringComparer.Ordinal.Equals(obj, other);
    public static bool EqualsOrdinalIgnoreCase(this string? obj, string? other) => StringComparer.OrdinalIgnoreCase.Equals(obj, other);
}

public static class ExtensionsComparable
{
    public static int CompareToNullable<T>(this T? obj, T? other) where T : struct, IComparable<T>
    {
        if (!obj.HasValue) return other.HasValue ? -1 : 0;
        if (!other.HasValue) return 1;
        return obj.Value.CompareTo(other.Value);
    }

    // ReSharper disable once MergeConditionalExpression
    public static int CompareToNullable<T>(this T? obj, T other) where T : struct, IComparable<T> => !obj.HasValue ? -1 : obj.Value.CompareTo(other);

    public static int CompareToNullable<T>(this T obj, T? other) where T : struct, IComparable<T> => !other.HasValue ? 1 : obj.CompareTo(other.Value);

    public static int CompareToOrdinalIgnoreCaseThenOrdinal(this string? obj, string? other)
    {
        var c = StringComparer.OrdinalIgnoreCase.Compare(obj, other);
        if (c != 0) return c;
        return StringComparer.Ordinal.Compare(obj, other);
    }

    public static int CompareToOrdinal(this string? obj, string? other) => StringComparer.Ordinal.Compare(obj, other);
    public static int CompareToOrdinalIgnoreCase(this string? obj, string? other) => StringComparer.OrdinalIgnoreCase.Compare(obj, other);
}

public static class ExtensionsGetHashCode
{
    public static int GetHashCodeOrdinal(this string? obj) => obj == null ? 0 : StringComparer.Ordinal.GetHashCode(obj);
    public static int GetHashCodeOrdinalIgnoreCase(this string? obj) => obj == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
}
