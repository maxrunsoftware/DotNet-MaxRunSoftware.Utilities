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

// ReSharper disable StringLiteralTypo

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    /// <summary>
    /// Characters A-Z
    /// </summary>
    public const string Chars_A_Z_Upper_String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>
    /// Characters A-Z
    /// </summary>
    public static readonly ImmutableArray<char> Chars_A_Z_Upper = ImmutableArray.Create(Chars_A_Z_Upper_String.ToCharArray());

    /// <summary>
    /// Characters a-z
    /// </summary>
    public const string Chars_A_Z_Lower_String = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// Characters a-z
    /// </summary>
    public static readonly ImmutableArray<char> Chars_A_Z_Lower = ImmutableArray.Create(Chars_A_Z_Lower_String.ToCharArray());

    /// <summary>
    /// Numbers 0-9
    /// </summary>
    public const string Chars_0_9_String = "0123456789";

    /// <summary>
    /// Numbers 0-9
    /// </summary>
    public static readonly ImmutableArray<char> Chars_0_9 = ImmutableArray.Create(Chars_0_9_String.ToCharArray());

    /// <summary>
    /// A-Z a-z 0-9
    /// </summary>
    public const string Chars_Alphanumeric_String = Chars_A_Z_Upper_String + Chars_A_Z_Lower_String + Chars_0_9_String;

    /// <summary>
    /// A-Z a-z 0-9
    /// </summary>
    public static readonly ImmutableArray<char> Chars_Alphanumeric = ImmutableArray.Create(Chars_Alphanumeric_String.ToCharArray());

    /// <summary>
    /// Printable characters (33 - 126)
    /// </summary>
    public const string Chars_Printable_String = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

    /// <summary>
    /// Printable characters (33 - 126)
    /// </summary>
    public static readonly ImmutableArray<char> Chars_Printable = ImmutableArray.Create(Chars_Printable_String.ToCharArray());

    /// <summary>
    /// Printable characters including space character (32 - 126)
    /// </summary>
    public const string Chars_Printable_Space_String = " " + Chars_Printable_String;

    /// <summary>
    /// Printable characters including space character (32 - 126)
    /// </summary>
    public static readonly ImmutableArray<char> Chars_Printable_Space = ImmutableArray.Create(Chars_Printable_Space_String.ToCharArray());
}
