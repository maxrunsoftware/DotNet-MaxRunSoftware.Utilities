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

namespace MaxRunSoftware.Utilities.Database;


public class DbParameter
{
    public DbParameter(string name, DbType type = DbType.String)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public DbType Type { get; }
}
public class DbParameterValue : DbParameter
{
    public DbParameterValue(string name, object? value, DbType type = DbType.String) : base(name: name, type: type)
    {
        Value = value;
    }

    public object? Value { get; }
}

public abstract class SqlBase : Sql
{
    protected readonly ILogger log;
    protected SqlBase(IDbConnection connection) : base(connection)
    {
        log = Constant.GetLogger(GetType());
    }

    #region Schema

    public override DatabaseSchemaDatabase GetCurrentDatabase() => GetCurrentSchema().Database;

    protected IEnumerable<T> GetSchemaObjects<T>(List<SqlError> errors, StringBuilder sql, Func<string?[], T> converter) where T : DatabaseSchemaObject =>
        GetSchemaObjects(errors, sql.ToString(), converter);

    protected IEnumerable<T> GetSchemaObjects<T>(List<SqlError> errors, string sql, Func<string?[], T> converter) where T : DatabaseSchemaObject
    {
        var tbl = this.QueryStrings(sql, out var exception);
        if (exception != null)
        {
            errors.Add(new SqlError(sql, exception));
            yield break;
        }

        foreach (var row in tbl)
        {
            T? so = null;
            try
            {
                so = converter(row);
            }
            catch (Exception e)
            {
                errors.Add(new SqlError(sql, e));
            }

            if (so != null) yield return so;
        }
    }

    #region GetDatabases

    public override IEnumerable<DatabaseSchemaDatabase> GetDatabases()
    {
        var errors = new List<SqlError>();
        foreach (var soDatabase in GetDatabases(errors))
        {
            if (DialectSettings.IsExcluded(soDatabase)) continue;
            yield return soDatabase;
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }
    protected abstract IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors);

    #endregion GetDatabases

    #region GetSchemas
    public override IEnumerable<DatabaseSchemaSchema> GetSchemas() => GetSchemas(GetDatabases());
    public override IEnumerable<DatabaseSchemaSchema> GetSchemas(DatabaseSchemaDatabase database) => GetSchemas(new []{ database });

    private IEnumerable<DatabaseSchemaSchema> GetSchemas(IEnumerable<DatabaseSchemaDatabase> databases)
    {
        var errors = new List<SqlError>();
        foreach (var soDatabase in databases)
        {
            if (DialectSettings.IsExcluded(soDatabase)) continue;
            foreach (var soSchema in GetSchemas(errors, soDatabase))
            {
                if (DialectSettings.IsExcluded(soSchema)) continue;
                yield return soSchema;
            }
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }
    protected abstract IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, DatabaseSchemaDatabase database);

    #endregion GetSchemas

    protected virtual DbType? GetDbType(string? typeName)
    {
        typeName = typeName.TrimOrNull();
        if (typeName == null) return null;
        var type = DatabaseTypes.TypeNames.GetValueNullable(typeName);
        return type?.DbType;
    }

    #region GetTables

    protected class GetTablesFilter
    {
        public GetTablesFilter(Sql sql, DatabaseSchemaDatabase? database, DatabaseSchemaSchema? schema)
        {
            Database = database;
            Schema = schema;

            DatabaseName =
                Schema?.Database.DatabaseName
                ?? Database?.DatabaseName
                ?? throw new NullReferenceException(nameof(DatabaseSchemaDatabase.DatabaseName) + " cannot be null");
            DatabaseNameEscaped = sql.Escape(DatabaseName);

            SchemaName = Schema?.SchemaName;
            SchemaNameEscaped = SchemaName == null ? null : sql.Escape(SchemaName);


        }

        public DatabaseSchemaDatabase? Database { get; }
        public DatabaseSchemaSchema? Schema { get; }

        public string DatabaseName { get; }
        public string DatabaseNameEscaped { get; }

        public string? SchemaName { get; }
        public string? SchemaNameEscaped { get; }

    }

