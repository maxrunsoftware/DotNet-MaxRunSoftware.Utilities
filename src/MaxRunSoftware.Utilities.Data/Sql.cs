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

// ReSharper disable PropertyCanBeMadeInitOnly.Global
[PublicAPI]
public abstract class Sql : IDisposable
{
    protected readonly ILogger log;
    private readonly SingleUse isDisposed = new();
    private IDbConnection? connection;

    public virtual IDbConnection Connection
    {
        get
        {
            // ReSharper disable once LocalVariableHidesMember
            var connection = this.connection;
            if (isDisposed.IsUsed || connection == null) throw new ObjectDisposedException(nameof(Connection));
            if (connection.State == ConnectionState.Closed)
            {
                log.LogTraceMethod(new(connection), "Opening database connection {ConnectionType}", connection.GetType().FullNameFormatted());
                connection.Open();
            }

            return connection;
        }
    }

    protected Sql(IDbConnection connection, ILoggerFactory loggerProvider)
    {
        log = loggerProvider.CreateLogger(GetType());
        this.connection = connection;
        currentDatabaseName = new(GetCurrentDatabaseName);
        currentSchemaName = new(GetCurrentSchemaName);
    }


    public abstract DatabaseAppType DatabaseAppType { get; }
    public bool ExceptionShowFullSql { get; set; }

    #region Dialect

    public string DefaultDataTypeString { get; set; } = "TEXT";
    public string DefaultDataTypeInteger { get; set; } = "INT";
    public string DefaultDataTypeDateTime { get; set; } = "DATETIME";

    public char? DialectEscapeLeft { get; set; }
    public char? DialectEscapeRight { get; set; }

    public ISet<DatabaseSchemaDatabase> ExcludedDatabases { get; } = new HashSet<DatabaseSchemaDatabase>();
    public ISet<DatabaseSchemaSchema> ExcludedSchemas { get; } = new HashSet<DatabaseSchemaSchema>();
    public ISet<string> ExcludedSchemasGlobal { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public ISet<DatabaseSchemaTable> ExcludedTables { get; } = new HashSet<DatabaseSchemaTable>();
    public ISet<string> ExcludedTablesGlobal { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public ISet<char> IdentifierCharactersValid { get; } = new HashSet<char>(Constant.CharComparer_OrdinalIgnoreCase);
    public ISet<string> ReservedWords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public virtual string GenerateParameterName(int parameterIndex) => "@p" + parameterIndex;

    protected static HashSet<string> ReservedWordsParse(string words) => words.SplitOnWhiteSpace().TrimOrNull().WhereNotNull().ToHashSet(StringComparer.OrdinalIgnoreCase);

    #region Escape

    public virtual bool NeedsEscaping(string objectThatMightNeedEscaping)
    {
        if (ReservedWords.Contains(objectThatMightNeedEscaping)) return true;

        if (!objectThatMightNeedEscaping.ContainsOnly(IdentifierCharactersValid)) return true;

        return false;
    }

    public virtual string Escape(string objectToEscape)
    {
        if (!NeedsEscaping(objectToEscape)) return objectToEscape;

        var el = DialectEscapeLeft;
        if (el != null && !objectToEscape.StartsWith(el.Value)) objectToEscape = el.Value + objectToEscape;

        var er = DialectEscapeRight;
        if (er != null && !objectToEscape.EndsWith(er.Value)) objectToEscape += er.Value;

        return objectToEscape;
    }

    public virtual string Unescape(string objectToUnescape)
    {
        var el = DialectEscapeLeft;
        if (el != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.StartsWith(el.Value))
            {
                objectToUnescape = objectToUnescape.RemoveLeft(1);
            }
        }

        var er = DialectEscapeRight;
        if (er != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.EndsWith(er.Value))
            {
                objectToUnescape = objectToUnescape.RemoveRight(1);
            }
        }

        return objectToUnescape;
    }

    public virtual string Escape(DatabaseSchemaDatabase database) => Escape(database.DatabaseName);
    public virtual string Escape(DatabaseSchemaSchema schema) => Escape(schema.Database.DatabaseName, schema.SchemaName);
    public virtual string Escape(DatabaseSchemaTable table) => Escape(table.Schema.Database.DatabaseName, table.Schema.SchemaName, table.TableName);

    public virtual string Escape(params string?[] objectsToEscape) => Escape((IReadOnlyList<string?>)objectsToEscape);

    public virtual string Escape(IReadOnlyList<string?> objectsToEscape)
    {
        var sb = new StringBuilder();
        foreach (var obj in objectsToEscape)
        {
            if (obj == null) continue;
            var objUnescaped = Unescape(obj);
            if (objUnescaped.TrimOrNull() == null) continue;
            var objEscaped = Escape(objUnescaped);
            if (sb.Length > 0) sb.Append('.');
            sb.Append(objEscaped);
        }

        return sb.ToString();
    }

