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

using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common;

public abstract class ComparerBaseStruct<T> : ComparerBase, IEqualityComparer<T>, IEqualityComparer, IComparer<T>, IComparer where T : struct
{
    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        if (x is T xTyped)
        {
            if (y is T yTypedInner) return Equals(xTyped, yTypedInner);
            return Equals(xTyped, y as T?);
        }

        if (y is T yTyped)
        {
            if (x is T xTypedInner) return Equals(xTypedInner, yTyped);
            return Equals(x as T?, yTyped);
        }

        // ReSharper disable once UsePatternMatching
        var xTypedNullable = x as T?;
        var yTypedNullable = y as T?;

        if (!xTypedNullable.HasValue)
        {
            if (!yTypedNullable.HasValue) return true;
            return Equals(xTypedNullable, yTypedNullable.Value);
        }

        if (!yTypedNullable.HasValue)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!xTypedNullable.HasValue) return true;
            return Equals(xTypedNullable.Value, yTypedNullable);
        }

        Debug.Assert(xTypedNullable.HasValue && yTypedNullable.HasValue);
        return Equals(xTypedNullable.Value, yTypedNullable.Value);
    }

    public bool Equals(T x, T y) => EqualsInternal(x, y);

    public bool Equals(T x, T? y) => y.HasValue && Equals(x, y.Value);

    public bool Equals(T? x, T y) => x.HasValue && Equals(x.Value, y);

    public bool Equals(T? x, T? y)
    {
        if (x.HasValue) return Equals(x.Value, y);
        if (y.HasValue) return Equals(x, y.Value);
        return true; // Both null
    }


    protected abstract bool EqualsInternal(T x, T y);

    public int GetHashCode(object obj)
    {
        if (ReferenceEquals(obj, null)) throw new ArgumentNullException(nameof(obj));

        if (obj is T objTyped) return GetHashCode(objTyped);
        // ReSharper disable once UsePatternMatching
        var objTypedNullable = obj as T?;
        if (objTypedNullable.HasValue) return GetHashCode(objTypedNullable.Value);

        Debug.Assert(!ReferenceEquals(obj, null));
        Debug.Assert(!objTypedNullable.HasValue);
        Debug.Assert(obj is not T);

        return obj.GetHashCode(); // .NET StringComparer.GetHashCode of object that is not a string
    }

    public int GetHashCode(T obj) => GetHashCodeInternal(obj);

    protected abstract int GetHashCodeInternal(T obj);

    public int Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        if (x is T xTyped)
        {
            if (y is T yTypedInner) return Compare(xTyped, yTypedInner);
            return Compare(xTyped, y as T?);
        }

        if (y is T yTyped)
        {
            if (x is T xTypedInner) return Compare(xTypedInner, yTyped);
            return Compare(x as T?, yTyped);
        }

        Debug.Assert(!ReferenceEquals(x, null) && !ReferenceEquals(y, null));

        // ReSharper disable once UsePatternMatching
        var xTypedNullable = x as T?;
        var yTypedNullable = y as T?;

        if (xTypedNullable.HasValue) return Compare(xTypedNullable.Value, yTypedNullable);
        if (yTypedNullable.HasValue) return Compare(xTypedNullable, yTypedNullable.Value);

        Debug.Assert(!xTypedNullable.HasValue && !yTypedNullable.HasValue);
        return 0;
    }

    public int Compare(T x, T y) => CompareInternal(x, y);

    public int Compare(T x, T? y) => y.HasValue ? Compare(x, y.Value) : 1;
    public int Compare(T? x, T y) => x.HasValue ? Compare(x.Value, y) : -1;

    public int Compare(T? x, T? y)
    {
        if (!x.HasValue) return y.HasValue ? -1 : 0;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!y.HasValue) return x.HasValue ? 1 : 0;
        return Compare(x.Value, y.Value);
    }

    protected abstract int CompareInternal(T x, T y);
}
