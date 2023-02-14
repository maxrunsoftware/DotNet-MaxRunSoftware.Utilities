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

namespace MaxRunSoftware.Utilities.Common;

public class LogStateParser
{
    private static readonly IReadOnlyList<Tuple<string?, object?>> EMPTY = Array.Empty<Tuple<string?, object?>>();

    private readonly DictionaryWeakType<LogStateParserValueGetter> cache = new();

    public IReadOnlyList<Tuple<string?, object?>> Parse(object? state)
    {
        if (state == null) return EMPTY;
        var list = new List<Tuple<string?, object?>>();
        Parse(state, list);
        return list.Count == 0 ? EMPTY : list;
    }

    private void Parse(object stateObject, List<Tuple<string?, object?>> list)
    {
        if (stateObject is string str)
        {
            list.Add(new(str, null));
            return;
        }

        if (stateObject is IEnumerable enumerable)
        {
            var items = enumerable.Cast<object?>().WhereNotNull().ToArray();
            var cBefore = list.Count;
            foreach (var item in items) Parse(item, list);
            if (list.Count > cBefore) return;
        }

        var getter = cache.GetOrAdd(stateObject.GetType(), t => new(t));
        var v = getter.GetValue(stateObject);
        if (v != null) list.Add(v);
    }
}
