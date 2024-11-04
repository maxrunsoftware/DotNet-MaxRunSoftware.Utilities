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

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsEqualCompare
{
    #region IsEqual

    public static bool IsEqual<T>(this T? x, T? y) where T : IEquatable<T>
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }

    public static bool IsEqual<T>(this T? x, T? y) where T : struct, IEquatable<T>
    {
        if (!x.HasValue && !y.HasValue) return true;
        if (!x.HasValue || !y.HasValue) return false;
        return x.Value.Equals(y.Value);
    }

    public static bool IsEqual(this object? x, object? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
        return x.Equals(y);
    }

    public static bool IsEqual(this string? x, string? y) => StringComparer.Ordinal.Equals(x, y);
    public static bool IsEqualOrdinalIgnoreCase(this string? x, string? y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);

    #endregion IsEqual

    #region Compare

    public static int Compare<T>(this T? x, T? y) where T : class, IComparable<T>
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        if (x is string xs && y is string ys) return xs.Compare(ys);
        
        return x.CompareTo(y);
    }
    
    public static int Compare<T>(this ref T? x, ref T? y) where T : struct, IComparable<T>
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;
        return x.Value.CompareTo(y.Value);
    }
    
    public static int Compare(this int? x, int? y)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;
        return x.Value.CompareTo(y.Value);
    }
    
    
    
    public static int Compare<T>(this T? x, T? y, IComparer<T> comparer) where T : class
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;
        return comparer.Compare(x, y);
    }
    
    public static int Compare<T>(this ref T? x, ref T? y, IComparer<T> comparer) where T : struct
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;
        return comparer.Compare(x.Value, y.Value);
    }

    
    public static int Compare<T>(this IEnumerable<T?>? x, IEnumerable<T?>? y, IComparer<T> comparer) where T : class => Compare_Internal(x, y, comparer.Compare);
    public static int Compare<T>(this IEnumerable<T?>? x, IEnumerable<T?>? y) where T : class, IComparable<T> => Compare_Internal(x, y, (xx, yy) => xx.CompareTo(yy));

    private static int Compare_Internal<T>(IEnumerable<T?>? x, IEnumerable<T?>? y, Func<T, T, int> comparer) where T : class
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
            if (!xContinue && !yContinue) return 0;

            var xItem = xEnumerator.Current;
            var yItem = yEnumerator.Current;

            if (!ReferenceEquals(xItem, yItem))
            {
                if (ReferenceEquals(xItem, null)) return -1;
                if (ReferenceEquals(yItem, null)) return 1;
                var c = comparer(xItem, yItem);
                if (c != 0) return c;
            }

        } while (xContinue && yContinue);

        return 0;

    }

    
    public static int Compare<T>(this IEnumerable<T?>? x, IEnumerable<T?>? y, IComparer<T> comparer) where T : struct => Compare_Internal_Struct(x, y, comparer.Compare);

    public static int Compare<T>(this IEnumerable<T?>? x, IEnumerable<T?>? y) where T : struct, IComparable<T> => Compare_Internal_Struct(x, y, (xx, yy) => xx.CompareTo(yy));

    private static int Compare_Internal_Struct<T>(IEnumerable<T?>? x, IEnumerable<T?>? y, Func<T, T, int> comparer) where T : struct
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;
        
        using var xEnumerator = x.GetEnumerator();
        using var yEnumerator = y.GetEnumerator();

        bool xContinue, yContinue;
        do
        {
            xContinue = xEnumerator.MoveNext();
            yContinue = yEnumerator.MoveNext();
            
            if (xContinue && !yContinue) return 1;
            if (!xContinue && yContinue) return -1;
            if (!xContinue && !yContinue) return 0;

            var xItem = xEnumerator.Current;
            var yItem = yEnumerator.Current;

            if (xItem == null)
            {
                if (yItem != null) return -1;
            }
            else if (yItem == null) return 1;
            else
            {
                var c = comparer(xItem.Value, yItem.Value);
                if (c != 0) return c;
            }
            
        } while (xContinue && yContinue);

        return 0;

    }



    public static int Compare<T>(this T?[]? x, T?[]? y) where T : class, IComparable<T> => Compare_Internal(x, y, Compare);
    private static int Compare_Internal<T>(T?[]? x, T?[]? y, Func<T?, T?, int> comparer) where T : class
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(x, null)) return -1;
        if (ReferenceEquals(y, null)) return 1;

        var xLen = x.Length;
        var yLen = y.Length;
        var len = Math.Min(xLen, yLen);

        for (var i = 0; i < len; i++)
        {
            int c;
            if ((c = comparer(x[i], y[i])) != 0) return c;
        }

        if (xLen == yLen) return 0;
        if (xLen < yLen) return -1;
        if (xLen > yLen) return 1;
        
        return 0;
    }
    
    public static int Compare<T>(this T?[]? x, T?[]? y) where T : struct, IComparable<T>
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var xLen = x.Length;
        var yLen = y.Length;
        var len = Math.Min(xLen, yLen);

        for (var i = 0; i < len; i++)
        {
            int c;
            if ((c = Compare(ref x[i], ref y[i])) != 0) return c;
        }

        if (xLen < yLen) return -1;
        if (xLen > yLen) return 1;
        if (xLen == yLen) return 0;
        
        return 0;
    }
    
    public static int Compare(this string? x, string? y)
    {
        int c;
        if ((c = CompareOrdinalIgnoreCase(x, y)) != 0) return c;
        if ((c = CompareOrdinal(x, y)) != 0) return c;
        return 0;
    }
    public static int CompareOrdinal(this string? x, string? y) => StringComparer.Ordinal.Compare(x, y);
    public static int CompareOrdinalIgnoreCase(this string? x, string? y) => StringComparer.OrdinalIgnoreCase.Compare(x, y);

    
    #endregion Compare
}
