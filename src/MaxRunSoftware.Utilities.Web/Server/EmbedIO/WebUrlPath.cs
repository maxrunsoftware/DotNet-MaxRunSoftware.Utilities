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

namespace MaxRunSoftware.Utilities.Web.Server.EmbedIO;

public class WebUrlPath : ComparerBaseClass<WebUrlPath>, IReadOnlyList<string>
{
    public static bool Default_Path_IsCaseSensitive { get; set; } = false;

    private static StringComparer GetStringComparer(bool isCaseSensitive) => isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    private readonly ImmutableArray<string> pathParts;

    public bool IsRoot { get; }
    public string PathRaw { get; }

    private readonly string toString;

    public WebUrlPath(string path)
    {
        PathRaw = path;

        pathParts = path
            .Split('/')
            .Select(o => o.TrimOrNull())
            .WhereNotNull()
            .ToImmutableArray();

        IsRoot = pathParts.Length == 0;

        toString = "/" + pathParts.ToStringDelimited("/");
    }

    public WebUrlPath(IEnumerable<string> pathParts) : this("/" + pathParts.ToStringDelimited('/')) { }

    public bool StartsWith(params string[] prefixParts) => StartsWith(new(prefixParts), Default_Path_IsCaseSensitive);

    public bool StartsWith(bool isCaseSensitive, params string[] prefixParts) => StartsWith(new(prefixParts), isCaseSensitive);

    public bool StartsWith(IEnumerable<string> prefixParts, bool isCaseSensitive = false) => StartsWith(new(prefixParts), isCaseSensitive);

    public bool StartsWith(WebUrlPath prefix, bool isCaseSensitive = false)
    {
        if (prefix.Count > Count) return false;
        var sc = GetStringComparer(isCaseSensitive);
        for (var i = 0; i < prefix.Count; i++)
        {
            if (!sc.Equals(prefix[i], this[i]))
            {
                return false;
            }
        }

        return true;
    }

    #region IReadOnlyList<string>

    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)pathParts).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => pathParts.Length;
    public string this[int index] => pathParts[index];

    #endregion IReadOnlyList<string>

    #region Overrides

    public override string ToString() => toString;

    public static bool Equals(WebUrlPath? x, WebUrlPath? y, bool isCaseSensitive) => Compare(x, y, isCaseSensitive) == 0;
    public static int GetHashCode(WebUrlPath obj, bool isCaseSensitive) => isCaseSensitive ? HashOrdinal(obj) : HashOrdinalIgnoreCase(obj);
    public static int Compare(WebUrlPath? x, WebUrlPath? y, bool isCaseSensitive) => CompareClassEnumerable(GetStringComparer(isCaseSensitive), x, y) ?? 0;

    public virtual bool Equals(WebUrlPath? y, bool isCaseSensitive) => Equals(this, y, isCaseSensitive);
    public virtual int GetHashCode(bool isCaseSensitive) => GetHashCode(this, isCaseSensitive);
    public virtual int Compare(WebUrlPath? y, bool isCaseSensitive) => Compare(this, y, isCaseSensitive);

    protected override bool EqualsInternal(WebUrlPath x, WebUrlPath y) => Equals(x, y, Default_Path_IsCaseSensitive);
    protected override int GetHashCodeInternal(WebUrlPath obj) => GetHashCode(obj, Default_Path_IsCaseSensitive);
    protected override int CompareInternal(WebUrlPath x, WebUrlPath y) => Compare(x, y, Default_Path_IsCaseSensitive);

    #endregion Overrides
}
