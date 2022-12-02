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

using System.Numerics;
using Microsoft.Extensions.Logging;

namespace MaxRunSoftware.Utilities.Common;

public class SqlResult
{
    public int Index { get; }
    public SqlResultColumnCollection Columns { get; }
    public SqlResultRowCollection Rows { get; }

    public SqlResult(int index, IDataReader reader)
    {
        Constant.GetLogger<SqlResult>().LogTrace("Reading {Type}[{Index}]", nameof(SqlResult), index);
        Index = index;
        Columns = new SqlResultColumnCollection(reader, this); // Important to construct Columns before Rows
        Rows = new SqlResultRowCollection(reader, this);
    }
}

public static class SqlResultExtensions
{
    public static IEnumerable<SqlResult> ReadSqlResults(this IDataReader reader)
    {
        var i = 0;
        do
        {
            var result = new SqlResult(i, reader);
            yield return result;
            i++;
        } while (reader.NextResult());
    }

    //public static SqlType GetSqlType(this SqlResultColumn sqlResultColumn, Sql sql) => sql.GetSqlDbType(sqlResultColumn.DataTypeName);
}

public class SqlResultColumnCollection : IReadOnlyList<SqlResultColumn>
{
    private readonly List<SqlResultColumn> columns;
    private readonly Dictionary<string, List<SqlResultColumn>> columnsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<SqlResultColumn>> columnsByNameCaseSensitive = new(StringComparer.Ordinal);

    internal readonly bool[] nullableColumns;

    public SqlResult Result { get; }
    public IReadOnlyList<string> Names { get; }
    public SqlResultColumn this[int index] => columns[index];
    public SqlResultColumn this[string name]
    {
        get
        {
            var list = columnsByName[name]; // Should throw exception on missing case-insensitive match
            if (list.Count == 1) return list[0];

            // We have multiple case-insensitive columns with that name so let's try case-sensitive
            if (columnsByNameCaseSensitive.TryGetValue(name, out var listCaseSensitive))
            {
                if (listCaseSensitive.Count == 1) return listCaseSensitive[0];
                if (listCaseSensitive.Count > 1) throw new ArgumentOutOfRangeException(nameof(name), $"Multiple columns ({listCaseSensitive.Count}) with name '{name}'");
            }

            // We have multiple case-insensitive columns with that name but 0 columns matching that name case-sensitive
            throw new ArgumentOutOfRangeException(nameof(name), $"Multiple columns ({list.Count}) with name '{name}'");
        }
    }

    public SqlResultColumnCollection(IDataReader reader, SqlResult result)
    {

        Result = result;
        columns = reader.GetSchema().Select(o => new SqlResultColumn(o, this)).OrderBy(o => o.Index).ToList();
        nullableColumns = new bool[columns.Count];
        Array.Fill(nullableColumns, false); // TODO: Not sure if needed

        foreach (var column in columns)
        {
            columnsByName.AddToList(column.Name, column);
            columnsByNameCaseSensitive.AddToList(column.Name, column);
        }
        Names = columns.Select(o => o.Name).ToList().AsReadOnly();
    }


    public bool Contains(int index) => index >= 0 && index < Count;
    public bool Contains(string name) => columnsByName.ContainsKey(name);
    public bool Contains(SqlResultColumn column) => columns.Contains(column);

    public int Count => columns.Count;

    public IEnumerator<SqlResultColumn> GetEnumerator() => columns.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class SqlResultColumn
{
    private SqlDataReaderSchemaColumn SchemaColumn { get; }

    public SqlResultColumnCollection ColumnCollection { get; }

    public int Index => SchemaColumn.Index;
    public string Name => SchemaColumn.ColumnName;
    public Type Type => SchemaColumn.FieldType;
    public string DataTypeName => SchemaColumn.DataTypeName;

    public bool IsNullable => ColumnCollection.nullableColumns[Index];

    public SqlResultColumn(SqlDataReaderSchemaColumn schemaColumn, SqlResultColumnCollection columnCollection)
    {
        ColumnCollection = columnCollection;
        SchemaColumn = schemaColumn.CheckNotNull(nameof(schemaColumn));
    }
}

public class SqlResultRowCollection : IReadOnlyList<SqlResultRow>
{
    public SqlResult Result { get; }

    private readonly List<SqlResultRow> rows;

    public SqlResultRow this[int index] => rows[index];

