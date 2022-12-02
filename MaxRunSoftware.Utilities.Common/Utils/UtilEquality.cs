﻿// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

public static partial class Util
{
    #region IsEqual

    public static bool IsEqual<T1>(T1 o11, T1 o21) => EqualityComparer<T1>.Default.Equals(o11, o21);

    public static bool IsEqual<T1, T2>(T1 o11, T1 o21, T2 o12, T2 o22) => EqualityComparer<T1>.Default.Equals(o11, o21) && EqualityComparer<T2>.Default.Equals(o12, o22);

    public static bool IsEqual<T1, T2, T3>(T1 o11, T1 o21, T2 o12, T2 o22, T3 o13, T3 o23) => EqualityComparer<T1>.Default.Equals(o11, o21) && EqualityComparer<T2>.Default.Equals(o12, o22) && EqualityComparer<T3>.Default.Equals(o13, o23);

    public static bool IsEqual<T1, T2, T3, T4>(T1 o11, T1 o21, T2 o12, T2 o22, T3 o13, T3 o23, T4 o14, T4 o24) => EqualityComparer<T1>.Default.Equals(o11, o21) && EqualityComparer<T2>.Default.Equals(o12, o22) && EqualityComparer<T3>.Default.Equals(o13, o23) && EqualityComparer<T4>.Default.Equals(o14, o24);

    public static bool IsEqualCaseInsensitive(string o11, string o21) => StringComparer.OrdinalIgnoreCase.Equals(o11, o21);

    public static bool IsEqualCaseInsensitive(string o11, string o21, string o12, string o22) => StringComparer.OrdinalIgnoreCase.Equals(o11, o21) && StringComparer.OrdinalIgnoreCase.Equals(o12, o22);

    public static bool IsEqualCaseInsensitive(string o11, string o21, string o12, string o22, string o13, string o23) => StringComparer.OrdinalIgnoreCase.Equals(o11, o21) && StringComparer.OrdinalIgnoreCase.Equals(o12, o22) && StringComparer.OrdinalIgnoreCase.Equals(o13, o23);

    public static bool IsEqualCaseInsensitive(string o11, string o21, string o12, string o22, string o13, string o23, string o14, string o24) => StringComparer.OrdinalIgnoreCase.Equals(o11, o21) && StringComparer.OrdinalIgnoreCase.Equals(o12, o22) && StringComparer.OrdinalIgnoreCase.Equals(o13, o23) && StringComparer.OrdinalIgnoreCase.Equals(o14, o24);

    #endregion IsEqual

    public static int Compare<TEnumerable1, TEnumerable2, TItem>(TEnumerable1? enumerable1, TEnumerable2? enumerable2, IComparer<TItem> comparer) where TEnumerable1 : IEnumerable<TItem> where TEnumerable2 : IEnumerable<TItem>
    {
        if (enumerable1 == null) return enumerable2 == null ? 0 : -1;

        if (enumerable2 == null) return 1;

        using (var e1 = enumerable1.GetEnumerator())
        {
            using (var e2 = enumerable2.GetEnumerator())
            {
                while (true)
                {
                    var mn1 = e1.MoveNext();
                    var mn2 = e2.MoveNext();
                    if (!mn1) return mn2 ? -1 : 0;

                    if (!mn2) return 1;

                    var o1 = e1.Current;
                    var o2 = e2.Current;

                    var r = comparer.Compare(o1, o2);

                    if (r != 0) return r;
                }
            }
        }
    }
}
