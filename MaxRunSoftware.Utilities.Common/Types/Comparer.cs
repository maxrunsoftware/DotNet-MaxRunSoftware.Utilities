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

// ReSharper disable ConditionIsAlwaysTrueOrFalse

// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

public abstract class ComparerBase
{
    #region Helpers

    protected static int Hash<TT>(TT? obj) => Util.Hash(obj);
    protected static int HashEnumerable<TT>(IEnumerable<TT?> enumerable) => Util.HashEnumerable(enumerable);
    protected static int Hash(params object?[] objs) => objs.Length == 0 ? 0 : Util.HashEnumerable(objs);
    protected static int HashOrdinal(string? s) => s == null ? 0 : StringComparer.Ordinal.GetHashCode(s);
    protected static int HashOrdinalIgnoreCase(string? s) => s == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(s);
    protected static int HashOrdinal(IEnumerable<string?>? s) => s == null ? 0 : Util.HashEnumerable(s.Select(HashOrdinal));
    protected static int HashOrdinalIgnoreCase(IEnumerable<string?>? s) => s == null ? 0 : Util.HashEnumerable(s.Select(HashOrdinalIgnoreCase));

    protected static bool EqualsClass<TT>(TT? x, TT? y) where TT : class, IEquatable<TT>
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }

    protected static bool EqualsClassEnumerable<TT>(IEnumerable<TT?>? x, IEnumerable<TT?>? y, bool skipCount = false) where TT : class, IEquatable<TT>
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        // ReSharper disable PossibleMultipleEnumeration
        if (!skipCount)
        {
            static int? GetCount(IEnumerable<TT?> enumerable) => enumerable switch
            {
                IReadOnlyList<TT?> readOnlyList => readOnlyList.Count,
                ICollection<TT?> collectionGeneric => collectionGeneric.Count,
                ICollection collection => collection.Count,
                _ => null,
            };

            var xCount = GetCount(x);
            var yCount = GetCount(y);
            if (xCount != null && yCount != null && xCount.Value != yCount.Value) return false;
        }

        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();
        // ReSharper restore PossibleMultipleEnumeration

        bool xContinue, yContinue;
        do
        {
            xContinue = xEnumerator.MoveNext();
            yContinue = yEnumerator.MoveNext();
            if (xContinue != yContinue) return false;
            if (!xContinue && !yContinue) return true;
            if (!EqualsClass(xEnumerator.Current, yEnumerator.Current)) return false;
        } while (xContinue && yContinue);

        return true;
    }



    protected static bool EqualsStruct<TT>(TT x, TT y) where TT : struct => x.Equals(y);
    protected static bool EqualsStruct<TT>(TT x, TT? y) where TT : struct => y.HasValue && EqualsStruct(x, y.Value);
    protected static bool EqualsStruct<TT>(TT? x, TT y) where TT : struct => x.HasValue && EqualsStruct(x.Value, y);
    protected static bool EqualsStruct<TT>(TT? x, TT? y) where TT : struct
    {
        if (!x.HasValue) return !y.HasValue;
        if (!y.HasValue) return !x.HasValue;
        return EqualsStruct(x.Value, y.Value);
    }

    protected static bool EqualsObject(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }

    protected static bool EqualsOrdinal(string? x, string? y) => StringComparer.Ordinal.Equals(x, y);
    protected static bool EqualsOrdinalIgnoreCase(string? x, string? y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);

    protected static int? CompareNullable(int i) => i == 0 ? null : i;

    protected static int? CompareClass<TT>(TT? x, TT? y) where TT : class, IComparable<TT>
    {
        if (ReferenceEquals(x, y)) return null;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        return CompareNullable(x.CompareTo(y));
    }

    protected static int? CompareStruct<TT>(TT x, TT y) where TT : struct, IComparable<TT> => CompareNullable(x.CompareTo(y));
    protected static int? CompareStruct<TT>(TT x, TT? y) where TT : struct, IComparable<TT> => y.HasValue ? CompareStruct(x, y.Value) : 1;
    protected static int? CompareStruct<TT>(TT? x, TT y) where TT : struct, IComparable<TT> => x.HasValue ? CompareStruct(x.Value, y) : -1;
    protected static int? CompareStruct<TT>(TT? x, TT? y) where TT : struct, IComparable<TT>
    {
        if (x.HasValue) return CompareStruct(x.Value, y);
        if (y.HasValue) return CompareStruct(x, y.Value);
        return null;
    }

    protected static int? CompareEnumerable<TT>(TT x, TT y) where TT : struct, Enum => CompareNullable(x.CompareTo(y));
    protected static int? CompareEnumerable<TT>(TT x, TT? y) where TT : struct, Enum => y.HasValue ? CompareEnumerable(x, y.Value) : 1;
    protected static int? CompareEnumerable<TT>(TT? x, TT y) where TT : struct, Enum => x.HasValue ? CompareEnumerable(x.Value, y) : -1;
    protected static int? CompareEnumerable<TT>(TT? x, TT? y) where TT : struct, Enum
    {
        if (x.HasValue) return CompareEnumerable(x.Value, y);
        if (y.HasValue) return CompareEnumerable(x, y.Value);
        return null;
    }

    protected static int? CompareObject(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return null;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        if (x is IComparable xComparable) return CompareNullable(xComparable.CompareTo(y));
        throw new ArgumentException($"Object {x.GetType().FullNameFormatted()} does not implement {nameof(IComparable)}", nameof(x));
    }

    protected static int? CompareOrdinal(string? x, string? y) => CompareNullable(StringComparer.Ordinal.Compare(x, y));
    protected static int? CompareOrdinalIgnoreCase(string? x, string? y) => CompareNullable(StringComparer.OrdinalIgnoreCase.Compare(x, y));
    protected static int? CompareOrdinalIgnoreCaseThenOrdinal(string? x, string? y) => CompareNullable(Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(x, y));

    protected static int? CompareOrdinal(IEnumerable<string?>? x, IEnumerable<string?>? y) => CompareStringEnumerable(StringComparer.Ordinal, x, y);
    protected static int? CompareOrdinalIgnoreCase(IEnumerable<string?>? x, IEnumerable<string?>? y) => CompareStringEnumerable(StringComparer.OrdinalIgnoreCase, x, y);
    protected static int? CompareOrdinalIgnoreCaseThenOrdinal(IEnumerable<string?>? x, IEnumerable<string?>? y) => CompareStringEnumerable(Constant.StringComparer_OrdinalIgnoreCase_Ordinal, x, y);
    private static int? CompareStringEnumerable(IComparer<string> comparer, IEnumerable<string?>? x, IEnumerable<string?>? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();

        bool xContinue, yContinue;
        do
        {
            xContinue = xEnumerator.MoveNext();
            yContinue = yEnumerator.MoveNext();
            if (xContinue && !yContinue) return 1;
            if (!xContinue && yContinue) return -1;
            if (!xContinue && !yContinue) return null;
            var c = comparer.Compare(xEnumerator.Current, yEnumerator.Current);
            if (c != 0) return c;
        } while (xContinue && yContinue);

        return null;
    }

    #endregion Helpers
}