    public SqlResultRowCollection(IDataReader reader, SqlResult result)
    {
        Result = result;
        var nullableColumnIndexes = Result.Columns.nullableColumns;
        rows = reader.GetValuesAll().Select(valueRow => new SqlResultRow(valueRow, nullableColumnIndexes)).ToList();
    }

    public IEnumerator<SqlResultRow> GetEnumerator() => rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => rows.Count;

}

public class SqlResultRow : IReadOnlyList<object?>
{
    private readonly object?[] objs;

    public SqlResultRow(object?[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == DBNull.Value) data[i] = null;
        }

        objs = data;
    }

    public SqlResultRow(object?[] data, bool[] nullableColumnIndexes)
    {
        for (var i = 0; i < data.Length; i++)
        {
            if (data[i] == null)
            {
                nullableColumnIndexes[i] = true;
            }
            else if (data[i] == DBNull.Value)
            {
                data[i] = null;
                nullableColumnIndexes[i] = true;
            }
        }

        objs = data;
    }

    public IEnumerator<object?> GetEnumerator() => ((IEnumerable<object?>)objs).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => objs.Length;

    public object? this[int index] => objs[index];


}

public static class SqlResultRowExtensions
{
    public static object? GetObject(this SqlResultRow row, int index) => row[index];

    public static object? GetObject(this SqlResultRow row, SqlResultRowCollection rowCollection, string name) => row[rowCollection.Result.Columns[name].Index];

    public static object? GetObject(this SqlResultRow row, SqlResultColumn column) => row[column.Index];

    public static string? GetString(this SqlResultRow row, int index) => GetObject(row, index).ToStringGuessFormat();

    public static string? GetString(this SqlResultRow row, SqlResultRowCollection rowCollection, string name) => GetObject(row, rowCollection, name).ToStringGuessFormat();

    public static string? GetString(this SqlResultRow row, SqlResultColumn column) => GetObject(row, column).ToStringGuessFormat();

    public static T? Get<T>(this SqlResultRow row, int index) => GetConvert<T>(GetObject(row, index));

    public static T? Get<T>(this SqlResultRow row, SqlResultRowCollection rowCollection, string name) => GetConvert<T>(GetObject(row, rowCollection, name));

    public static T? Get<T>(this SqlResultRow row, SqlResultColumn column) => GetConvert<T>(GetObject(row, column));

    private static T? GetConvert<T>(object? o)
    {
        if (o == null) return default;

        var returnType = typeof(T);

        if (returnType == typeof(object)) return (T)o;

        if (returnType == typeof(byte[])) return (T)o;

        if (returnType == typeof(string)) return (T?)(object?)o.ToStringGuessFormat();

        if (returnType == typeof(char[])) return (T?)(object?)o.ToStringGuessFormat();

        var os = o.ToString().TrimOrNull();
        if (os == null) return default;

        if (CONVERTERS.TryGetValue(returnType, out var converter)) return (T)converter(os);

        return (T)o;
    }


    private static readonly Dictionary<Type, Func<string, object>> CONVERTERS = CreateConverters();

    private static Dictionary<Type, Func<string, object>> CreateConverters()
    {
        var d = new Dictionary<Type, Func<string, object>>();

        d.AddRange(o => o.ToBool(), typeof(bool), typeof(bool?));

        d.AddRange(o => o.ToByte(), typeof(byte), typeof(byte?));
        d.AddRange(o => o.ToSByte(), typeof(sbyte), typeof(sbyte?));

        d.AddRange(o => o.ToShort(), typeof(short), typeof(short?));
        d.AddRange(o => o.ToUShort(), typeof(ushort), typeof(ushort?));

        d.AddRange(o => o.ToInt(), typeof(int), typeof(int?));
        d.AddRange(o => o.ToUInt(), typeof(uint), typeof(uint?));

        d.AddRange(o => o.ToLong(), typeof(long), typeof(long?));
        d.AddRange(o => o.ToULong(), typeof(ulong), typeof(ulong?));

        d.AddRange(o => o.ToFloat(), typeof(float), typeof(float?));
        d.AddRange(o => o.ToDouble(), typeof(double), typeof(double?));
        d.AddRange(o => o.ToDecimal(), typeof(decimal), typeof(decimal?));

        d.AddRange(o => BigInteger.Parse(o), typeof(BigInteger), typeof(BigInteger?));

        d.AddRange(o => o[0], typeof(char), typeof(char?));

        d.AddRange(o => o.ToGuid(), typeof(Guid), typeof(Guid?));

        return new Dictionary<Type, Func<string, object>>(d);
    }
}
