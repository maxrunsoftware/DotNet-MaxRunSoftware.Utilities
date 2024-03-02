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
    #region StringComparer

    /// <summary>
    /// List of String Comparisons from most restrictive to least
    /// </summary>
    public static readonly ImmutableArray<StringComparer> StringComparers = ImmutableArray.Create(
        StringComparer.Ordinal,
        StringComparer.CurrentCulture,
        StringComparer.InvariantCulture,
        StringComparer.OrdinalIgnoreCase,
        StringComparer.CurrentCultureIgnoreCase,
        StringComparer.InvariantCultureIgnoreCase
    );

    /// <summary>
    /// Map of StringComparer to StringComparison
    /// </summary>
    public static readonly ImmutableDictionary<StringComparer, StringComparison> StringComparer_StringComparison = StringComparer_StringComparison_Create();

    private static ImmutableDictionary<StringComparer, StringComparison> StringComparer_StringComparison_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<StringComparer, StringComparison>();
        b.TryAdd(StringComparer.CurrentCulture, StringComparison.CurrentCulture);
        b.TryAdd(StringComparer.CurrentCultureIgnoreCase, StringComparison.CurrentCultureIgnoreCase);
        b.TryAdd(StringComparer.InvariantCulture, StringComparison.InvariantCulture);
        b.TryAdd(StringComparer.InvariantCultureIgnoreCase, StringComparison.InvariantCultureIgnoreCase);
        b.TryAdd(StringComparer.Ordinal, StringComparison.Ordinal);
        b.TryAdd(StringComparer.OrdinalIgnoreCase, StringComparison.OrdinalIgnoreCase);
        return b.ToImmutableDictionary();
    }

    #endregion StringComparer

    #region StringComparison

    /// <summary>
    /// List of String Comparisons from most restrictive to least
    /// </summary>
    public static readonly ImmutableArray<StringComparison> StringComparisons = ImmutableArray.Create(
        StringComparison.Ordinal,
        StringComparison.CurrentCulture,
        StringComparison.InvariantCulture,
        StringComparison.OrdinalIgnoreCase,
        StringComparison.CurrentCultureIgnoreCase,
        StringComparison.InvariantCultureIgnoreCase
    );

    /// <summary>
    /// Map of StringComparison to StringComparer
    /// </summary>
    public static readonly ImmutableDictionary<StringComparison, StringComparer> StringComparison_StringComparer = StringComparison_StringComparer_Create();

    private static ImmutableDictionary<StringComparison, StringComparer> StringComparison_StringComparer_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<StringComparison, StringComparer>();
        b.TryAdd(StringComparison.CurrentCulture, StringComparer.CurrentCulture);
        b.TryAdd(StringComparison.CurrentCultureIgnoreCase, StringComparer.CurrentCultureIgnoreCase);
        b.TryAdd(StringComparison.InvariantCulture, StringComparer.InvariantCulture);
        b.TryAdd(StringComparison.InvariantCultureIgnoreCase, StringComparer.InvariantCultureIgnoreCase);
        b.TryAdd(StringComparison.Ordinal, StringComparer.Ordinal);
        b.TryAdd(StringComparison.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase);
        return b.ToImmutableDictionary();
    }

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

    private sealed class StringComparerOrdinalIgnoreCaseOrdinal : StringComparer
    {
        private readonly StringComparer ordinal = Ordinal;
        private readonly StringComparer ordinalIgnoreCase = OrdinalIgnoreCase;
        private readonly bool reversed;
        public StringComparerOrdinalIgnoreCaseOrdinal(bool reversed) => this.reversed = reversed;

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

    public sealed class CharComparer : IEqualityComparer<char>, IComparer<char>
    {
        private readonly Func<char, char> converter;
        public CharComparer(Func<char, char> converter) => this.converter = converter;
        public bool Equals(char x, char y) => converter(x) == converter(y);
        public int GetHashCode(char obj) => converter(obj).GetHashCode();
        public int Compare(char x, char y) => converter(x).CompareTo(converter(y));
    }

    #endregion CharComparer
}
