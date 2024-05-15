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

using static System.DayOfWeek;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    // ReSharper disable StringLiteralTypo
    public static readonly FrozenDictionary<DayOfWeek, ImmutableArray<string>> DaysOfWeek_Strings = new Dictionary<DayOfWeek, ImmutableArray<string>>
    {
        [Sunday] = ["U", "SU", "SUN", "SUND", "SUNDA", "SUNDAY"],
        [Monday] = ["M", "MO", "MON", "MOND", "MONDA", "MONDAY"],
        [Tuesday] = ["T", "TU", "TUE", "TUES", "TUESD", "TUESDA", "TUESDAY"],
        [Wednesday] = ["W", "WE", "WED", "WEDN", "WEDNE", "WEDNES", "WEDNESD", "WEDNESDA", "WEDNESDAY"],
        [Thursday] = ["R", "TH", "THU", "THUR", "THURS", "THURSD", "THURSDA", "THURSDAY"],
        [Friday] = ["F", "FR", "FRI", "FRID", "FRIDA", "FRIDAY"],
        [Saturday] = ["S", "SA", "SAT", "SATU", "SATUR", "SATURD", "SATURDA", "SATURDAY"],
    }.ToFrozenDictionary();
    
    public static readonly ImmutableArray<string> DayOfWeek_Sunday_Strings = DaysOfWeek_Strings[Sunday];
    public static readonly ImmutableArray<string> DayOfWeek_Monday_Strings = DaysOfWeek_Strings[Monday];
    public static readonly ImmutableArray<string> DayOfWeek_Tuesday_Strings = DaysOfWeek_Strings[Tuesday];
    public static readonly ImmutableArray<string> DayOfWeek_Wednesday_Strings = DaysOfWeek_Strings[Wednesday];
    public static readonly ImmutableArray<string> DayOfWeek_Thursday_Strings = DaysOfWeek_Strings[Thursday];
    public static readonly ImmutableArray<string> DayOfWeek_Friday_Strings = DaysOfWeek_Strings[Friday];
    public static readonly ImmutableArray<string> DayOfWeek_Saturday_Strings = DaysOfWeek_Strings[Saturday];
    
    // ReSharper restore StringLiteralTypo

    public static readonly ImmutableArray<DayOfWeek> DaysOfWeek = [Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday];
    public static readonly FrozenDictionary<string, DayOfWeek> String_DayOfWeek = DaysOfWeek_Strings
        .SelectMany(kvp => kvp.Value.Select(s => (s, kvp.Key)))
        .ConstantToFrozenDictionaryTry(StringComparer.OrdinalIgnoreCase);

    
}
