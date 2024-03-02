// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

public abstract class ComparerBaseClass<T> : ComparerBase, IEqualityComparer<T>, IEqualityComparer, IComparer<T>, IComparer where T : class
{
    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;

        var xTyped = x as T;
        var yTyped = y as T;
        if (ReferenceEquals(xTyped, null))
        {
            if (ReferenceEquals(yTyped, null)) return x.Equals(y); // Neither are of type T
            return false; // x is not type T but y is
        }

        if (ReferenceEquals(yTyped, null))
        {
            if (ReferenceEquals(xTyped, null)) return x.Equals(y); // Neither are of type T
            return false; // y is not type T but x is
        }

        Debug.Assert(!ReferenceEquals(xTyped, null) && !ReferenceEquals(yTyped, null));
        return EqualsInternal(xTyped, yTyped);
    }

    public bool Equals(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        return EqualsInternal(x, y);
    }

    protected abstract bool EqualsInternal(T x, T y);

    public int GetHashCode(object obj)
    {
        if (ReferenceEquals(obj, null)) throw new ArgumentNullException(nameof(obj));
        if (obj is T objTyped) return GetHashCode(objTyped);
        return obj.GetHashCode(); // .NET StringComparer.GetHashCode of object that is not a string
    }

    public int GetHashCode(T obj)
    {
        if (ReferenceEquals(obj, null)) throw new ArgumentNullException(nameof(obj));
        return GetHashCodeInternal(obj);
    }

    protected abstract int GetHashCodeInternal(T obj);

    public int Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return ReferenceEquals(y, null) ? 0 : -1;
        if (ReferenceEquals(y, null)) return ReferenceEquals(x, null) ? 0 : 1;

        if (x is not T xTyped) throw new ArgumentException($"Object must be of type {typeof(T).FullNameFormatted()} but was instead type {x.GetType().FullNameFormatted()}.", nameof(x));
        if (y is not T yTyped) throw new ArgumentException($"Object must be of type {typeof(T).FullNameFormatted()} but was instead type {y.GetType().FullNameFormatted()}.", nameof(y));

        return CompareInternal(xTyped, yTyped);
    }

    public int Compare(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        return CompareInternal(x, y);
    }

    protected abstract int CompareInternal(T x, T y);
}
