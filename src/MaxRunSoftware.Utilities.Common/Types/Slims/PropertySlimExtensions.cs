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

public static class PropertySlimExtensions
{
    public static PropertyInfo ToPropertyInfo(this PropertySlim obj) => obj;
    public static PropertySlim ToPropertySlim(this PropertyInfo obj) => obj;

    public static ImmutableArray<PropertySlim> GetPropertySlims(this TypeSlim type, BindingFlags flags) =>
        type.Type.GetPropertySlims(flags);

    public static ImmutableArray<PropertySlim> GetPropertySlims(this Type type, BindingFlags flags) =>
        type.GetProperties(flags).Select(o => new PropertySlim(o)).ToImmutableArray();

    public static PropertySlim? GetPropertySlim(this TypeSlim type, string name, BindingFlags? flags = null) =>
        type.Type.GetPropertySlim(name, flags);

    public static PropertySlim? GetPropertySlim(this Type type, string name, BindingFlags? flags = null)
    {
        var flagsList = flags != null
            ? new[] { flags.Value }
            : new[]
            {
                BindingFlags.Public | BindingFlags.Instance,
                BindingFlags.Public | BindingFlags.Static,
                BindingFlags.NonPublic | BindingFlags.Instance,
                BindingFlags.NonPublic | BindingFlags.Static,
            };

        foreach (var f in flagsList)
        {
            var items = GetPropertySlims(type, f);
            foreach (var sc in Constant.StringComparers)
            {
                var matches = items.Where(prop => sc.Equals(prop.Name, name)).ToList();
                if (matches.Count == 1) return matches[0];
                if (matches.Count > 1) throw new AmbiguousMatchException($"Found {matches.Count} properties on {type.FullNameFormatted()} with name {name}: " + matches.Select(o => o.Name).ToStringDelimited(", "));
            }
        }

        return null;
    }
}
