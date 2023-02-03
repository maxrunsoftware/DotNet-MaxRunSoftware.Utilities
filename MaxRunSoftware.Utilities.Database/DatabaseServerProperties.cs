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
    protected Dictionary<string, string?> Dictionary { get; set; }
    protected DatabaseServerProperties(Dictionary<string, string?> dictionary) => Dictionary = dictionary;

    public static Dictionary<string, string?> ToDictionaryHorizontal(DataReaderResult result)
    {
        var cols = result.Columns.Names;
        var row = result.Rows.Count > 0 ? result.Rows[0] : null;
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (row == null) return values;
        for (var i = 0; i < cols.Count; i++)
        {
            var name = cols[i].TrimOrNull();
            if (name == null) continue;
            values.Add(name, row.GetString(i));
        }

        return values;
    }

    public static Dictionary<string, string?> ToDictionaryVertical(DataReaderResult result)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in result.Rows)
        {
            var name = row.GetString(0).TrimOrNull();
            if (name == null) continue;
            values.Add(name, row.GetString(1));
        }

        return values;
    }

    protected virtual string? GetStringNullable(string name) => Dictionary.TryGetValue(name, out var value) ? value : null;
    protected string GetString(string name) => GetStringNullable(name).CheckNotNull(name);

    protected int? GetIntNullable(string name) => GetStringNullable(name).ToIntNullable();
    protected int GetInt(string name) => GetString(name).ToInt();

    protected bool? GetBoolNullable(string name) => GetStringNullable(name).ToBoolNullable();
    protected bool GetBool(string name) => GetString(name).ToBool();

    protected long? GetLongNullable(string name) => GetStringNullable(name).ToLongNullable();
    protected long GetLong(string name) => GetString(name).ToLong();

    protected byte? GetByteNullable(string name) => GetStringNullable(name).ToByteNullable();
    protected byte GetByte(string name) => GetString(name).ToByte();

    protected ushort? GetUShortNullable(string name) => GetStringNullable(name).ToUShortNullable();
    protected ushort GetUShort(string name) => GetString(name).ToUShort();

    protected DateTime? GetDateTimeNullable(string name) => GetStringNullable(name).ToDateTimeNullable();
    protected DateTime GetDateTime(string name) => GetString(name).ToDateTime();

    public virtual List<(PropertySlim Property, string Value)> ToStringProperties() =>
        GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(o => o.ToPropertySlim())
            .Where(o => o.IsGettablePublic)
            .Select(p => (p, p.GetValue(this).ToStringGuessFormat() ?? string.Empty))
            .ToList();

    public Dictionary<string, (PropertySlim Property, string Value)> ToStringPropertiesDictionary() => ToStringProperties().ToDictionary(o => o.Property.Name, o => o);

    public override string ToString() => GetType().NameFormatted() + "(" + ToStringProperties().Select(p => p.Property.Name + "=" + p.Value).ToStringDelimited(", ") + ")";

    public virtual string ToStringDetailed()
    {
        return ToStringDetailed(GetType(), (null, this));
    }

    public static string ToStringDetailed(Type type, params (string? Name, DatabaseServerProperties Properties)[] items)
    {
        var sb = new StringBuilder();
        sb.AppendLine(type.NameFormatted());
        foreach (var (name, properties) in items)
        {
            var indent = name == null ? "" : "  ";
            if (name != null) sb.AppendLine($"{indent}{name}:");
            foreach (var (p, v) in properties.ToStringProperties())
            {
                sb.AppendLine($"{indent}  {p.Name}: {v}");
            }
        }
        return sb.ToString().Trim();
    }


}
