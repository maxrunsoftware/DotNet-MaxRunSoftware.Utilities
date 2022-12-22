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

public class DataReaderSchemaColumn
{
    public int Index { get; }
    public string ColumnName { get; }
    public Type FieldType { get; }
    public string DataTypeName { get; }

    public DataReaderSchemaColumn(int index, string columnName, Type fieldType, string dataTypeName)
    {
        Index = index;
        ColumnName = columnName;
        FieldType = fieldType;
        DataTypeName = dataTypeName;
    }

    public DataReaderSchemaColumn(IDataReader reader, int columnIndex)
    {
        Index = columnIndex;
        ColumnName = reader.GetName(columnIndex);
        FieldType = reader.GetFieldType(columnIndex);
        DataTypeName = reader.GetDataTypeName(columnIndex);
    }

    public static List<DataReaderSchemaColumn> Create(IDataReader reader) =>
        Enumerable.Range(0, reader.FieldCount).Select(i => new DataReaderSchemaColumn(reader, i)).ToList();
}

public class DataReaderSchemaColumnExtended
{
    public IReadOnlyDictionary<string, object?> Values { get; }

    /// <summary>
    /// FYI: Check for MySql ordinal/index bug
    /// https://stackoverflow.com/questions/51253130/mysqldatareader-getschematable-columnordinal-is-1-of-the-actual-data
    /// https://bugs.mysql.com/bug.php?id=61477
    /// </summary>
    public int? ColumnOrdinal => GetValue<int>(nameof(ColumnOrdinal));

    public string? ColumnName => GetValue<string>(nameof(ColumnName));
    public int? ColumnSize => GetValue<int>(nameof(ColumnSize));
    public int? NumericPrecision => GetValue<int>(nameof(NumericPrecision));
    public int? NumericScale => GetValue<int>(nameof(NumericScale));
    public bool? IsUnique => GetValue<bool>(nameof(IsUnique));
    public bool? IsKey => GetValue<bool>(nameof(IsKey));
    public bool? IsRowId => GetValue<bool>(nameof(IsRowId));
    public string? BaseServerName => GetValue<string>(nameof(BaseServerName));
    public string? BaseCatalogName => GetValue<string>(nameof(BaseCatalogName));
    public string? BaseColumnName => GetValue<string>(nameof(BaseColumnName));
    public string? BaseSchemaName => GetValue<string>(nameof(BaseSchemaName));
    public string? BaseTableName => GetValue<string>(nameof(BaseTableName));
    public Type? DataType => GetValue<Type>(nameof(DataType));
    public bool? AllowDbNull => GetValue<bool>(nameof(AllowDbNull));
    public int? ProviderType => GetValue<int>(nameof(ProviderType));
    public bool? IsAliased => GetValue<bool>(nameof(IsAliased));
    public bool? IsByteSemantic => GetValue<bool>(nameof(IsByteSemantic));
    public bool? IsExpression => GetValue<bool>(nameof(IsExpression));
    public bool? IsIdentity => GetValue<bool>(nameof(IsIdentity));
    public bool? IsAutoIncrement => GetValue<bool>(nameof(IsAutoIncrement));
    public bool? IsRowVersion => GetValue<bool>(nameof(IsRowVersion));
    public bool? IsHidden => GetValue<bool>(nameof(IsHidden));
    public bool? IsLong => GetValue<bool>(nameof(IsLong));
    public bool? IsReadOnly => GetValue<bool>(nameof(IsReadOnly));
    public bool? IsValueLob => GetValue<bool>(nameof(IsValueLob));
    public Type? ProviderSpecificDataType => GetValue<Type>(nameof(ProviderSpecificDataType));
    public string? IdentityType => GetValue<string>(nameof(IdentityType));
    public string? DataTypeName => GetValue<string>(nameof(DataTypeName));
    public string? XmlSchemaCollectionDatabase => GetValue<string>(nameof(XmlSchemaCollectionDatabase));
    public string? XmlSchemaCollectionOwningSchema => GetValue<string>(nameof(XmlSchemaCollectionOwningSchema));
    public string? XmlSchemaCollectionName => GetValue<string>(nameof(XmlSchemaCollectionName));
    public string? UdtAssemblyQualifiedName => GetValue<string>(nameof(UdtAssemblyQualifiedName));
    public string? UdtTypeName => GetValue<string>(nameof(UdtTypeName));
    public int? NonVersionedProviderType => GetValue<int>(nameof(NonVersionedProviderType));
    public bool? IsColumnSet => GetValue<bool>(nameof(IsColumnSet));

    private T? GetValue<T>(string name)
    {
        if (!Values.TryGetValue(name, out var o)) return default;
        if (o == null) return default;
        if (o == DBNull.Value) return default;
        if (o.GetType().IsAssignableTo(typeof(T))) return (T)o;
        var oo = Util.ChangeType(o, typeof(T));
        return (T?)oo;
    }

    public DataReaderSchemaColumnExtended(IReadOnlyDictionary<string, object?> values)
    {
        Values = values;
    }
    public DataReaderSchemaColumnExtended(IDictionary<string, object?> values)
    {
        Values = values.AsReadOnly();
    }

    public static List<DataReaderSchemaColumnExtended> Create(IDataReader reader)
    {
        var result = new List<DataReaderSchemaColumnExtended>();
        var dataTable = reader.GetSchemaTable();
        if (dataTable != null)
        {
            var dataTableColumns = dataTable.Columns.Cast<DataColumn>().ToList();
            foreach (DataRow row in dataTable.Rows)
            {
                var d = new DictionaryIndexed<string, object?>(StringComparer.OrdinalIgnoreCase); // There should be no duplicate column names in IDataReader.GetSchemaTable()
                foreach (var col in dataTableColumns)
                {
                    var v = row[col];
                    if (v == DBNull.Value) v = null;
                    d[col.ColumnName] = v;
                }

                var colExt = new DataReaderSchemaColumnExtended(d);
                result.Add(colExt);
            }
        }

        return result;
    }

    public override string ToString()
    {
        if (Values.Count < 1) return GetType().NameFormatted() + " { }" + Constant.NewLine;
        var maxColumnLen = Values.Keys.Select(o => o.Length).Max() + 3;

        var sb = new StringBuilder();
        sb.AppendLine(GetType().NameFormatted() + " {");
        foreach (var val in Values) sb.AppendLine("  " + (val.Key + " :").PadRight(maxColumnLen) + (val.Value.ToStringGuessFormat() ?? string.Empty));
        sb.AppendLine("}");

        return sb.ToString();
    }
}

public static class DataReaderSchemaExtensions
{
    public static List<DataReaderSchemaColumn> GetSchema(this IDataReader reader) => DataReaderSchemaColumn.Create(reader);
    public static List<DataReaderSchemaColumnExtended> GetSchemaExtended(this IDataReader reader) => DataReaderSchemaColumnExtended.Create(reader);
}