    public override IEnumerable<DatabaseSchemaTable> GetTables() => GetTables(GetDatabases().Select(o => new GetTablesFilter(this, o, null)).ToArray());
    public override IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaDatabase database) => GetTables(new GetTablesFilter(this, database, null));
    public override IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaSchema schema) => GetTables(new GetTablesFilter(this, null, schema));

    private IEnumerable<DatabaseSchemaTable> GetTables(params GetTablesFilter[] filters)
    {
        var errors = new List<SqlError>();
        foreach (var filter in filters)
        {
            if (filter.Database == null && filter.Schema == null) throw new NotImplementedException();
            if (filter.Database != null && DialectSettings.IsExcluded(filter.Database)) continue;
            if (filter.Schema != null && DialectSettings.IsExcluded(filter.Schema)) continue;

            foreach (var soTable in GetTables(errors, filter))
            {
                if (DialectSettings.IsExcluded(soTable)) continue;
                yield return soTable;
            }
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }
    protected abstract IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetTablesFilter filter);

    #endregion GetTables

    #region GetTableColumns
    protected class GetTableColumnsFilter
    {
        public GetTableColumnsFilter(Sql sql, DatabaseSchemaDatabase? database, DatabaseSchemaSchema? schema, DatabaseSchemaTable? table)
        {
            Database = database;
            Schema = schema;
            Table = table;

            DatabaseName =
                Table?.Schema.Database.DatabaseName
                ?? Schema?.Database.DatabaseName
                ?? Database?.DatabaseName
                ?? throw new NullReferenceException(nameof(DatabaseSchemaDatabase.DatabaseName) + " cannot be null");
            DatabaseNameEscaped = sql.Escape(DatabaseName);

            SchemaName =
                Table?.Schema.SchemaName
                ?? Schema?.SchemaName;
            SchemaNameEscaped = SchemaName == null ? null : sql.Escape(SchemaName);

            TableName = Table?.TableName;
            TableNameEscaped = TableName == null ? null : sql.Escape(TableName);
        }

        public DatabaseSchemaDatabase? Database { get; }
        public DatabaseSchemaSchema? Schema { get; }
        public DatabaseSchemaTable? Table { get; }

        public string DatabaseName { get; }
        public string DatabaseNameEscaped { get; }

        public string? SchemaName { get; }
        public string? SchemaNameEscaped { get; }

        public string? TableName { get; }
        public string? TableNameEscaped { get; }
    }

    public override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns() => GetTableColumns(GetDatabases().Select(o => new GetTableColumnsFilter(this, o, null, null)).ToArray());
    public override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaDatabase database) => GetTableColumns(new GetTableColumnsFilter(this, database, null, null));
    public override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaSchema schema) => GetTableColumns(new GetTableColumnsFilter(this, null, schema, null));
    public override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaTable table) => GetTableColumns(new GetTableColumnsFilter(this, null, null, table));

    private IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(params GetTableColumnsFilter[] filters)
    {
        var errors = new List<SqlError>();
        foreach (var filter in filters)
        {
            if (filter.Database == null && filter.Schema == null && filter.Table == null) throw new NotImplementedException();
            if (filter.Database != null && DialectSettings.IsExcluded(filter.Database)) continue;
            if (filter.Schema != null && DialectSettings.IsExcluded(filter.Schema)) continue;
            if (filter.Table != null && DialectSettings.IsExcluded(filter.Table)) continue;

            foreach (var soColumn in GetTableColumns(errors, filter))
            {
                if (DialectSettings.IsExcluded(soColumn)) continue;
                yield return soColumn;
            }
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }
    protected abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetTableColumnsFilter filter);

    #endregion GetTableColumns
    #endregion Schema
}

// ReSharper disable PropertyCanBeMadeInitOnly.Global
public abstract class Sql : IDisposable
{
    private readonly ILogger log = Constant.GetLogger<Sql>();
    private readonly SingleUse isDisposed = new();
    public virtual IDbConnection Connection { get; }

    protected Sql(IDbConnection connection)
    {
        Connection = connection;
    }

    protected record class SqlError(string Sql, Exception Error);

    public abstract DatabaseAppType DatabaseAppType { get; }
    public bool ExceptionShowFullSql { get; set; }

    #region DatabaseTypes

