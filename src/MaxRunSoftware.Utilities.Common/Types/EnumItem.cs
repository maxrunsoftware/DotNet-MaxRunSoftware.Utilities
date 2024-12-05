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

[PublicAPI]
public sealed class EnumItem : IEquatable<EnumItem>, IComparable<EnumItem>, IComparable
{
    public Type Type { get; }
    public string Name { get; }
    public object Item { get; }
    public int Index { get; }
    private readonly int getHashCode;
    private readonly FieldInfo info;

    public IEnumerable<Attribute> Attributes => info.GetCustomAttributes();

    private EnumItem(Type type, string name, object item, int index, FieldInfo info)
    {
        Type = type;
        Name = name;
        Item = item;
        Index = index;
        this.info = info;
        getHashCode = Util.Hash(type.GetHashCode(), Index, StringComparer.Ordinal.GetHashCode(Name));
    }

    #region Override

    public override string ToString() => Name;

    public override int GetHashCode() => getHashCode;

    public override bool Equals(object? obj) => Equals(obj as EnumItem);

    public bool Equals(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (Index != other.Index) return false;
        if (Type != other.Type) return false;
        if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;

        return true;
    }

    public int CompareTo(object? obj) => CompareTo(obj as EnumItem);

    public int CompareTo(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = (Type.FullName ?? Type.Name).CompareToOrdinal(other.Type.FullName ?? other.Type.Name))) return c;
        if (0 != (c = Index.CompareTo(other.Index))) return c;
        if (0 != (c = Name.CompareToOrdinalIgnoreCaseThenOrdinal(other.Name))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    #endregion Override

    #region Static

    public static ImmutableArray<EnumItem> Get(Type enumType)
    {
        enumType.CheckIsEnum();
        var names = Enum.GetNames(enumType).ToHashSet();

        // https://stackoverflow.com/questions/6819348/enum-getnames-results-in-unexpected-order-with-negative-enum-constants
        return enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fieldInfo => names.Contains(fieldInfo.Name))
                .Select((fieldInfo, i) => new EnumItem(
                    enumType,
                    fieldInfo.Name,
                    Enum.Parse(enumType, fieldInfo.Name),
                    i,
                    fieldInfo
                ))
                .ToImmutableArray()
            ;
    }

    public static ImmutableArray<EnumItem> Get<T>() where T : struct, Enum => typeof(T).GetEnumItems();

    public static EnumItem Get<T>(T enumItemObj) where T : struct, Enum
    {
        var name = Enum.GetName(enumItemObj);
        return Get<T>().First(item => string.Equals(item.Name, name, StringComparison.Ordinal));
    }

    public static EnumItem Get(object enumItemObj)
    {
        var enumType = enumItemObj.GetType();
        enumType.CheckIsEnum();
        var name = Enum.GetName(enumType, enumItemObj);
        return enumType.GetEnumItems().First(item => string.Equals(item.Name, name, StringComparison.Ordinal));
    }

    public static EnumItem Get(Type enumType, string enumItemName)
    {
        var enumItems = Get(enumType);
        var d = new Dictionary<string, List<EnumItem>>(StringComparer.OrdinalIgnoreCase);
        foreach (var enumItem in enumItems) d.AddToList(enumItem.Name, enumItem);
        if (!d.TryGetValue(enumItemName, out var list)) throw new ArgumentException($"{enumType.FullNameFormatted()} does not contain item '{enumItemName}'", nameof(enumItemName));
        if (list.Count == 1) return list[0];
        var item = list.FirstOrDefault(listItem => string.Equals(listItem.Name, enumItemName, StringComparison.Ordinal));
        if (item != null) return item;
        throw new ArgumentException($"{enumType.FullNameFormatted()} contains multiple possible '{enumItemName}' values but none matching exact case: " + list.Select(o => o.Name).ToStringDelimited(", "), nameof(enumItemName));
    }

    #endregion Static
}
