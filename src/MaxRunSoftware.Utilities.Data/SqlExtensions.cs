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

namespace MaxRunSoftware.Utilities.Data;

public static class SqlExtensions
{
    private static string? ConvertToStringDefault(object? obj) => obj.ToStringGuessFormat();
    private static int? ConvertToIntDefault(object? obj) => ConvertToStringDefault(obj).TrimOrNull().ToIntNullable();

    #region Query

    public static List<string?[]> QueryStrings(this Sql instance, string sql, params DatabaseParameterValue[] values) => instance.QueryStrings(sql, ConvertToStringDefault, values);
    public static List<string?[]> QueryStrings(this Sql instance, string sql, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, values).Select(o => o.Select(converter).ToArray()).ToList();

    public static List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, params DatabaseParameterValue[] values) => instance.QueryStrings(sql, out exception, ConvertToStringDefault, values);
    public static List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, out exception, values).Select(o => o.Select(converter).ToArray()).ToList();

    #endregion Query

    #region QueryScalar

    public static string? QueryScalarString(this Sql instance, string sql, params DatabaseParameterValue[] values) => instance.QueryScalarString(sql, ConvertToStringDefault, values);
    public static string? QueryScalarString(this Sql instance, string sql, Func<object?, string?> converter, params DatabaseParameterValue[] values) => converter(instance.QueryScalar(sql, values));

    public static int? QueryScalarInt(this Sql instance, string sql, params DatabaseParameterValue[] values) => instance.QueryScalarInt(sql, ConvertToIntDefault, values);
    public static int? QueryScalarInt(this Sql instance, string sql, Func<object?, int?> converter, params DatabaseParameterValue[] values) => converter(instance.QueryScalar(sql, values));

    #endregion QueryScalar

    #region QueryColumn

    public static List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, values).Select(o => o[columnIndex]).ToList();

    public static List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, out Exception? exception, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, out exception, values).Select(o => o[columnIndex]).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, params DatabaseParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, ConvertToStringDefault, values);

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryColumn(sql, columnIndex, values).Select(converter).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, params DatabaseParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, out exception, ConvertToStringDefault, values);

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryColumn(sql, columnIndex, out exception, values).Select(converter).ToList();

    #endregion QueryColumn

    #region Insert

    public static int Insert(this Sql instance, DatabaseSchemaTable table, IEnumerable<(string ColumnName, object? ColumnValue)> values)
    {
        var itemColumns = new List<(string ColumnName, DbType? ColumnType)>();
        var itemValues = new List<object?>();
        foreach (var value in values)
        {
            itemColumns.Add((value.ColumnName, DbType.String));
            itemValues.Add(value.ColumnValue);
        }

        return instance.Insert(table, itemColumns, itemValues.ToArray().Yield());
    }
//    public static int Insert(this Sql instance, DatabaseSchemaTable table, IEnumerable<(string ColumnName, string? ColumnValue)> values) =>
//        instance.Insert(table, values.Select(o => (o.ColumnName, (object?)o.ColumnValue)));

    public static int Insert(this Sql instance, DatabaseSchemaTable table, IEnumerable<KeyValuePair<string, object?>> values) =>
        instance.Insert(table, values.Select(o => (o.Key, o.Value)));

//    public static int Insert(this Sql instance, DatabaseSchemaTable table, IEnumerable<KeyValuePair<string, string?>> values) =>
//        instance.Insert(table, values.Select(o => (o.Key, o.Value)));

    public static int Insert(this Sql instance, DatabaseSchemaTable table, IDictionary<string, object?> values) =>
        instance.Insert(table, values.Select(o => (o.Key, o.Value)));

//    public static int Insert(this Sql instance, DatabaseSchemaTable table, IDictionary<string, string?> values) =>
//        instance.Insert(table, values.Select(o => (o.Key, (object?)o.Value)));

    #endregion Insert

    #region DatabaseParameter

    public static DatabaseParameterValue NextParameter(this Sql instance, string? value) => instance.NextParameter(value, DbType.String);

    public static DatabaseParameterValue NextParameter(this Sql instance, DatabaseSchemaDatabase database)
    {
        var o = NextParameterSchema(instance, database.DatabaseName);
        return o[0];
    }

    public static (DatabaseParameterValue Database, DatabaseParameterValue Schema)
        NextParameter(this Sql instance, DatabaseSchemaSchema schema)
    {
        var o = NextParameterSchema(instance, schema.Database.DatabaseName, schema.SchemaName);
        return (o[0], o[1]);
    }

    public static (DatabaseParameterValue Database, DatabaseParameterValue Schema, DatabaseParameterValue Table)
        NextParameter(this Sql instance, DatabaseSchemaTable table)
    {
        var o = NextParameterSchema(instance, table.Schema.Database.DatabaseName, table.Schema.SchemaName, table.TableName);
        return (o[0], o[1], o[2]);
    }

    private static DatabaseParameterValue[] NextParameterSchema(this Sql instance, params string?[] names)
    {
        var list = new List<DatabaseParameterValue>();
        foreach (var name in names)
        {
            var n = name;
            if (n != null) n = instance.Unescape(n);
            list.Add(instance.NextParameter(n));
        }

        return list.ToArray();
    }

    #endregion DatabaseParameter

    public static DataReaderResult GetRows(this DatabaseSchemaTable table, Sql sql)
    {
        var sqlCode = new StringBuilder(1024);
        sqlCode.Append("SELECT * FROM ");
        sqlCode.Append(sql.Escape(table));
        return sql.Query(sqlCode.ToString()).CheckNotNull();
    }
}
