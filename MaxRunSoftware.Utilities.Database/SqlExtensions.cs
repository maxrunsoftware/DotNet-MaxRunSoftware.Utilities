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

public static class SqlExtensions
{
    private static string? ConvertToStringDefault(object? obj) => obj.ToStringGuessFormat();

    #region Escape

    public static string Escape(this Sql instance, IReadOnlyList<string?> objectsToEscape) => instance.Escape('.', objectsToEscape);
    public static string Escape(this Sql instance, params string?[] objectsToEscape) => instance.Escape('.', objectsToEscape);
    public static string Escape(this Sql instance, char delimiter, params string?[] objectsToEscape) => instance.Escape(delimiter, (IReadOnlyList<string?>)objectsToEscape);

    #endregion Escape


    #region Query

    public static int NonQuery(this Sql instance, string sql, out Exception? exception, params DatabaseParameterValue[] values)
    {
        try
        {
            exception = null;
            return instance.NonQuery(sql, values);
        }
        catch (Exception e)
        {
            Constant.GetLogger(typeof(Sql)).LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return 0;
        }
    }

    public static DataReaderResult? Query(this Sql instance, string sql, out Exception? exception, params DatabaseParameterValue[] values)
    {
        try
        {
            exception = null;
            return instance.Query(sql, values);
        }
        catch (Exception e)
        {
            Constant.GetLogger(typeof(Sql)).LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return null;
        }
    }

    public static List<object?[]> QueryObjects(this Sql instance, string sql, params DatabaseParameterValue[] values)
    {
        var result = instance.Query(sql, values);
        return result == null ? new List<object?[]>() : new List<object?[]>(result.Rows.Select(row => row.ToArray()));
    }
    public static List<object?[]> QueryObjects(this Sql instance, string sql, out Exception? exception, params DatabaseParameterValue[] values)
    {
        try
        {
            exception = null;
            return instance.QueryObjects(sql, values);
        }
        catch (Exception e)
        {
            Constant.GetLogger(typeof(Sql)).LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return new List<object?[]>();
        }
    }

    public static List<string?[]> QueryStrings(this Sql instance, string sql, params DatabaseParameterValue[] values) => instance.QueryStrings(sql, ConvertToStringDefault, values);
    public static  List<string?[]> QueryStrings(this Sql instance, string sql, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, values).Select(o => o.Select(converter).ToArray()).ToList();

    public static List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, params DatabaseParameterValue[] values) => instance.QueryStrings(sql, out exception, ConvertToStringDefault, values);
    public static  List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, out exception, values).Select(o => o.Select(converter).ToArray()).ToList();


    #endregion Query

    #region QueryScalar

    public static string? QueryScalarString(this Sql instance, string sql, params DatabaseParameterValue[] values) => instance.QueryScalarString(sql, ConvertToStringDefault, values);
    public static string? QueryScalarString(this Sql instance, string sql, Func<object?, string?> converter, params DatabaseParameterValue[] values) => converter(instance.QueryScalar(sql, values));



    #endregion QueryScalar

    #region QueryColumn

    public static  List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, values).Select(o => o[columnIndex]).ToList();

    public static List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, out Exception? exception, params DatabaseParameterValue[] values) =>
        instance.QueryObjects(sql, out exception, values).Select(o => o[columnIndex]).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, params DatabaseParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, ConvertToStringDefault, values);

    public static  List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
        instance.QueryColumn(sql, columnIndex, values).Select(converter).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, params DatabaseParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, out exception, ConvertToStringDefault, values);

    public static  List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, Func<object?, string?> converter, params DatabaseParameterValue[] values) =>
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
    public static int Insert(this Sql instance, DatabaseSchemaTable table, IEnumerable<(string ColumnName, string? ColumnValue)> values) =>
        instance.Insert(table, values.Select(o => (o.ColumnName, (object?)o.ColumnValue)));

    public static int Insert(this Sql instance, DatabaseSchemaTable table, IDictionary<string, object?> values) =>
        instance.Insert(table, values.Select(o => (o.Key, o.Value)));

    public static int Insert(this Sql instance, DatabaseSchemaTable table, IDictionary<string, string?> values) =>
        instance.Insert(table, values.Select(o => (o.Key, (object?)o.Value)));

    private static DatabaseSchemaTable InsertCreateTable(Sql instance, string? database, string? schema, string table) =>
        new(database ?? instance.GetCurrentDatabase().Name, schema ?? instance.GetCurrentSchema().Name, table);

    public static int Insert(this Sql instance, string? database, string? schema, string table, IDictionary<string, object?> values) =>
        instance.Insert(InsertCreateTable(instance, database, schema, table), values);

    public static int Insert(this Sql instance, string? database, string? schema, string table, IDictionary<string, string?> values) =>
        instance.Insert(InsertCreateTable(instance, database, schema, table), values);


    #endregion Insert
}
