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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

// ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

namespace MaxRunSoftware.Utilities.Common;

public sealed class DictionaryWeakType<TValue> : IReadOnlyDictionary<Type, TValue> where TValue : notnull
{
    private sealed class Value
    {
        public readonly object obj;
        public Value(object obj)
        {
            this.obj = obj;
        }
    }

    private readonly ConditionalWeakTable<Type, Value> table = new();

    public TValue GetOrAdd(Type type, Func<Type, TValue> funcFactory) => (TValue)table.GetValue(type, key => new(funcFactory(key))).obj;

    public IEnumerator<KeyValuePair<Type, TValue>> GetEnumerator()
    {
        foreach (var (k, v) in table)
        {
            if (ReferenceEquals(k, null)) continue;
            var vv = v?.obj;
            if (vv == null) continue;
            yield return new(k, (TValue)vv);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => table.Count();
    public bool ContainsKey(Type key) => table.TryGetValue(key, out _);
    public bool TryGetValue(Type key, out TValue value)
    {
        if (table.TryGetValue(key, out var v))
        {
            var vv = v?.obj;
            if (vv != null)
            {
                value = (TValue)vv;
                return true;
            }
        }


        value = default!;
        return false;
    }

    public TValue this[Type key] => TryGetValue(key, out var value)
        ? value
        : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

    public IEnumerable<Type> Keys => this.Select(o => o.Key);
    public IEnumerable<TValue> Values => this.Select(o => o.Value);
}
