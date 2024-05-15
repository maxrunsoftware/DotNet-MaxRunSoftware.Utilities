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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: Guid(Constant.Id)]

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
[PublicAPI]
public static partial class Constant
{
    public const string Id = "461985d6-d681-4a0f-b110-547f3beaf967";
    public static readonly Guid Id_Guid = new(Id);

    #region Helpers
    
    private static void LogError(Exception? exception, [CallerMemberName] string memberName = "")
    {
        var msg = nameof(Constant) + "." + memberName + "() failed.";
        if (exception != null)
        {
            try
            {
                var err = exception.ToString();
                msg = msg + " " + err;
            }
            catch (Exception)
            {
                try
                {
                    var err = exception.Message;
                    msg = msg + " " + err;
                }
                catch (Exception)
                {
                    try
                    {
                        var err = exception.GetType().FullName;
                        msg = msg + " " + err;
                    }
                    catch (Exception) { }
                }
            }
        }

        try { Debug.WriteLine(msg); }
        catch (Exception)
        {
            try { Console.Error.WriteLine(msg); }
            catch (Exception)
            {
                try { Console.WriteLine(msg); }
                catch { }
            }
        }
    }

    private static string? TrimOrNull(string? str)
    {
        if (str == null) return null;
        str = str.Trim();
        return str.Length == 0 ? null : str;
    }

    private static List<string> PermuteCase(string s)
    {
        // https://stackoverflow.com/a/905377
        var listPermutations = new List<string>();
        var array = s.ToLower().ToCharArray();
        var iterations = (1 << array.Length) - 1;
        for (var i = 0; i <= iterations; i++)
        {
            for (var j = 0; j < array.Length; j++)
            {
                array[j] = (i & (1 << j)) != 0
                    ? char.ToUpper(array[j])
                    : char.ToLower(array[j]);
            }

            listPermutations.Add(new(array));
        }

        return listPermutations;
    }
    
    private static FrozenDictionary<TKey, TValue> ConstantToFrozenDictionaryTry<TKey, TValue>(this IEnumerable<(TKey, TValue)> items, IEqualityComparer<TKey>? equalityComparer = null) where TKey : notnull
    {
        var d = equalityComparer == null ? new Dictionary<TKey, TValue>() : new Dictionary<TKey, TValue>(equalityComparer);
        
        foreach (var (k, v) in items)
        {
            d.TryAdd(k, v);
        }
        
        return d.ToFrozenDictionary(equalityComparer);
    }
    
    #endregion Helpers
}
