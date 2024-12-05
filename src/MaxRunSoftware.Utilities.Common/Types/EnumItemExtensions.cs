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
