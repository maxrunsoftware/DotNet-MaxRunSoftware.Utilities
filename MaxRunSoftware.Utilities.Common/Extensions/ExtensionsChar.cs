// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsChar
{
    private static readonly ImmutableHashSet<char> IsBase16_Cache = "0123456789abcdefABCDEF".ToImmutableHashSet();
    public static bool IsBase16(this char c) => IsBase16_Cache.Contains(c);
    public static bool IsHex(this char c) => c.IsBase16();

    private static readonly ImmutableHashSet<char> IsNumber_Cache = "0123456789".ToImmutableHashSet();
    public static bool IsNumber(this char c) => IsNumber_Cache.Contains(c);

    public static string ToStringJoined(this IEnumerable<char> chars) => new(chars as char[] ?? chars.ToArray());
}
