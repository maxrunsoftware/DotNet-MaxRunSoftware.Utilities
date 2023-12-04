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

public class HttpHeaderCollection : IEnumerable<string>
{
    protected readonly IDictionary<string, IList<string>> d = new DictionaryIndexed<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

    public virtual HttpHeaderCollection Add(string header, params string[] values)
    {
        if (!d.TryGetValue(header, out var list))
        {
            list = new List<string>();
            d.Add(header, list);
        }

        foreach (var value in values)
        {
            list.Add(value);
        }

        return this;
    }


    public virtual HttpHeaderCollection Remove(params string[] headers)
    {
        foreach (var header in headers)
        {
            d.Remove(header);
        }

        return this;
    }

    public virtual HttpHeaderCollection Remove(string header, string value)
    {
        if (d.TryGetValue(header, out var list))
        {
            while (list.Remove(value)) { }

            if (list.IsEmpty()) d.Remove(header);
        }

        return this;
    }

    public virtual bool Contains(string header) => d.ContainsKey(header);

    public virtual IEnumerable<KeyValuePair<string, string>> Headers
    {
        get
        {
            foreach (var kvp in d.Where(kvp => !kvp.Value.IsEmpty()))
            {
                if (kvp.Key.EqualsOrdinalIgnoreCase("Set-Cookie"))
                {
                    foreach (var value in kvp.Value)
                    {
                        yield return KeyValuePair.Create(kvp.Key, value);
                    }
                }
                else
                {
                    yield return KeyValuePair.Create(kvp.Key, kvp.Value.ToStringDelimited(","));
                }
            }
        }
    }

    public virtual IEnumerable<string> this[string header] => d.TryGetValue(header, out var list) ? list : Enumerable.Empty<string>();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public virtual IEnumerator<string> GetEnumerator() => d.Keys.GetEnumerator();
}
