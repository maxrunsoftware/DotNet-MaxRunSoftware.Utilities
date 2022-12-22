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

using MySql.Data.MySqlClient.X.XDevAPI.Common;

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

// ReSharper disable PropertyCanBeMadeInitOnly.Global
public abstract class Sql : IDisposable
{
    private readonly ILogger log = Constant.GetLogger<Sql>();
    public virtual IDbConnection Connection { get; }

    protected Sql(IDbConnection connection)
    {
        Connection = connection;
    }

    public abstract Type DatabaseTypeEnum { get; }
    public bool ExceptionShowFullSql { get; set; }
    public virtual IDbCommand CreateCommand()
    {
        var command = Connection.CreateCommand();
        command.CommandTimeout = CommandTimeout;
        return command;
    }



    public virtual void Dispose() => Connection.Dispose();

    #region SqlDialectSettings

    public abstract SqlDialectSettings DialectSettingsDefault { get; }

    private SqlDialectSettings? dialectSettings;
    public SqlDialectSettings DialectSettings
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

    public abstract string? GetCurrentDatabaseName();
    public abstract string? GetCurrentSchemaName();
    public abstract IEnumerable<SqlSchemaDatabase> GetDatabases();
    public abstract IEnumerable<SqlSchemaSchema> GetSchemas(string? database);
    public abstract IEnumerable<SqlSchemaTable> GetTables(string? database, string? schema);
    public abstract IEnumerable<SqlSchemaTableColumn> GetTableColumns(string? database, string? schema, string? table);

    public virtual bool GetTableExists(string? database, string? schema, string table) => GetTables(
        database: database,
        schema: schema
    ).Any(o => o.TableName.EqualsOrdinalIgnoreCase(Unescape(table)));

    public abstract bool DropTable(string? database, string? schema, string table);

    #endregion Schema

    #region Helpers

