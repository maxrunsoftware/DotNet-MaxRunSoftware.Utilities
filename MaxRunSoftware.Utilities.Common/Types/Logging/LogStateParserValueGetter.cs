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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MaxRunSoftware.Utilities.Common;

public sealed class LogStateParserValueGetter
{
    private readonly Func<object, Tuple<string?, object?>?> getter;
    public LogStateParserValueGetter(Type stateType) => getter = Create(stateType);

    public Tuple<string?, object?>? GetValue(object instance) => getter.Invoke(instance);

    private static Func<object, Tuple<string?, object?>?> Create(Type stateType)
    {
        if (stateType == typeof(string)) return o => new((string)o, null);

        if (stateType.IsAssignableTo(typeof(KeyValuePair<string?, object?>)))
            return o =>
            {
                var kvp = (KeyValuePair<string?, object?>)o;
                return new(kvp.Key, kvp.Value);
            };

        if (stateType.IsAssignableTo(typeof(Tuple<string?, object?>)))
            return o =>
            {
                var kvp = (Tuple<string?, object?>)o;
                return new(kvp.Item1, kvp.Item2);
            };

        var stateTypeSlim = stateType.ToTypeSlim();
        var gettersKey = GetValueGetters(stateTypeSlim, "Name", "Key", "Item1");
        var gettersVal = GetValueGetters(stateTypeSlim, "Value", "Item2");

        if (gettersKey.Count > 0 && gettersVal.Count > 0)
            return o =>
            {
                var k = gettersKey.Select(g => g(o)).Select(oo => oo.ToStringGuessFormat()).WhereNotNull().FirstOrDefault();
                var v = gettersVal.Select(g => g(o)).WhereNotNull().FirstOrDefault();
                return new(k, v);
            };

        if (gettersKey.Count > 0)
            return o =>
            {
                var k = gettersKey.Select(g => g(o)).Select(oo => oo.ToStringGuessFormat()).WhereNotNull().FirstOrDefault();
                var v = Serialize(o).TrimOrNull() ?? o.ToStringGuessFormat();
                return new(k, v);
            };

        if (gettersVal.Count > 0)
            return o =>
            {
                var k = o.GetType().NameFormatted();
                var v = gettersVal.Select(g => g(o)).WhereNotNull().FirstOrDefault();
                return new(k, v);
            };

        return o =>
        {
            var k = o.GetType().NameFormatted();
            var v = Serialize(o).TrimOrNull() ?? o.ToStringGuessFormat();
            return new(k, v);
        };
    }

    private static List<Func<object, object?>> GetValueGetters(TypeSlim stateTypeSlim, params string[] names)
    {
        var namesSet = names.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var flags = BindingFlags.Public | BindingFlags.Instance;

        var props = stateTypeSlim.GetPropertySlims(flags)
            .Where(o => !o.IsStatic && o.IsGettablePublic && namesSet.Contains(o.Name))
            .Select(o => (ISlimValueGetter)o).ToArray();

        var fields = stateTypeSlim.GetFieldSlims(flags)
            .Where(o => !o.IsStatic && namesSet.Contains(o.Name))
            .Select(o => (ISlimValueGetter)o).ToArray();

        return props.Concat(fields).Select(getter => (Func<object, object?>)getter.GetValue).ToList();
    }

    private static string? Serialize(object? o)
    {
        var settings = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            // DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        };
        try
        {
            return JsonSerializer.Serialize(o, settings);
        }
        catch (JsonException) { }

        return null;
    }
}
