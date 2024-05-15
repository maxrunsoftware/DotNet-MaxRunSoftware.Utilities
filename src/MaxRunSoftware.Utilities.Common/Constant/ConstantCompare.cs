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

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    private static FrozenDictionary<TKey, TValue> FD<TKey, TValue>(IEnumerable<(TKey, TValue)> items) where TKey : notnull
    {
        var d = new Dictionary<TKey, TValue>();
        foreach (var item in items) d.TryAdd(item.Item1, item.Item2);
        return d.ToFrozenDictionary();
    }
    
    private static readonly ImmutableArray<(StringComparer, StringComparison)> StringComparer_StringComparison_Items =
    [
        (StringComparer.Ordinal, StringComparison.Ordinal),
        (StringComparer.CurrentCulture, StringComparison.CurrentCulture),
        (StringComparer.InvariantCulture, StringComparison.InvariantCulture),
        (StringComparer.OrdinalIgnoreCase, StringComparison.OrdinalIgnoreCase),
        (StringComparer.CurrentCultureIgnoreCase, StringComparison.CurrentCultureIgnoreCase),
        (StringComparer.InvariantCultureIgnoreCase, StringComparison.InvariantCultureIgnoreCase),
    ];
    
    #region StringComparer
    
    /// <summary>
    /// List of String Comparisons from most restrictive to least
    /// </summary>
    public static readonly ImmutableArray<StringComparer> StringComparers = [..StringComparer_StringComparison_Items.Select(o => o.Item1)];

    /// <summary>
    /// Map of StringComparer to StringComparison
    /// </summary>
    public static readonly FrozenDictionary<StringComparer, StringComparison> StringComparer_StringComparison = StringComparer_StringComparison_Items.ToFrozenDictionary(o => o.Item1, o => o.Item2);
    
    #endregion StringComparer

    #region StringComparison
    
    /// <summary>
    /// List of String Comparisons from most restrictive to least
    /// </summary>
    public static readonly ImmutableArray<StringComparison> StringComparisons = [..StringComparer_StringComparison_Items.Select(o => o.Item2)];

    /// <summary>
    /// Map of StringComparison to StringComparer
    /// </summary>
    public static readonly FrozenDictionary<StringComparison, StringComparer> StringComparison_StringComparer = StringComparer_StringComparison_Items.ToFrozenDictionary(o => o.Item2, o => o.Item1);

    #endregion StringComparison

    #region StringComparer_OrdinalIgnoreCase_Ordinal

    /// <summary>
    /// Comparer first sorting OrdinalIgnoreCase then sub-sorting Ordinal.
    /// Example: b,B,a,A,C,c becomes A,a,B,b,C,c
    /// </summary>
    public static readonly StringComparer StringComparer_OrdinalIgnoreCase_Ordinal = new StringComparerOrdinalIgnoreCaseOrdinal(false);

    /// <summary>
    /// Comparer first sorting OrdinalIgnoreCase then sub-sorting Ordinal Reversed.
    /// Example: b,B,a,A,C,c becomes a,A,b,B,c,C
    /// </summary>
    public static readonly StringComparer StringComparer_OrdinalIgnoreCase_OrdinalReversed = new StringComparerOrdinalIgnoreCaseOrdinal(true);

    private sealed class StringComparerOrdinalIgnoreCaseOrdinal(bool reversed) : StringComparer
    {
        private readonly StringComparer ordinal = Ordinal;
        private readonly StringComparer ordinalIgnoreCase = OrdinalIgnoreCase;
        
        public override int Compare(string? x, string? y)
        {
            var c = ordinalIgnoreCase.Compare(x, y);
            if (c != 0) return c;
            return reversed ? ordinal.Compare(y, x) : ordinal.Compare(x, y);
        }
        public override bool Equals(string? x, string? y) => ordinal.Equals(x, y);
        public override int GetHashCode(string obj) => ordinal.GetHashCode(obj);
    }

    #endregion StringComparer_OrdinalIgnoreCase_Ordinal

    #region CharComparer

    public static readonly CharComparer CharComparer_Lower = new(char.ToLower);
    public static readonly CharComparer CharComparer_LowerInvariant = new(char.ToLowerInvariant);

    public static readonly CharComparer CharComparer_Upper = new(char.ToUpper);
    public static readonly CharComparer CharComparer_UpperInvariant = new(char.ToUpperInvariant);

    public static readonly CharComparer CharComparer_Ordinal = new(c => c);
    public static readonly CharComparer CharComparer_OrdinalIgnoreCase = new(char.ToUpperInvariant);

    public sealed class CharComparer(Func<char, char> converter) : IEqualityComparer<char>, IComparer<char>
    {
        public bool Equals(char x, char y) => converter(x) == converter(y);
        public int GetHashCode(char obj) => converter(obj).GetHashCode();
        public int Compare(char x, char y) => converter(x).CompareTo(converter(y));
    }

    #endregion CharComparer
}