    protected virtual AggregateException CreateExceptionInSqlStatements(IEnumerable<(string SqlStatement, Exception Exception)> values)
    {
        var valuesArray = values.ToArray();
        var sb = new StringBuilder();
        sb.Append($"Error executing {valuesArray.Length} SQL queries");
        for (var i = 0; i < valuesArray.Length; i++)
        {
            var iStr = (i + 1).ToString().PadLeft(valuesArray.Length.ToString().Length);
            var eName = valuesArray[i].Exception.GetType().NameFormatted();
            var eMsg = valuesArray[i].Exception.Message;
            var ss = valuesArray[i].SqlStatement;
            sb.Append($"{Constant.NewLine}  [{iStr}] {eName}: {eMsg} --> {ss}");
        }
        return new AggregateException(sb.ToString(), valuesArray.Select(o => o.Exception));
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

    protected virtual bool IsExcludedDatabase(string databaseName) => DialectSettings.ExcludedDatabases.Contains(databaseName);

    protected virtual bool IsExcludedSchema(string schemaName) => DialectSettings.ExcludedSchemas.Contains(schemaName);

    public virtual string[] GenerateParameterNames(int count) => Enumerable.Range(0, count).Select(GenerateParameterName).ToArray();

    public virtual string GenerateParameterName(int index) => "@p" + index;

    protected virtual string? ConvertToString(object? obj) => obj.ToStringGuessFormat();

    #region CreateCommand

    protected virtual Tuple<IDbCommand, IDbDataParameter[]> CreateCommand(string sql, DbParameter[] parameters)
    {
        var command = CreateCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = sql;
        var ps = new List<IDbDataParameter>();
        for (var i = 0; i < parameters.Length; i++)
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

    public virtual string? QueryScalarString(string sql, params DbParameterValue[] values) => ConvertToString(QueryScalar(sql, values));

    public virtual  List<object?[]> QueryTable(string sql, params DbParameterValue[] values)
    {
        log.LogTrace(nameof(QueryTable) + ": {Sql}", sql);

        var tpl = CreateCommandWithValues(sql, values);
        using var command = tpl.Item1;
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        var result = command.ExecuteReaderResult();
        return result == null ? new List<object?[]>() : new List<object?[]>(result.Rows.Select(row => row.ToArray()));
    }

    public virtual List<object?[]> QueryTable(string sql, out Exception? exception, params DbParameterValue[] values)
    {
        try
        {
            exception = null;
            return QueryTable(sql, values);
        }
        catch (Exception e)
        {
            log.LogDebug(e, "Error Executing SQL: {Sql}", sql);
            exception = e;
            return new List<object?[]>();
        }
    }

    public virtual  List<string?[]> QueryTableStrings(string sql, params DbParameterValue[] values) =>
        QueryTable(sql, values).Select(o => o.Select(ConvertToString).ToArray()).ToList();

    public virtual  List<string?[]> QueryTableStrings(string sql, out Exception? exception, params DbParameterValue[] values) =>
        QueryTable(sql, out exception, values).Select(o => o.Select(ConvertToString).ToArray()).ToList();

    public virtual  List<object?> QueryTableColumn(string sql, int columnIndex, params DbParameterValue[] values) =>
        QueryTable(sql, values).Select(o => o[columnIndex]).ToList();

    public virtual List<object?> QueryTableColumn(string sql, int columnIndex, out Exception? exception, params DbParameterValue[] values) =>
        QueryTable(sql, out exception, values).Select(o => o[columnIndex]).ToList();

    public virtual  List<string?> QueryTableColumnStrings(string sql, int columnIndex, params DbParameterValue[] values) =>
        QueryTableColumn(sql, columnIndex, values).Select(ConvertToString).ToList();

    public virtual  List<string?> QueryTableColumnStrings(string sql, int columnIndex, out Exception? exception, params DbParameterValue[] values) =>
        QueryTableColumn(sql, columnIndex, out exception, values).Select(ConvertToString).ToList();

    #endregion Query

    #region NonQuery
    public virtual int NonQuery(string sql, params DbParameterValue[] values)
    {
        log.LogTrace(nameof(NonQuery) + ": {Sql}", sql);

        var tpl = CreateCommandWithValues(sql, values);
        using var command = tpl.Item1;
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
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
        using var command = CreateCommand(sql);

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

    #region SqlDbType

    public SqlType? GetSqlDbType(string? rawSqlType)
    {
        rawSqlType = rawSqlType.TrimOrNull();
        if (rawSqlType == null) return null;

        var dbTypesEnumType = DatabaseTypeEnum;
        if (dbTypesEnumType == null) throw new NullReferenceException(GetType().FullNameFormatted() + "." + nameof(DatabaseTypeEnum) + " is null");
        dbTypesEnumType.CheckIsEnum(nameof(DatabaseTypeEnum));

        var item = SqlType.GetEnumItemBySqlType(dbTypesEnumType, rawSqlType);
        if (item == null) return null;

        if (!item.HasAttribute) throw MissingAttributeException.FieldMissingAttribute<SqlTypeAttribute>(dbTypesEnumType, rawSqlType);

        return item;
    }

    public IReadOnlyList<SqlType> GetSqlDbTypes()
    {
        var dbTypesEnumType = DatabaseTypeEnum;
        if (dbTypesEnumType == null) throw new NullReferenceException(GetType().FullNameFormatted() + "." + nameof(DatabaseTypeEnum) + " is null");

        dbTypesEnumType.CheckIsEnum(nameof(DatabaseTypeEnum));

        return SqlType.GetEnumItems(dbTypesEnumType);
    }

    #endregion SqlDbType

    #region Protected

    protected AggregateException CreateExceptionErrorInSqlStatements(IEnumerable<string> sqlStatements, IEnumerable<Exception> exceptions)
    {
        var sqlStatementsArray = sqlStatements.ToArray();
        return new AggregateException("Error executing " + sqlStatementsArray.Length + " SQL queries", exceptions);
    }

    #endregion Protected


}