    protected abstract Type DatabaseTypesEnum { get; }
    public DatabaseTypes DatabaseTypes => DatabaseTypes.Get(DatabaseTypesEnum);
    #endregion DatabaseTypes

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (isDisposed.TryUse())
            {
                Connection.DisposeSafely(log);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable


    #region SqlDialectSettings

    public abstract DatabaseDialectSettings DialectSettingsDefault { get; }

    private DatabaseDialectSettings? dialectSettings;
    public DatabaseDialectSettings DialectSettings
    {
        get => dialectSettings ??= DialectSettingsDefault.Copy();
        set => dialectSettings = value;
    }

    #endregion SqlDialectSettings

    #region CommandSettings

    public int CommandTimeout { get; set; } = 60 * 60 * 24; // 24 hours

    public bool CommandInsertCoerceValues { get; set; }

    private int commandInsertBatchSize = 1000;
    public int CommandInsertBatchSize { get => commandInsertBatchSize; set => commandInsertBatchSize = value < 1 ? 1 : value; }

    private int commandInsertBatchSizeMax = 2000; // MSSQL limit
    public int CommandInsertBatchSizeMax { get => commandInsertBatchSizeMax; set => commandInsertBatchSizeMax = value < 1 ? 1 : value; }

    #endregion CommandSettings

    #region Schema

    public abstract DatabaseSchemaDatabase GetCurrentDatabase();
    public abstract DatabaseSchemaSchema GetCurrentSchema();

    public abstract IEnumerable<DatabaseSchemaDatabase> GetDatabases();

    public abstract IEnumerable<DatabaseSchemaSchema> GetSchemas();
    public abstract IEnumerable<DatabaseSchemaSchema> GetSchemas(DatabaseSchemaDatabase database);

    public abstract IEnumerable<DatabaseSchemaTable> GetTables();
    public abstract IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaDatabase database);
    public abstract IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaSchema schema);

    public abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns();
    public abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaDatabase database);
    public abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaSchema schema);
    public abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaTable table);

    public virtual bool GetTableExists(DatabaseSchemaTable table) => !GetTableColumns(table).IsEmpty();

    public abstract bool DropTable(DatabaseSchemaTable table);

    #endregion Schema

    #region Helpers

    protected virtual AggregateException CreateExceptionInSqlStatements(IEnumerable<SqlError> errors)
    {
        var valuesArray = errors.ToArray();
        var sb = new StringBuilder();
        sb.Append($"Error executing {valuesArray.Length} SQL queries");
        for (var i = 0; i < valuesArray.Length; i++)
        {
            var iStr = (i + 1).ToString().PadLeft(valuesArray.Length.ToString().Length);
            var eName = valuesArray[i].Error.GetType().NameFormatted();
            var eMsg = valuesArray[i].Error.Message;
            var ss = valuesArray[i].Sql;
            sb.Append($"{Constant.NewLine}  [{iStr}] {eName}: {eMsg} --> {ss}");
        }
        return new AggregateException(sb.ToString(), valuesArray.Select(o => o.Error));
    }

    public virtual string Escape(params string?[] objectsToEscape)
    {
        if (objectsToEscape.Length == 0) return string.Empty;
        if (objectsToEscape.Length == 1) return objectsToEscape[0] == null ? string.Empty : DialectSettings.Escape(objectsToEscape[0]!);

        Func<string, string> esc = DialectSettings.Escape;

        var sb = new StringBuilder();

        foreach (var obj in objectsToEscape)
        {
            if (obj == null) continue;
            var objEscaped = esc(obj).TrimOrNull();
            if (objEscaped == null) continue;
            if (sb.Length > 0) sb.Append('.');
            sb.Append(objEscaped);
        }

        return sb.ToString();
    }

    public virtual string Unescape(string objectToUnescape) => DialectSettings.Unescape(objectToUnescape);


    public virtual string[] GenerateParameterNames(int count) => Enumerable.Range(0, count).Select(GenerateParameterName).ToArray();

    public virtual string GenerateParameterName(int index) => "@p" + index;



    #region CreateCommand


    protected virtual Tuple<IDbCommand, IDbDataParameter[]> CreateCommand<TParameter>(string sql, IReadOnlyList<TParameter> parameters)
    where TParameter : DbParameter
    {
        var command = Connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = sql;
        command.CommandTimeout = CommandTimeout;
        var ps = new List<IDbDataParameter>();
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.DbType = parameter.Type;
            if (parameter is DbParameterValue parameterValue)
            {
                p.Value = parameterValue.Value;
            }
            ps.Add(p);
        }
        return new Tuple<IDbCommand, IDbDataParameter[]>(command, ps.ToArray());
    }

    public IDbCommand CreateCommand() => CreateCommand(string.Empty, Array.Empty<DbParameter>()).Item1;

    public Tuple<IDbCommand, IDbDataParameter[]> CreateCommandWithParameters(string sql, params DbParameter[] parameters) =>
        CreateCommand(sql, parameters);

    public Tuple<IDbCommand, IDbDataParameter[]> CreateCommandWithValues(string sql, params DbParameterValue[] values) =>
        CreateCommand(sql, values.Select(o => (DbParameter)o).ToArray());


    #endregion CreateCommand

    #region Query

    public virtual object? QueryScalar(string sql, params DbParameterValue[] values)
    {
        log.LogTrace(nameof(QueryScalar) + ": {Sql}", sql);

        var tpl = CreateCommandWithValues(sql, values);
        using var command = tpl.Item1;
        var result = command.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    public virtual  List<object?[]> Query(string sql, params DbParameterValue[] values)
    {
        log.LogTrace(nameof(Query) + ": {Sql}", sql);

        var tpl = CreateCommandWithValues(sql, values);
        using var command = tpl.Item1;
        var result = command.ExecuteReaderResult();
        return result == null ? new List<object?[]>() : new List<object?[]>(result.Rows.Select(row => row.ToArray()));
    }

    #endregion Query

    #region NonQuery

    public virtual int NonQuery(string sql, params DbParameterValue[] values)
    {
        log.LogTrace(nameof(NonQuery) + ": {Sql}", sql);

        var tpl = CreateCommandWithValues(sql, values);
        using var command = tpl.Item1;
        return command.ExecuteNonQuery();
    }

    public virtual  int NonQuery(string sql, out Exception? exception, params DbParameterValue[] values)
    {
        try
        {
            exception = null;
            return NonQuery(sql, values);
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return 0;
        }
    }

    #endregion NonQuery

    #endregion Helpers

    #region Insert

    public virtual int Insert(string? database, string? schema, string table, params DbParameterValue[] values)
    {
        var dst = Escape(database, schema, table);
        values.CheckNotEmpty();
        values = values.Select(o => new DbParameterValue(Escape(o.Name), o.Value, o.Type)).ToArray();
        var parameterNames = GenerateParameterNames(values.Length);

        var sqlColumnNames = values.Select(o => o.Name).ToStringDelimited(",");
        var sqlParameters = parameterNames.ToStringDelimited(",");
        var sql = $"INSERT INTO {dst} ({sqlColumnNames}) VALUES ({sqlParameters})";
        //sql += ";"; // breaks Oracle
        var tuple = CreateCommand(sql, values);
        using var command = tuple.Item1;

        for (var i = 0; i < values.Length; i++)
        {
            command.AddParameter(
                parameterName: parameterNames[i],
                value: values[i].Value,
                dbType: DbType.String
                );
        }

        return command.ExecuteNonQuery();
    }

    public virtual int Insert(string? database, string? schema, string table, IDictionary<string, object?> values) => Insert(
        database: database,
        schema: schema,
        table: table,
        values: values.Select(kvp => new DbParameterValue(kvp.Key, kvp.Value)).ToArray()
    );

    public virtual int Insert(string? database, string? schema, string table, IDictionary<string, string?> values) => Insert(
        database: database,
        schema: schema,
        table: table,
        values: values.Select(kvp => new DbParameterValue(kvp.Key, kvp.Value)).ToArray()
    );

    #endregion Insert





}


