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

namespace MaxRunSoftware.Utilities.Database;

[AttributeUsage(AttributeTargets.Property)]
public class DatabaseServerPropertyAttribute : Attribute
{
    public DatabaseServerPropertyAttribute(string? group, string? name, object? databaseType) : this(group, name, databaseType, null) { }

    public DatabaseServerPropertyAttribute(string? name, object? databaseType) : this(null, name, databaseType, null) { }

    public DatabaseServerPropertyAttribute(string? name, object? databaseType, int databaseTypeLength) : this(null, name, databaseType, databaseTypeLength) { }

    public DatabaseServerPropertyAttribute(object? databaseType) : this(null, null, databaseType, null) { }

    public DatabaseServerPropertyAttribute(object? databaseType, int databaseTypeLength) : this(null, null, databaseType, databaseTypeLength) { }

    public DatabaseServerPropertyAttribute(string? group, string? name, object? databaseType, int databaseTypeLength) : this(group, name, databaseType, (int?)databaseTypeLength) { }

    protected DatabaseServerPropertyAttribute(string? group, string? name, object? databaseType, int? databaseTypeLength)
    {
        Group = group;
        Name = name;
        DatabaseType = databaseType;
        DatabaseTypeLength = databaseTypeLength;
    }

    public string? Group { get; }
    public string? Name { get; }
    public object? DatabaseType { get; }
    public int? DatabaseTypeLength { get; }
}

public abstract class DatabaseServerProperties
{
    protected DatabaseServerProperties()
    {
        getProperties = GetPropertiesPrivate(GetType());
    }

    private static PropertyCollection GetPropertiesPrivate(TypeSlim type)
    {
        if (cache.TryGetValue(type, out var properties)) return properties;
        lock (locker)
        {
            if (cache.TryGetValue(type, out properties)) return properties;
            properties = new(type);
            cache = cache.Add(type, properties);
            return properties;
        }
    }
    private readonly PropertyCollection getProperties;
    protected PropertyCollection GetProperties() => getProperties;

    // ReSharper disable once InconsistentNaming
    private static readonly object locker = new();

    private static ImmutableDictionary<TypeSlim, PropertyCollection> cache = ImmutableDictionary<TypeSlim, PropertyCollection>.Empty;

    protected class PropertyItem : IComparable<PropertyItem>
    {
        public static readonly string GROUP_NULL_DICTIONARY_KEY = "NULL_" + Guid.NewGuid();
        public PropertyItem(PropertySlim property, DatabaseServerPropertyAttribute? attribute)
        {
            Property = property;
            Attribute = attribute;
        }
        public PropertySlim Property { get; }
        public DatabaseServerPropertyAttribute? Attribute { get; }

        public string Name => Attribute?.Name ?? Property.Name;
        public string? Group => Attribute?.Group;
        public string GroupDictionaryId => Group ?? GROUP_NULL_DICTIONARY_KEY;
        public object? DatabaseType => Attribute?.DatabaseType;
        public int? DatabaseTypeLength => Attribute?.DatabaseTypeLength;
        public bool IsCustom => Attribute == null;
        public int CompareTo(PropertyItem? other)
        {
            if (ReferenceEquals(other, null)) return 1;
            if (ReferenceEquals(this, other)) return 0;
            int c;
            if (0 != (c = IsCustom.CompareTo(other.IsCustom))) return c;
            if (0 != (c = StringComparer.OrdinalIgnoreCase.Compare(Group, other.Group))) return c;
            if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
            return c;
        }
    }

    protected class PropertyCollection
    {
        public TypeSlim Type { get; }
        public ImmutableArray<PropertyItem> PropertyItems { get; }
        private readonly ImmutableDictionary<string, ImmutableDictionary<string, PropertyItem>> propertyItemsGroupName;
        public ImmutableDictionary<string, PropertyItem> GetPropertyItems(string? groupName) =>
            propertyItemsGroupName.TryGetValue(groupName ?? PropertyItem.GROUP_NULL_DICTIONARY_KEY, out var d) ? d : ImmutableDictionary<string, PropertyItem>.Empty;

