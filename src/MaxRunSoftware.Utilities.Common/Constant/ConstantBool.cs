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
    private static ImmutableArray<string> ConstantBoolArray(params string[] items) =>
        CreateArray(items.Select(o => o.ToUpperInvariant()).Distinct().ToArray());

    private static readonly ImmutableArray<string> BOOL_TRUE_VALUES = ConstantBoolArray(bool.TrueString, "1", "T", "TRUE", "Y", "YES", "ON");
    private static readonly ImmutableArray<string> BOOL_FALSE_VALUES = ConstantBoolArray(bool.FalseString, "0", "F", "FALSE", "N", "NO", "OFF");

    /// <summary>
    /// Case-Insensitive hashset of boolean true values.
    /// The set is case insensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly ImmutableHashSet<string> Bool_True = Bool_Values_Create(true);

    /// <summary>
    /// Case-Insensitive hashset of boolean false values
    /// The set is case insensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly ImmutableHashSet<string> Bool_False = Bool_Values_Create(false);

    private static ImmutableHashSet<string> Bool_Values_Create(bool value)
    {
        var array = value ? BOOL_TRUE_VALUES : BOOL_FALSE_VALUES;
        var hs = new HashSet<string>(
            array.SelectMany(arrayItem => new[]
            {
                arrayItem,
                arrayItem.ToUpper(),
                arrayItem.ToUpperInvariant(),
                arrayItem.ToLower(),
                arrayItem.ToLowerInvariant(),
            }.Concat(PermuteCase(arrayItem))));

        var b = ImmutableHashSet.CreateBuilder<string>();
        foreach (var item in hs) b.Add(string.Intern(item));
        return b.ToImmutable();
    }

    /// <summary>
    /// Map of all boolean string values to boolean values
    /// The dictionary is case insensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly ImmutableDictionary<string, bool> String_Bool = String_Bool_Create();

    private static ImmutableDictionary<string, bool> String_Bool_Create()
    {
        var b = ImmutableDictionary.CreateBuilder<string, bool>();
        foreach (var s in Bool_True) b.Add(string.Intern(s), true);
        foreach (var s in Bool_False) b.Add(string.Intern(s), false);
        return b.ToImmutable();
    }
}
