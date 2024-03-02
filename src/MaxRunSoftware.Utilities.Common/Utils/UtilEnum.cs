﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

public static partial class Util
{
    public static IReadOnlyList<TEnum> GetEnumValues<TEnum>() where TEnum : struct, Enum => (TEnum[])Enum.GetValues(typeof(TEnum));

    public static TEnum CombineEnumFlags<TEnum>(IEnumerable<TEnum> enums) where TEnum : struct, Enum => (TEnum)Enum.Parse(typeof(TEnum), string.Join(", ", enums.Select(o => o.ToString())));
}
