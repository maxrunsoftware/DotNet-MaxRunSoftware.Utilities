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

// ReSharper disable PropertyCanBeMadeInitOnly.Global

public abstract class Sql : IDisposable
{
    protected readonly ILogger log;
    private readonly SingleUse isDisposed = new();
    private IDbConnection? connection;
    public virtual IDbConnection Connection
    {
        get
        {
            var c = connection;
            if (isDisposed.IsUsed || c == null) throw new ObjectDisposedException(nameof(Connection));
            if (c.State == ConnectionState.Closed) c.Open();
            return c;
        }
    }

    protected Sql(IDbConnection connection)
    {
        log = Constant.GetLogger(GetType());
        this.connection = connection;
    }


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
            DatabaseNameEscaped = sql.DialectSettings.Escape(DatabaseName);
            DatabaseNameUnescaped = sql.DialectSettings.Unescape(DatabaseName);

            SchemaName =
                Table?.Schema.SchemaName
                ?? Schema?.SchemaName;
            SchemaNameEscaped = SchemaName == null ? null : sql.DialectSettings.Escape(SchemaName);
            SchemaNameUnescaped = SchemaName == null ? null : sql.DialectSettings.Unescape(SchemaName);

            TableName = Table?.TableName;
            TableNameEscaped = TableName == null ? null : sql.DialectSettings.Escape(TableName);
            TableNameUnescaped = TableName == null ? null : sql.DialectSettings.Unescape(TableName);
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

    private IEnumerable<T> GetSchemaObjects<T>(Func<List<SqlError>, GetSchemaInfoFilter, IEnumerable<T>> func, params GetSchemaInfoFilter[] filters) where T : DatabaseSchemaObject
    {
        var items = new HashSet<T>();
        var errors = new List<SqlError>();
        foreach (var filter in filters)
        {
            if (filter.Database == null && filter.Schema == null && filter.Table == null) throw new NotImplementedException();
            if (filter.Database != null && DialectSettings.IsExcluded(filter.Database)) continue;
            if (filter.Schema != null && DialectSettings.IsExcluded(filter.Schema)) continue;
            if (filter.Table != null && DialectSettings.IsExcluded(filter.Table)) continue;

            foreach (var obj in func(errors, filter))
            {
                if (DialectSettings.IsExcluded(obj)) continue;
                if (!items.Add(obj)) continue;
                yield return obj;
            }
        }

        if (errors.IsNotEmpty()) throw CreateExceptionInSqlStatements(errors);
    }

    #endregion Helpers

    public bool CacheCurrentDatabaseName { get; set; } = true;
    private bool currentDatabaseNameIsCached;
    private string? currentDatabaseNameCached;
    public string? CurrentDatabaseName
    {
        get
        {
            if (!CacheCurrentDatabaseName) return GetCurrentDatabaseName();
            if (currentDatabaseNameIsCached) return currentDatabaseNameCached;
            currentDatabaseNameCached = GetCurrentDatabaseName();
            currentDatabaseNameIsCached = true;
            return currentDatabaseNameCached;

        }
    }
    protected abstract string? GetCurrentDatabaseName();

    public bool CacheCurrentSchemaName { get; set; } = true;
    private bool currentSchemaNameIsCached;
    private string? currentSchemaNameCached;
    public string? CurrentSchemaName
    {
        get
        {
            if (!CacheCurrentSchemaName) return GetCurrentSchemaName();
            if (currentSchemaNameIsCached) return currentSchemaNameCached;
            currentSchemaNameCached = GetCurrentSchemaName();
            currentSchemaNameIsCached = true;
            return currentSchemaNameCached;

        }
    }
    protected abstract string? GetCurrentSchemaName();

    #region GetDatabases

    public virtual IEnumerable<DatabaseSchemaDatabase> GetDatabases()
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
            .FirstOrDefault(o => StringComparer.OrdinalIgnoreCase.Equals(o.TableName, table.TableName)) != null
        ;

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
        return new AggregateException(sb.ToString(), valuesArray.Select(o => o.Error));
    }




    protected string EscapeColumns(Dictionary<int, string> columns, bool skipEscaping = false) => EscapeColumns(null, columns, skipEscaping);
    protected virtual string EscapeColumns(string? tableAlias, Dictionary<int, string> columns, bool skipEscaping = false)
    {
        if (string.IsNullOrEmpty(tableAlias)) tableAlias = string.Empty;
        else if (!tableAlias.EndsWith(".")) tableAlias += ".";

        return columns.OrderBy(kvp => kvp.Key)
            .Select(o => tableAlias + (skipEscaping ? o.Value : DialectSettings.Escape(o.Value)))
            .ToStringDelimited(",");
    }

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

    public virtual DataReaderResult? Query(string sql, params DatabaseParameterValue[] values)
    {
        log.LogTrace(nameof(Query) + ": {Sql}", sql);
        var tpl = CreateCommand(sql, values);
        using var command = tpl.Command;
        return command.ExecuteReaderResult();
    }

    public virtual int NonQuery(string sql, params DatabaseParameterValue[] values)
    {
        log.LogTrace(nameof(NonQuery) + ": {Sql}", sql);
        var tpl = CreateCommand(sql, values);
        using var command = tpl.Item1;
        return command.ExecuteNonQuery();
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
            columnName = DialectSettings.Escape(columnName);
            var p = new DatabaseParameter(DialectSettings.GenerateParameterName(i), columnType ?? DbType.String);
            databaseParameters.Add(p);
            parameterNames.Add(p.Name);
            columnNames.Add(columnName);
        }

        var sql = $"INSERT INTO {DialectSettings.Escape(table)} ({columnNames}) VALUES ({parameterNames})";
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

    public virtual DatabaseParameter NextParameter(DbType type) => new(DialectSettings.GenerateParameterName(Interlocked.Increment(ref parameterCounter)), type);
    public virtual DatabaseParameterValue NextParameter(object? value, DbType type) => new(DialectSettings.GenerateParameterName(Interlocked.Increment(ref parameterCounter)), type, value);

    #endregion DatabaseParameter


}
