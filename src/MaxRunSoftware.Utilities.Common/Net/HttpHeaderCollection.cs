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
    public static ISet<string> ALLOWED_DUPLICATE_HEADERS { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Set-Cookie",
    };

    protected readonly IDictionary<string, IList<string>> d = new DictionaryIndexed<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

    #region Properties

    protected virtual string? GetPropertyFirstTrimmed(string name) => this[name].TrimOrNull().WhereNotNull().FirstOrDefault();

    public virtual string? Authorization => GetPropertyFirstTrimmed(nameof(Authorization));

    public virtual string? AuthorizationType => (Authorization ?? string.Empty).SplitOnWhiteSpace().TrimOrNull().WhereNotNull().FirstOrDefault();

    public virtual (string? username, string? password)? AuthorizationBasic
    {
        get
        {
            if (!AuthorizationType.EqualsOrdinalIgnoreCase("basic")) return null;
            var auth = Authorization;
            if (auth == null) return null;
            var authParts = auth.SplitOnWhiteSpace(2).Select(o => o.TrimOrNull()).WhereNotNull().ToArray();
            var authType = authParts.GetAtIndexOrDefault(0);
            if (authType == null) return null;
            if (!authType.EqualsOrdinalIgnoreCase("basic")) return null;

            var authValue = authParts.GetAtIndexOrDefault(1);
            if (authValue == null) return (null, null); // Authorization: BASIC is defined but nothing specified after BASIC


            var userpassEncoded = Convert.FromBase64String(authValue);
            var userpass = Constant.Encoding_UTF8_Without_BOM.GetString(userpassEncoded).TrimOrNull();
            if (userpass == null) return (null, null);

            var userpassParts = userpass.Split(':', 2);
            var username = userpassParts.GetAtIndexOrDefault(0).TrimOrNull();
            var password = userpassParts.GetAtIndexOrDefault(1).TrimOrNull();

            return (username, password);
        }
    }

    #endregion Properties

    #region Collection Functions

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
                if (ALLOWED_DUPLICATE_HEADERS.Contains(kvp.Key))
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

    #endregion Collection Functions
}