public static class SqlExtensions
{
    private static string? ConvertToStringDefault(object? obj) => obj.ToStringGuessFormat();



    #region Query


    public static List<object?[]> Query(this Sql instance, string sql, out Exception? exception, params DbParameterValue[] values)
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
            return new List<object?[]>();
        }
    }

    public static List<string?[]> QueryStrings(this Sql instance, string sql, params DbParameterValue[] values) => instance.QueryStrings(sql, ConvertToStringDefault, values);
    public static  List<string?[]> QueryStrings(this Sql instance, string sql, Func<object?, string?> converter, params DbParameterValue[] values) =>
        instance.Query(sql, values).Select(o => o.Select(converter).ToArray()).ToList();

    public static List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, params DbParameterValue[] values) => instance.QueryStrings(sql, out exception, ConvertToStringDefault, values);
    public static  List<string?[]> QueryStrings(this Sql instance, string sql, out Exception? exception, Func<object?, string?> converter, params DbParameterValue[] values) =>
        instance.Query(sql, out exception, values).Select(o => o.Select(converter).ToArray()).ToList();


    #endregion Query

    #region QueryScalar

    public static string? QueryScalarString(this Sql instance, string sql, params DbParameterValue[] values) => instance.QueryScalarString(sql, ConvertToStringDefault, values);
    public static string? QueryScalarString(this Sql instance, string sql, Func<object?, string?> converter, params DbParameterValue[] values) => converter(instance.QueryScalar(sql, values));



    #endregion QueryScalar

    #region QueryColumn

    public static  List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, params DbParameterValue[] values) =>
        instance.Query(sql, values).Select(o => o[columnIndex]).ToList();

    public static List<object?> QueryColumn(this Sql instance, string sql, int columnIndex, out Exception? exception, params DbParameterValue[] values) =>
        instance.Query(sql, out exception, values).Select(o => o[columnIndex]).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, params DbParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, ConvertToStringDefault, values);

    public static  List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, Func<object?, string?> converter, params DbParameterValue[] values) =>
        instance.QueryColumn(sql, columnIndex, values).Select(converter).ToList();

    public static List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, params DbParameterValue[] values) => instance.QueryColumnStrings(sql, columnIndex, out exception, ConvertToStringDefault, values);

    public static  List<string?> QueryColumnStrings(this Sql instance, string sql, int columnIndex, out Exception? exception, Func<object?, string?> converter, params DbParameterValue[] values) =>
        instance.QueryColumn(sql, columnIndex, out exception, values).Select(converter).ToList();

    #endregion QueryColumn

}
