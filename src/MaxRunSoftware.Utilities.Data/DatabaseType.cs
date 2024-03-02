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

namespace MaxRunSoftware.Utilities.Data;

[AttributeUsage(AttributeTargets.Enum)]
public class DatabaseTypesAttribute : Attribute
{
    public DatabaseAppType DatabaseAppType { get; }

    public DatabaseTypesAttribute(DatabaseAppType databaseAppType) => DatabaseAppType = databaseAppType;

    public Type? ExternalEnum { get; set; }
}

public class DatabaseTypes
{
    public DatabaseAppType DatabaseAppType { get; }
    public TypeSlim EnumType { get; }
    public ImmutableArray<DatabaseType> Types { get; }
    public ImmutableDictionary<EnumItem, DatabaseType> EnumItems { get; }
    public ImmutableDictionary<object, DatabaseType> EnumItemObjects { get; }
    public ImmutableDictionary<string, DatabaseType> TypeNames { get; }

    public DatabaseTypes(DatabaseAppType databaseAppType, TypeSlim enumType, IEnumerable<DatabaseType> types)
    {
        DatabaseAppType = databaseAppType;
        EnumType = enumType;
        Types = types.ToImmutableArray();

        var enumItems = new Dictionary<EnumItem, DatabaseType>();
        var enumItemObjects = new Dictionary<object, DatabaseType>();
        var typeNames = new Dictionary<string, DatabaseType>(StringComparer.OrdinalIgnoreCase);
        foreach (var type in Types)
        {
            enumItems.Add(type.EnumItem, type);
            enumItemObjects.Add(type.EnumItem.Item, type);
            foreach (var typeName in type.TypeNames)
            {
                typeNames.Add(typeName, type);
            }
        }

        EnumItems = enumItems.ToImmutableDictionary();
        EnumItemObjects = enumItemObjects.ToImmutableDictionary();
        TypeNames = typeNames.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    // ReSharper disable InconsistentNaming
    private static readonly object locker = new();
    private static readonly Dictionary<Type, DatabaseTypes> cacheType = new();
    private static readonly Dictionary<DatabaseAppType, DatabaseTypes> cacheDatabaseAppType = new();
    // ReSharper restore InconsistentNaming

    private static DatabaseTypes GetCreateCache(DatabaseAppType databaseAppType, Type enumType)
    {
        lock (locker)
        {
            var types = DatabaseType.Get(databaseAppType, enumType);

            cacheType[enumType] = types;
            cacheDatabaseAppType[databaseAppType] = types;

            return types;
        }
    }

    public static DatabaseTypes Get<T>() where T : struct, Enum => Get(typeof(T));
    public static DatabaseTypes Get(Type enumType)
    {
        lock (locker)
        {
            if (cacheType.TryGetValue(enumType, out var items)) return items;

            var databaseTypesAttribute = enumType.GetCustomAttributes<DatabaseTypesAttribute>().FirstOrDefault();
            if (databaseTypesAttribute == null) throw new ArgumentException($"{enumType.FullNameFormatted()} does not define a {typeof(DatabaseTypesAttribute).FullNameFormatted()}", nameof(enumType));

            return GetCreateCache(databaseTypesAttribute.DatabaseAppType, enumType);
        }
    }

    public static DatabaseTypes Get(DatabaseAppType databaseAppType)
    {
        lock (locker)
        {
            if (cacheDatabaseAppType.TryGetValue(databaseAppType, out var items)) return items;

            var types = new Dictionary<DatabaseAppType, List<Type>>();
            foreach (var type in typeof(DatabaseType).Assembly.GetTypes().Where(o => o.IsEnum))
            {
                var attribute = type.GetCustomAttributes<DatabaseTypesAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                types.AddToList(attribute.DatabaseAppType, type);
            }

            // Make sure only one of DatabaseAppType exists
            foreach (var kvp in types)
            {
                if (kvp.Value.Count > 1)
                {
                    var itemsString = kvp.Value.Select(o => new TypeSlim(o)).OrderBy(o => o).Select(o => o.Type.FullNameFormatted()).ToStringDelimited(", ");
                    throw new ArgumentException($"Multiple {nameof(DatabaseAppType)}.{kvp.Key} defined in assembly " + itemsString, nameof(databaseAppType));
                }
            }

            if (!types.TryGetValue(databaseAppType, out var list))
            {
                throw new ArgumentException($"No {nameof(DatabaseAppType)}.{databaseAppType} defined in assembly", nameof(databaseAppType));
            }

            return GetCreateCache(databaseAppType, list[0]);
        }
    }

    public static DatabaseType Get<T>(T enumItem) where T : struct, Enum => Get<T>().EnumItemObjects[enumItem];
}

[AttributeUsage(AttributeTargets.Field)]
public class DatabaseTypeAttribute : Attribute
{
    public DbType DbType { get; }
    public Type? DotNetType { get; set; }

    public string[]? DatabaseTypeNames { get; set; }
    public object? AliasFor { get; set; }

