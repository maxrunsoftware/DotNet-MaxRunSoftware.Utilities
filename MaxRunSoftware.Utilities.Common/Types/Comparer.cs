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

    protected static int H<TT>(TT? obj) => Util.Hash(obj);
    protected static int H(params object?[] objs) => objs.Length == 0 ? 0 : Util.HashEnumerable(objs);
    protected static int HOrdinal(string? s) => s == null ? 0 : StringComparer.Ordinal.GetHashCode(s);
    protected static int HOrdinalIgnoreCase(string? s) => s == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(s);

    protected static bool EC<TT>(TT? x, TT? y) where TT : class, IEquatable<TT>
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }
    protected static bool ES<TT>(TT x, TT y) where TT : struct => x.Equals(y);
    protected static bool ES<TT>(TT x, TT? y) where TT : struct => y.HasValue && ES(x, y.Value);
    protected static bool ES<TT>(TT? x, TT y) where TT : struct => x.HasValue && ES(x.Value, y);
    protected static bool ES<TT>(TT? x, TT? y) where TT : struct
    {
        if (!x.HasValue) return !y.HasValue;
        if (!y.HasValue) return !x.HasValue;
        return ES(x.Value, y.Value);
    }

    protected static bool EO(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }

    protected static bool EOrdinal(string? x, string? y) => StringComparer.Ordinal.Equals(x, y);
    protected static bool EOrdinalIgnoreCase(string? x, string? y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);

    protected static int? CNullable(int i) => i == 0 ? null : i;

    protected static int? CC<TT>(TT? x, TT? y) where TT : class, IComparable<TT>
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        return CNullable(x.CompareTo(y));
    }

    protected static int? CS<TT>(TT x, TT y) where TT : struct, IComparable<TT> => CNullable(x.CompareTo(y));
    protected static int? CS<TT>(TT x, TT? y) where TT : struct, IComparable<TT> => y.HasValue ? CS(x, y.Value) : 1;
    protected static int? CS<TT>(TT? x, TT y) where TT : struct, IComparable<TT> => x.HasValue ? CS(x.Value, y) : -1;
    protected static int? CS<TT>(TT? x, TT? y) where TT : struct, IComparable<TT>
    {
        if (x.HasValue) return CS(x.Value, y);
        if (y.HasValue) return CS(x, y.Value);
        return null;
    }

    protected static int? CE<TT>(TT x, TT y) where TT : struct, Enum => CNullable(x.CompareTo(y));
    protected static int? CE<TT>(TT x, TT? y) where TT : struct, Enum => y.HasValue ? CE(x, y.Value) : 1;
    protected static int? CE<TT>(TT? x, TT y) where TT : struct, Enum => x.HasValue ? CE(x.Value, y) : -1;
    protected static int? CE<TT>(TT? x, TT? y) where TT : struct, Enum
    {
        if (x.HasValue) return CE(x.Value, y);
        if (y.HasValue) return CE(x, y.Value);
        return null;
    }

    protected static int? CO(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        if (x is IComparable xComparable) return CNullable(xComparable.CompareTo(y));
        throw new ArgumentException($"Object {x.GetType().FullNameFormatted()} does not implement {nameof(IComparable)}", nameof(x));
    }

    protected static int? COrdinal(string x, string y) => CNullable(StringComparer.Ordinal.Compare(x, y));
    protected static int? COrdinalIgnoreCase(string x, string y) => CNullable(StringComparer.OrdinalIgnoreCase.Compare(x, y));
    protected static int? COrdinalIgnoreCaseThenOrdinal(string x, string y) => CNullable(Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(x, y));

    #endregion Helpers
}
