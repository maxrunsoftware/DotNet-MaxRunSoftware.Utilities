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

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public sealed class EnumItem : IEquatable<EnumItem>, IComparable<EnumItem>, IComparable
{
    public TypeSlim TypeSlim { get; }
    public Type Type => TypeSlim.Type;
    public string Name { get; }
    public object Item { get; }
    public int Index { get; }
    private readonly int getHashCode;
    private readonly FieldInfo info;
    public IEnumerable<Attribute> Attributes => info.GetCustomAttributes();

    private EnumItem(TypeSlim typeSlim, string name, object item, int index, FieldInfo info)
    {
        TypeSlim = typeSlim;
        Name = name;
        Item = item;
        Index = index;
        this.info = info;
        getHashCode = GetHashCodeCreate();
    }




    #region Override

    private int GetHashCodeCreate() => Util.Hash(TypeSlim.GetHashCode(), Index, StringComparer.Ordinal.GetHashCode(Name));

    public override int GetHashCode() => getHashCode;

    public override bool Equals(object? obj) => Equals(obj as EnumItem);

    public bool Equals(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(this, other)) return true;

        if (getHashCode != other.getHashCode) return false;
        if (Index != other.Index) return false;
        if (!TypeSlim.Equals(other.TypeSlim)) return false;
        if (!StringComparer.Ordinal.Equals(Name, other.Name)) return false;

        return true;
    }

    public int CompareTo(object? obj) => CompareTo(obj as EnumItem);

    public int CompareTo(EnumItem? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = TypeSlim.CompareTo(other.TypeSlim))) return c;
        if (0 != (c = Index.CompareTo(other.Index))) return c;
        if (0 != (c = Name.CompareToOrdinalIgnoreCaseThenOrdinal(other.Name))) return c;
        if (0 != (c = getHashCode.CompareTo(other.getHashCode))) return c;
        return c;
    }

    public override string ToString() => Name;

    #endregion Override

    #region Static

    public static ImmutableArray<EnumItem> Get(Type enumType)
    {
        enumType.CheckIsEnum();
        var typeSlim = new TypeSlim(enumType);
        var names = Enum.GetNames(enumType).ToHashSet();

        // https://stackoverflow.com/questions/6819348/enum-getnames-results-in-unexpected-order-with-negative-enum-constants
        return enumType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(fieldInfo => names.Contains(fieldInfo.Name))
                .Select((fieldInfo, i) => new EnumItem(
                    typeSlim: typeSlim,
                    name: fieldInfo.Name,
                    item: Enum.Parse(enumType, fieldInfo.Name),
                    index: i,
                    info: fieldInfo
                ))
                .ToImmutableArray()
            ;
    }

    public static ImmutableArray<EnumItem> Get<T>() where T : struct, Enum => Get(typeof(T));

    public static EnumItem Get<T>(T enumItem) where T : struct, Enum
    {
        var name = Enum.GetName(enumItem);
        return Get<T>().First(item => string.Equals(item.Name, name, StringComparison.Ordinal));
    }

    public static EnumItem Get(object enumItem)
    {
        var enumType = enumItem.GetType();
        enumType.CheckIsEnum();
        var name = Enum.GetName(enumType, enumItem);
        return Get(enumType).First(item => string.Equals(item.Name, name, StringComparison.Ordinal));
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

    public static EnumItem Get<T>(string enumItemName) where T : struct, Enum => Get(typeof(T), enumItemName);

    #endregion Static
}

public static class EnumItemExtensions
{
    public static IEnumerable<T> GetAttributes<T>(this EnumItem item) where T : Attribute => item.Attributes.Select(o => o as T).WhereNotNull();

    public static T? GetAttribute<T>(this EnumItem item) where T : Attribute => item.GetAttributes<T>().FirstOrDefault();

    public static EnumItem ToEnumItem<T>(this T enumItem) where T : struct, Enum => EnumItem.Get(enumItem);
}