        public PropertyCollection(TypeSlim type)
        {
            Type = type;
            PropertyItems = type.Type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(o => o.CanRead)
                .Select(o => o.ToPropertySlim())
                .Select(o => new PropertyItem(o, o.Info.GetCustomAttribute<DatabaseServerPropertyAttribute>()))
                .OrderBy(o => o)
                .ToImmutableArray();


            var props = new Dictionary<string, Dictionary<string, PropertyItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var propertyItem in PropertyItems)
            {
                if (propertyItem is { IsCustom: false, Property.CanSetPublic: false }) throw new ArgumentException($"Property {type.NameFull}.{propertyItem.Property.Name} has no visible SET method");

                if (!props.TryGetValue(propertyItem.GroupDictionaryId, out var propsSub))
                {
                    propsSub = new Dictionary<string, PropertyItem>(StringComparer.OrdinalIgnoreCase);
                    props.Add(propertyItem.GroupDictionaryId, propsSub);
                }

                propsSub.Add(propertyItem.Name, propertyItem);
            }

            // Convert to immutable
            var propsImmutable = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, PropertyItem>>(StringComparer.OrdinalIgnoreCase);
            foreach (var groupDict in props)
            {
                var propsSubImmutable = ImmutableDictionary.CreateBuilder<string, PropertyItem>(StringComparer.OrdinalIgnoreCase);
                foreach (var nameProp in groupDict.Value)
                {
                    propsSubImmutable.Add(nameProp.Key, nameProp.Value);
                }

                propsImmutable.Add(groupDict.Key, propsSubImmutable.ToImmutable());
            }

            propertyItemsGroupName = propsImmutable.ToImmutable();
        }


    }


    public virtual void Load(IReadOnlyDictionary<string, string?> result, string? group)
    {
        var log = Constant.GetLogger(GetType());
        var props = GetProperties().GetPropertyItems(group);
        var usedKeys = new HashSet<string>();

        foreach (var (key, val) in result)
        {
            if (!props.TryGetValue(key, out var prop)) continue;
            if (prop.IsCustom) throw new InvalidCastException($"Unable to set property {prop.Property.NameClass} with {key}={val ?? "null"} because property does not define a [{nameof(DatabaseServerPropertyAttribute)}]");
            log.LogTrace("For item {ResultName}={ResultValue} found property {PropertyName}", key, val, $"{prop.Property.NameClass}");
            object? valConverted;
            try
            {
                valConverted = Util.ChangeType(val, prop.Property.Type);
            }
            catch (Exception e)
            {
                throw new InvalidCastException($"Unable to convert {key}={val ?? "null"} to type {prop.Property.Type.Name} for property {prop.Property.NameClass}", e);
            }

            prop.Property.SetValue(this, valConverted);
            usedKeys.Add(key);
        }

        foreach (var (key, val) in result.Where(kvp => !usedKeys.Contains(kvp.Key)))
        {
            log.LogWarning("Result contained {ResultName}={ResultValue} but no property was defined on type {Type}", key, val, GetType().NameFormatted());
        }

        foreach (var property in props
            .Where(kvp => !usedKeys.Contains(kvp.Key))
            .OrderBy(kvp => kvp.Value)
            .Select(o => o.Value.Property)
        )
        {
            log.LogWarning("Property {Property} was not set", property.NameClass);
        }
    }

    // ReSharper disable once UnusedParameter.Global
    protected virtual Dictionary<string, string?> ToDictionary(DataReaderResult result, string? group)
    {
        var cols = result.Columns.Names;
        var row = result.Rows.FirstOrDefault();
        if (row == null) throw new Exception("No rows returned");
        var values = new Dictionary<string, string?>();
        for (var i = 0; i < cols.Count; i++)
        {
            var name = cols[i].TrimOrNull();
            if (name == null) continue;
            values.Add(name, row.GetString(i));
        }

        return values;
    }

    public virtual void Load(DataReaderResult result, string? group) => Load(ToDictionary(result, group), group);

    public abstract void Load(Sql sql);


    private List<(string Name, string Value)> ToStringProperties() =>
        GetProperties().PropertyItems
            .Select(p => (p.Property.Name, p.Property.GetValue(this).ToStringGuessFormat() ?? string.Empty))
            .ToList();

    public override string ToString() => GetType().NameFormatted() + "(" + ToStringProperties().Select(p => p.Name + "=" + p.Value).ToStringDelimited(", ") + ")";

    public virtual string ToStringDetailed()
    {
        var ps = ToStringProperties();
        var sb = new StringBuilder();
        sb.AppendLine(GetType().NameFormatted());
        if (ps.IsNotEmpty())
        {
            var nameLength = ps.Select(o => o.Name.Length).Max() + 2;
            foreach (var p in ps) sb.AppendLine(p.Name.PadLeft(nameLength) + ": " + p.Value);
        }

        return sb.ToString().Trim();
    }


}
