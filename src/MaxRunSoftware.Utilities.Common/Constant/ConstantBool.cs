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
    private static ImmutableArray<string> ConstantBoolArray(params string[] items) => [..items.Select(o => o.ToUpperInvariant()).Distinct()];

    private static readonly ImmutableArray<string> BOOL_TRUE_VALUES = ConstantBoolArray(bool.TrueString, "1", "T", "TRUE", "Y", "YES", "ON");
    private static readonly ImmutableArray<string> BOOL_FALSE_VALUES = ConstantBoolArray(bool.FalseString, "0", "F", "FALSE", "N", "NO", "OFF");

    /// <summary>
    /// Case-Insensitive hashset of boolean true values.
    /// The set is case insensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly FrozenSet<string> Bool_True = Bool_Values_Create(true);

    /// <summary>
    /// Case-Insensitive hashset of boolean false values
    /// The set is case in-sensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly FrozenSet<string> Bool_False = Bool_Values_Create(false);
    
    private static FrozenSet<string> Bool_Values_Create(bool value) => (value ? BOOL_TRUE_VALUES : BOOL_FALSE_VALUES)
        .SelectMany(Bool_Values_Create_HashSet)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .SelectMany(PermuteCase)
        .Distinct()
        .ToFrozenSet();
    
    private static HashSet<string> Bool_Values_Create_HashSet(string item)
    {
        var set = new HashSet<string>();
        
        try { set.Add(item); } catch (Exception) { /* swallow */ }
        try { set.Add(item.ToUpper()); } catch (Exception) { /* swallow */ }
        try { set.Add(item.ToUpperInvariant()); } catch (Exception) { /* swallow */ }
        try { set.Add(item.ToLower()); } catch (Exception) { /* swallow */ }
        try { set.Add(item.ToLowerInvariant()); } catch (Exception) { /* swallow */ }
        
        return set;
    }
    
    /// <summary>
    /// Map of all boolean string values to boolean values
    /// The dictionary is case insensitive in that it contains all permutations of case variations.
    /// </summary>
    public static readonly FrozenDictionary<string, bool> String_Bool = String_Bool_Create();

    private static FrozenDictionary<string, bool> String_Bool_Create()
    {
        var b = new Dictionary<string, bool>();
        foreach (var s in Bool_True) b.TryAdd(string.Intern(s), true);
        foreach (var s in Bool_False) b.TryAdd(string.Intern(s), false);
        return b.ToFrozenDictionary();
    }
}
