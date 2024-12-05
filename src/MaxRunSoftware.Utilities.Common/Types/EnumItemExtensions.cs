// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

public static class EnumItemExtensions
{
    public static IEnumerable<T> GetAttributes<T>(this EnumItem item) where T : Attribute => item.Attributes.Select(o => o as T).WhereNotNull();

    public static T? GetAttribute<T>(this EnumItem item) where T : Attribute => item.GetAttributes<T>().FirstOrDefault();

    public static ImmutableArray<EnumItem> GetEnumItems(this Type enumType) => EnumItem.Get(enumType);

    public static EnumItem GetEnumItem(this Type enumType, string enumItemName) => EnumItem.Get(enumType, enumItemName);

    public static EnumItem ToEnumItem<T>(this T enumItem) where T : struct, Enum => EnumItem.Get(enumItem);

    public static EnumItem ToEnumItem<T>(this string enumItemName) where T : struct, Enum => GetEnumItem(typeof(T), enumItemName);
}