    #endregion Escape

    #region IsExcluded

    public virtual bool IsExcluded(DatabaseSchemaObject obj) => obj switch
    {
        DatabaseSchemaTableColumn tableColumn => IsExcluded(tableColumn),
        DatabaseSchemaTable table => IsExcluded(table),
        DatabaseSchemaSchema schema => IsExcluded(schema),
        DatabaseSchemaDatabase database => IsExcluded(database),
        _ => throw new NotImplementedException(),
    };

    public virtual bool IsExcluded(DatabaseSchemaDatabase database) => ExcludedDatabases.Contains(database);
    public virtual bool IsExcluded(DatabaseSchemaSchema schema) => IsExcluded(schema.Database) || ExcludedSchemas.Contains(schema) || (schema.SchemaName != null && ExcludedSchemasGlobal.Contains(schema.SchemaName));
    public virtual bool IsExcluded(DatabaseSchemaTable table) => IsExcluded(table.Schema) || ExcludedTables.Contains(table) || ExcludedTablesGlobal.Contains(table.TableName);
    public virtual bool IsExcluded(DatabaseSchemaTableColumn column) => IsExcluded(column.Table);

    #endregion IsExcluded

    #endregion Dialect

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
                var c = connection;
                connection = null;
                c?.DisposeSafely(log);
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region CommandSettings

    public int CommandTimeout { get; set; } = 60 * 60 * 24; // 24 hours

    public bool CommandInsertCoerceValues { get; set; }

    private int commandInsertBatchSize = 1000;
    public int CommandInsertBatchSize { get => commandInsertBatchSize; set => commandInsertBatchSize = value < 1 ? 1 : value; }

    private int commandInsertBatchSizeMax = 2000; // MSSQL limit
    public int CommandInsertBatchSizeMax { get => commandInsertBatchSizeMax; set => commandInsertBatchSizeMax = value < 1 ? 1 : value; }

    #endregion CommandSettings

    #region Schema

    protected virtual DbType? GetDbType(string? typeName)
    {
        typeName = typeName.TrimOrNull();
        if (typeName == null) return null;
        var type = DatabaseTypes.TypeNames.GetValueNullable(typeName);
        return type?.DbType;
    }

    protected class GetSchemaInfoFilter
    {
        public GetSchemaInfoFilter(Sql sql, DatabaseSchemaDatabase? database, DatabaseSchemaSchema? schema = null, DatabaseSchemaTable? table = null)
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
            DatabaseNameUnescaped = sql.Unescape(DatabaseName);

            SchemaName =
                Table?.Schema.SchemaName
                ?? Schema?.SchemaName;
            SchemaNameEscaped = SchemaName == null ? null : sql.Escape(SchemaName);
            SchemaNameUnescaped = SchemaName == null ? null : sql.Unescape(SchemaName);

            TableName = Table?.TableName;
            TableNameEscaped = TableName == null ? null : sql.Escape(TableName);
            TableNameUnescaped = TableName == null ? null : sql.Unescape(TableName);
        }

        public DatabaseSchemaDatabase? Database { get; }
        public DatabaseSchemaSchema? Schema { get; }
        public DatabaseSchemaTable? Table { get; }

        public string DatabaseName { get; }
        public string DatabaseNameEscaped { get; }
        public string DatabaseNameUnescaped { get; }

        public string? SchemaName { get; }
        public string? SchemaNameEscaped { get; }
        public string? SchemaNameUnescaped { get; }

