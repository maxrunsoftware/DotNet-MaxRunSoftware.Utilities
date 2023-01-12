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

public abstract class DatabaseServerProperties
{
    protected CachedProperties GetProperties()
    {
        return GetProperties(GetType());
    }

    protected static CachedProperties GetProperties(TypeSlim type)
    {
        lock (setPropertyValuesCacheLock)
        {
            if (!setPropertyValuesCache.TryGetValue(type, out var props))
            {
                props = new CachedProperties(type);
                setPropertyValuesCache.Add(type, props);
            }

            return props;
        }
    }

    private static object? SetPropertyValueConvert(PropertyInfo property, string? value)
    {
        var targetType = property.PropertyType;

        if (targetType == typeof(string)) return value;
        var valueTrimmed = value.TrimOrNull();
        if (targetType.Equals<bool, bool?>()) return valueTrimmed?.ToBool();
        if (targetType.Equals<byte, byte?>()) return valueTrimmed?.ToByte();
        if (targetType.Equals<int, int?>()) return valueTrimmed?.ToInt();
        if (targetType.Equals<long, long?>()) return valueTrimmed?.ToLong();
        if (targetType.Equals<DateTime, DateTime?>()) return valueTrimmed?.ToDateTime();
        throw new NotImplementedException($"Undefined conversion for property {property.Name} type {targetType.NameFormatted()}");
    }
    public static void SetPropertyValue(object instance, PropertyInfo property, string? value)
    {
        var objConverted = SetPropertyValueConvert(property, value);
        if (objConverted == null && !property.IsNullable()) throw new Exception($"Could not set non-nullable property {property.Name} of type {property.PropertyType.NameFormatted()} to null");
        property.SetValue(instance, objConverted);
    }

    // ReSharper disable once InconsistentNaming
    private static readonly object setPropertyValuesCacheLock = new();

    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<TypeSlim, CachedProperties> setPropertyValuesCache = new();

    protected class CachedProperties
    {
        public TypeSlim Type { get; }
        public ImmutableDictionary<string, PropertyInfo> Properties { get; }
        public ImmutableDictionary<string, PropertyInfo> PropertiesCustom { get; }

        public CachedProperties(TypeSlim type)
        {
            Type = type;
            var props = ImmutableDictionary.CreateBuilder<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            var propsCustom = ImmutableDictionary.CreateBuilder<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in type.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanRead) continue;

                if (property.CanWrite) props.Add(property.Name, property);
                else propsCustom.Add(property.Name, property);
            }

            Properties = props.ToImmutable();
            PropertiesCustom = propsCustom.ToImmutable();
        }
    }
    public static Dictionary<string, string?> SetPropertyValues(object instance, IDictionary<string, string?> values)
    {
        var props = GetProperties(instance.GetType());

        var propsNotSet = new Dictionary<string, string?>();
        foreach (var kvp in values)
        {
            if (props.Properties.TryGetValue(kvp.Key, out var prop))
            {
                SetPropertyValue(instance, prop, kvp.Value);
            }
            else
            {
                propsNotSet.Add(kvp.Key, kvp.Value);
            }
        }

        return propsNotSet;
    }


    protected virtual string LoadConvertName(string name) => name;

    public virtual void Load(DataReaderResult result)
    {
        var cols = result.Columns.Names;
        var row = result.Rows.FirstOrDefault();
        if (row == null) throw new Exception("No rows returned");
        var values = new Dictionary<string, string?>();
        var prop2Col = new Dictionary<string, string>();
        for (var i = 0; i < cols.Count; i++)
        {
            var name = LoadConvertName(cols[i]).Trim();
            if (name.Length == 0) continue;
            prop2Col.Add(name, cols[i]);
            values.Add(name, row.GetString(i));
        }

        var propsNotSet = SetPropertyValues(this, values);

        var log = Constant.GetLogger(GetType());
        foreach (var kvp in propsNotSet)
        {
            log.LogWarning(
                "Property {PropertyName} defined on type {Type}: {PropertyValue}",
                prop2Col[kvp.Key],
                GetType().NameFormatted(),
                kvp.Value
            );
        }
    }

    public abstract void Load(Sql sql);


    private List<(string Name, string Value)> ToStringProperties()
    {
        var c = GetProperties();
        return c.Properties
            .Select(kvp => kvp.Value)
            .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal)
            .Concat(c.PropertiesCustom
                .Select(kvp => kvp.Value)
                .OrderBy(o => o.Name, Constant.StringComparer_OrdinalIgnoreCase_Ordinal))
            .Select(p => (p.Name, p.GetValue(this).ToStringGuessFormat() ?? string.Empty))
            .ToList();
    }

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
