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

public static class FieldSlimExtensions
{
    public static FieldInfo ToFieldInfo(this FieldSlim obj) => obj;
    public static FieldSlim ToPropertySlim(this FieldInfo obj) => obj;

    public static ImmutableArray<FieldSlim> GetFieldSlims(this TypeSlim type, BindingFlags flags) =>
        type.Type.GetFieldSlims(flags);

    public static ImmutableArray<FieldSlim> GetFieldSlims(this Type type, BindingFlags flags) =>
        type.GetFields(flags).Select(o => new FieldSlim(o)).ToImmutableArray();

    public static FieldSlim? GetFieldSlim(this TypeSlim type, string name, BindingFlags? flags = null) =>
        type.Type.GetFieldSlim(name, flags);

    public static FieldSlim? GetFieldSlim(this Type type, string name, BindingFlags? flags = null)
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
            var items = GetFieldSlims(type, f);
            foreach (var sc in Constant.StringComparers)
            {
                var matches = items.Where(prop => sc.Equals(prop.Name, name)).ToList();
                if (matches.Count == 1) return matches[0];
                if (matches.Count > 1) throw new AmbiguousMatchException($"Found {matches.Count} fields on {type.FullNameFormatted()} with name {name}: " + matches.Select(o => o.Name).ToStringDelimited(", "));
            }
        }

        return null;
    }
}