        public string? TableName { get; }
        public string? TableNameEscaped { get; }
        public string? TableNameUnescaped { get; }
    }


    #region Helpers

    protected IEnumerable<T> GetSchemaObjects<T>(List<SqlError> errors, StringBuilder sql, Func<string?[], T> converter) where T : DatabaseSchemaObject =>
        GetSchemaObjects(errors, sql.ToString(), converter);

    protected IEnumerable<T> GetSchemaObjects<T>(List<SqlError> errors, string sql, Func<string?[], T> converter) where T : DatabaseSchemaObject
    {
        var tbl = this.QueryStrings(sql, out var exception);
        if (exception != null)
        {
            errors.Add(new(sql, exception));
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
                errors.Add(new(sql, e));
            }

            if (so != null) yield return so;
        }
    }

    private IEnumerable<T> GetSchemaObjects<T>(Func<List<SqlError>, GetSchemaInfoFilter, IEnumerable<T>> func, params GetSchemaInfoFilter[] filters) where T : DatabaseSchemaObject
    {
        var items = new HashSet<T>();
        var errors = new List<SqlError>();
        foreach (var filter in filters)
        {
            if (filter.Database == null && filter.Schema == null && filter.Table == null) throw new NotImplementedException();
            if (filter.Database != null && IsExcluded(filter.Database)) continue;
            if (filter.Schema != null && IsExcluded(filter.Schema)) continue;
            if (filter.Table != null && IsExcluded(filter.Table)) continue;

            foreach (var obj in func(errors, filter))
            {
                if (IsExcluded(obj)) continue;
                if (!items.Add(obj)) continue;
                yield return obj;
            }
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }

    #endregion Helpers

    private readonly Lzy<string?> currentDatabaseName;
    //public bool IsCachedCurrentDatabaseName { get => currentDatabaseName.IsEnabled; set => currentDatabaseName.IsEnabled = value; }
    public string? CurrentDatabaseName => currentDatabaseName.Value;
    protected abstract string? GetCurrentDatabaseName();

    private readonly Lzy<string?> currentSchemaName;
    //public bool IsCachedCurrentSchemaName { get => currentSchemaName.IsEnabled; set => currentSchemaName.IsEnabled = value; }
    public string? CurrentSchemaName => currentSchemaName.Value;
    protected abstract string? GetCurrentSchemaName();

    #region GetDatabases

    public virtual IEnumerable<DatabaseSchemaDatabase> GetDatabases()
    {
        var errors = new List<SqlError>();
        foreach (var soDatabase in GetDatabases(errors))
        {
            if (IsExcluded(soDatabase)) continue;
            yield return soDatabase;
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }
    protected abstract IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors);

    #endregion GetDatabases

    #region GetSchemas

    public virtual IEnumerable<DatabaseSchemaSchema> GetSchemas() => GetSchemaObjects(GetSchemas, GetDatabases().Select(o => new GetSchemaInfoFilter(this, o)).ToArray());
    public virtual IEnumerable<DatabaseSchemaSchema> GetSchemas(DatabaseSchemaDatabase database) => GetSchemaObjects(GetSchemas, new GetSchemaInfoFilter(this, database));

    protected abstract IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter);

    #endregion GetSchemas

    #region GetTables

    public virtual IEnumerable<DatabaseSchemaTable> GetTables() => GetSchemaObjects(GetTables, GetDatabases().Select(o => new GetSchemaInfoFilter(this, o)).ToArray());
    public virtual IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaDatabase database) => GetSchemaObjects(GetTables, new GetSchemaInfoFilter(this, database));
    public virtual IEnumerable<DatabaseSchemaTable> GetTables(DatabaseSchemaSchema schema) => GetSchemaObjects(GetTables, new GetSchemaInfoFilter(this, null, schema));

    protected abstract IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter);

    #endregion GetTables

    #region GetTableColumns

    public virtual IEnumerable<DatabaseSchemaTableColumn> GetTableColumns() => GetSchemaObjects(GetTableColumns, GetDatabases().Select(o => new GetSchemaInfoFilter(this, o)).ToArray());
    public virtual IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaDatabase database) => GetSchemaObjects(GetTableColumns, new GetSchemaInfoFilter(this, database));
    public virtual IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaSchema schema) => GetSchemaObjects(GetTableColumns, new GetSchemaInfoFilter(this, null, schema));
    public virtual IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(DatabaseSchemaTable table) => GetSchemaObjects(GetTableColumns, new GetSchemaInfoFilter(this, null, null, table));

    protected abstract IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter);

    #endregion GetTableColumns

    public virtual bool GetTableExists(DatabaseSchemaTable table) =>
        GetTables(table.Schema)
            .FirstOrDefault(o => StringComparer.OrdinalIgnoreCase.Equals(o.TableName, table.TableName)) != null;

    public abstract bool DropTable(DatabaseSchemaTable table);

    #endregion Schema

    #region Helpers

    protected record SqlError(string Sql, Exception Error);

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

        return new(sb.ToString(), valuesArray.Select(o => o.Error));
    }


    protected string EscapeColumns(Dictionary<int, string> columns, bool skipEscaping = false) => EscapeColumns(null, columns, skipEscaping);
    protected virtual string EscapeColumns(string? tableAlias, Dictionary<int, string> columns, bool skipEscaping = false)
    {
        if (string.IsNullOrEmpty(tableAlias)) tableAlias = string.Empty;
        else if (!tableAlias.EndsWith(".")) tableAlias += ".";

        return columns.OrderBy(kvp => kvp.Key)
            .Select(o => tableAlias + (skipEscaping ? o.Value : Escape(o.Value)))
            .ToStringDelimited(",");
    }
    
    public virtual DatabaseSchemaTable CreateDatabaseSchemaTable(string? database, string? schema, string table) => new(
        database ?? CurrentDatabaseName ?? throw new ArgumentException("No database name provided and could not determine current database name"),
        schema ?? CurrentSchemaName,
        table
    );

    #region CreateCommand

    public IDbCommand CreateCommand() => CreateCommand(string.Empty, Array.Empty<DatabaseParameter>()).Command;

    protected virtual (IDbCommand Command, IDbDataParameter[] Parameters) CreateCommand<TParameter>(string sql, IReadOnlyList<TParameter> parameters)
        where TParameter : DatabaseParameter
    {
        var command = Connection.CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = sql;
        command.CommandTimeout = CommandTimeout;
        if (parameters.Count == 0) return (command, Array.Empty<IDbDataParameter>());

        var ps = new List<IDbDataParameter>();
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.DbType = parameter.Type;
            if (parameter is DatabaseParameterValue parameterValue) p.Value = parameterValue.Value;
            command.Parameters.Add(p);
            ps.Add(p);
        }

        return (command, ps.ToArray());
    }

    #endregion CreateCommand

    #region Execute

    public virtual object? QueryScalar(string sql, params DatabaseParameterValue[] values)
    {
        log.LogTrace(nameof(QueryScalar) + ": {Sql}", sql);
        var tpl = CreateCommand(sql, values);
        using var command = tpl.Command;
        var result = command.ExecuteScalar();
        return result == DBNull.Value ? null : result;
    }

    public virtual DataReaderResult? Query(string sql, params DatabaseParameterValue[] values) => QueryMultiple(sql, values).FirstOrDefault();

    public DataReaderResult? Query(string sql, out Exception? exception, params DatabaseParameterValue[] values)
    {
        try
        {
            exception = null;
            return Query(sql, values);
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return null;
        }
    }

    public virtual List<DataReaderResult> QueryMultiple(string sql, params DatabaseParameterValue[] values)
    {
        log.LogTrace(nameof(QueryMultiple) + ": {Sql}", sql);
        var tpl = CreateCommand(sql, values);
        using var command = tpl.Command;
        return command.ExecuteReaderResults().ToList();
    }

    public virtual int NonQuery(string sql, params DatabaseParameterValue[] values)
    {
        log.LogTrace(nameof(NonQuery) + ": {Sql}", sql);
        var tpl = CreateCommand(sql, values);
        using var command = tpl.Item1;
        return command.ExecuteNonQuery();
    }

    public int NonQuery(string sql, out Exception? exception, params DatabaseParameterValue[] values)
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


    public List<object?[]> QueryObjects(string sql, params DatabaseParameterValue[] values)
    {
        var result = Query(sql, values);
        var list = new List<object?[]>();
        if (result != null)
        {
            list = result.Rows.Select<DataReaderResultRow, object?[]>(row => row.ToArray()).ToList();
        }

        return list;
    }
    public List<object?[]> QueryObjects(string sql, out Exception? exception, params DatabaseParameterValue[] values)
    {
        try
        {
            exception = null;
            return QueryObjects(sql, values);
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return new();
        }
    }

    #endregion Execute

    #endregion Helpers

    #region Insert

    public virtual int Insert(DatabaseSchemaTable table, IReadOnlyList<(string ColumnName, DbType? ColumnType)> columns, IEnumerable<object?[]> values)
    {
        // https://stackoverflow.com/questions/2972974/how-should-i-multiple-insert-multiple-records

        var databaseParameters = new List<DatabaseParameter>();
        var columnNames = new List<string>();
        var parameterNames = new List<string>();

        for (var i = 0; i < columns.Count; i++)
        {
            var (columnName, columnType) = columns[i];
            columnName = Escape(columnName);
            var p = new DatabaseParameter(GenerateParameterName(i), columnType ?? DbType.String);
            databaseParameters.Add(p);
            parameterNames.Add(p.Name);
            columnNames.Add(columnName);
        }

        var sql = $"INSERT INTO {Escape(table)} ({columnNames}) VALUES ({parameterNames})";
        //sql += ";"; // breaks Oracle

        var cp = CreateCommand(sql, databaseParameters);
        using var command = cp.Command;
        var parameters = cp.Parameters;

        var resultCount = 0;
        foreach (var row in values)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = (IDbDataParameter?)command.Parameters[i];
                if (p == null) throw new NullReferenceException($"Parameter {parameters[i].ParameterName} does not exist on command"); // should not happen
                p.Value = row[i] ?? DBNull.Value;
            }

            resultCount += command.ExecuteNonQuery();
        }

        return resultCount;
    }

    #endregion Insert

    #region DatabaseParameter

    private volatile int parameterCounter;

    public virtual DatabaseParameter NextParameter(DbType type) => new(GenerateParameterName(Interlocked.Increment(ref parameterCounter)), type);
    public virtual DatabaseParameterValue NextParameter(object? value, DbType type) => new(GenerateParameterName(Interlocked.Increment(ref parameterCounter)), type, value);

    #endregion DatabaseParameter
}