    public DatabaseTypeAttribute(DbType dbType) => DbType = dbType;
}

public class DatabaseType
{
    public DatabaseAppType DatabaseAppType { get; private init; }
    public EnumItem EnumItem { get; private init; } = null!;
    public DbType DbType { get; private init; }
    public Type DotNetType { get; private init; } = null!;
    public ImmutableArray<string> TypeNames { get; private init; }

    public DatabaseType? AliasFor { get; private set; }
    public string TypeName { get; private set; } = null!;

    public static DatabaseTypes Get(DatabaseAppType databaseAppType, Type enumType)
    {
        var enumItems = EnumItem.Get(enumType).Select(o => (Item: o, Attr: o.GetAttribute<DatabaseTypeAttribute>())).ToArray();

        var checkName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var checkDatabaseTypeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // create SqlType objects
        var sqlTypes = new List<(DatabaseType DatabaseType, DatabaseTypeAttribute Attr)>();
        foreach (var enumItem in enumItems)
        {
            // check to be sure every enum item has a SqlTypeAttribute
            var attr = enumItem.Attr ?? throw new ArgumentException($"{enumItem.Item.Type.FullNameFormatted()}.{enumItem.Item.Name} does not define a {typeof(DatabaseTypeAttribute).FullNameFormatted()}", nameof(enumType));

            var databaseTypeNames = attr.DatabaseTypeNames.OrEmpty().TrimOrNull().WhereNotNull().Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (databaseTypeNames.Count == 0) databaseTypeNames.Add(enumItem.Item.Name);
            var st = new DatabaseType
            {
                DatabaseAppType = databaseAppType,
                EnumItem = enumItem.Item,
                DbType = attr.DbType,
                DotNetType = attr.DotNetType ?? attr.DbType.GetDotNetType(),
                TypeNames = databaseTypeNames.ToImmutableArray(),
            };

            // duplicate EnumItem check
            if (!checkName.Add(st.EnumItem.Name)) throw new ArgumentException(enumType.FullNameFormatted() + " defines multiple items with the same name but different case", nameof(enumType));

            // duplicate SqlTypeNames check
            foreach (var sqlTypeName in st.TypeNames)
            {
                if (!checkDatabaseTypeNames.Add(sqlTypeName)) throw new ArgumentException(enumType.FullNameFormatted() + $" defines multiple names of '{sqlTypeName}'", nameof(enumType));
            }

            sqlTypes.Add((st, attr));
        }

        // update AliasFor references
        var d = sqlTypes.ToDictionary(o => o.DatabaseType.EnumItem, o => o);
        foreach (var item in sqlTypes.Where(o => o.Attr.AliasFor != null))
        {
            var name = item.DatabaseType.EnumItem.Type.FullNameFormatted() + "." + item.DatabaseType.EnumItem.Name;
            var aliasForEnumItem = EnumItem.Get(item.Attr.AliasFor!);
            var nameAlias = aliasForEnumItem.Type.FullNameFormatted() + "." + aliasForEnumItem.Name;
            if (!item.DatabaseType.EnumItem.TypeSlim.Equals(aliasForEnumItem.TypeSlim)) throw new ArgumentException($"{name} {nameof(AliasFor)} references item in different enum {nameAlias}", nameof(enumType));
            if (item.DatabaseType.EnumItem.Equals(aliasForEnumItem)) throw new ArgumentException($"{name} {nameof(AliasFor)} cannot reference itself", nameof(enumType));
            if (!d.ContainsKey(aliasForEnumItem)) throw new ArgumentException($"{name} {nameof(AliasFor)} references unknown enum item {nameAlias}", nameof(enumType));
            item.DatabaseType.AliasFor = d[aliasForEnumItem].DatabaseType;
        }

        // update SqlTypeName for objects without AliasOf
        foreach (var item in sqlTypes.Where(o => o.DatabaseType.AliasFor == null)) item.DatabaseType.TypeName = item.DatabaseType.TypeNames.First();

        // update SqlTypeName for object with AliasOf
        foreach (var item in sqlTypes.Select(o => o.DatabaseType).Where(o => o.AliasFor != null))
        {
            // check for circular references
            var alreadyCheckedItemNames = new HashSet<EnumItem>();
            var current = item;
            while (current.AliasFor != null)
            {
                if (!alreadyCheckedItemNames.Add(current.EnumItem)) throw new InvalidOperationException($"Circular [{nameof(AliasFor)}] reference detected in {item.EnumItem.Type.FullNameFormatted()} with items " + alreadyCheckedItemNames.OrderBy(o => o).Select(o => o.ToString()).ToStringDelimited(", "));
                current = current.AliasFor;
            }

            item.TypeName = current.TypeName;
        }

        // check every SqlType.SqlTypeName is not null
        foreach (var sqlType in sqlTypes.Select(o => o.DatabaseType))
        {
            if (sqlType.TypeName == null)
            {
                throw new ArgumentException($"Internal error {sqlType.EnumItem.Type.FullNameFormatted() + "." + sqlType.EnumItem.Name} does not contain a {nameof(TypeName)}", nameof(enumType));
            }
        }

        return new(databaseAppType, enumType, sqlTypes.Select(o => o.DatabaseType));
    }
}
