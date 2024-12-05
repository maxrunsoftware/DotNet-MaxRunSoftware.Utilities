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

using Oracle.ManagedDataAccess.Client;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable RedundantStringInterpolation
public class OracleSql : Sql
{
    public static OracleConnection CreateConnection(string connectionString) => new(connectionString);

    public OracleSql(string connectionString, ILoggerFactory loggerProvider) : this(CreateConnection(connectionString), loggerProvider) { }
    public OracleSql(IDbConnection connection, ILoggerFactory loggerProvider) : base(connection, loggerProvider)
    {
        DefaultDataTypeString = DatabaseTypes.Get(OracleSqlType.NClob).TypeName;
        DefaultDataTypeInteger = DatabaseTypes.Get(OracleSqlType.Int32).TypeName;
        DefaultDataTypeDateTime = DatabaseTypes.Get(OracleSqlType.DateTime).TypeName;
        DialectEscapeLeft = '"';
        DialectEscapeRight = '"';

        // https://docs.oracle.com/cd/B19306_01/server.102/b14200/sql_elements008.htm
        IdentifierCharactersValid.AddRange((Constant.Chars_Alphanumeric_String + "$_#").ToCharArray());

        // https://docs.oracle.com/cd/B19306_01/em.102/b40103/app_oracle_reserved_words.htm
        ReservedWords.AddRange(ReservedWordsParse(OracleSqlReservedWords.WORDS));
    }

    public override string GenerateParameterName(int parameterIndex) => ":p" + parameterIndex; // https://stackoverflow.com/questions/22694334/ora-00936-missing-expression

    public override string Escape(string objectToEscape) => base.Escape(objectToEscape).ToUpperInvariant();

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.OracleSql;
    protected override Type DatabaseTypesEnum => typeof(OracleSqlType);

    public OracleSqlServerProperties GetServerProperties() => new(this);

    private string? GetCurrentDatabaseName(List<SqlError> errors)
    {
        // ReSharper disable StringLiteralTypo
        string[] sqls =
        {
            "select SYS_CONTEXT('USERENV','DB_NAME') from dual",
            "select global_name from global_name",
            "select ora_database_name from dual",
            "select pdb_name FROM DBA_PDBS",
            "select name from v$database",
        };
        // ReSharper restore StringLiteralTypo

        var errors2 = new List<SqlError>();
        foreach (var sql in sqls)
        {
            var result = this.QueryStrings(sql, out var exception);
            if (exception != null)
            {
                errors2.Add(new(sql, exception));
                continue;
            }

            foreach (var row in result)
            {
                var name = row[0];
                if (name == null) continue;
                if (name.TrimOrNull() == null) continue;
                return name;
            }
        }

        errors.AddRange(errors2);

        return null;
    }
    protected override string? GetCurrentDatabaseName()
    {
        var errors = new List<SqlError>();
        var name = GetCurrentDatabaseName(errors);
        if (name != null) return name;

        if (errors.IsEmpty())
        {
            try
            {
                throw CreateExceptionInSqlStatements(errors);
            }
            catch (Exception e)
            {
                log.LogDebug(e, "Could not determine current database name");
            }
        }

        return null;
    }
    private string? GetCurrentSchemaName(List<SqlError> errors)
    {
        // ReSharper disable StringLiteralTypo
        string[] sqls =
        {
            "select SYS_CONTEXT('USERENV','CURRENT_SCHEMA') from dual",
            "select user from dual",
            "select SYS_CONTEXT('USERENV','SESSION_USER') from dual",
        };
        // ReSharper restore StringLiteralTypo

        var errors2 = new List<SqlError>();
        foreach (var sql in sqls)
        {
            var result = this.QueryStrings(sql, out var exception);
            if (exception != null)
            {
                errors2.Add(new(sql, exception));
                continue;
            }

            foreach (var row in result)
            {
                var name = row[0];
                if (name == null) continue;
                if (name.TrimOrNull() == null) continue;
                return name;
            }
        }

        errors.AddRange(errors2);

        return null;
    }
    protected override string? GetCurrentSchemaName()
    {
        var errors = new List<SqlError>();
        var name = GetCurrentSchemaName(errors);
        if (name != null) return name;

        if (errors.IsEmpty())
        {
            try
            {
                throw CreateExceptionInSqlStatements(errors);
            }
            catch (Exception e)
            {
                log.LogDebug(e, "Could not determine current schema name");
            }
        }

        return null;
    }

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var currentDatabaseName = GetCurrentDatabaseName(errors);
        if (currentDatabaseName != null) yield return new(currentDatabaseName);
    }

    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        // TODO: whole method is expensive operation

        var currentDatabaseName = GetCurrentDatabaseName(errors);
        if (currentDatabaseName == null) yield break;

        var sqls = new[]
        {
            "SELECT DISTINCT username FROM dba_users",
            "SELECT DISTINCT username FROM all_users",
            "SELECT DISTINCT owner FROM dba_objects",
            "SELECT DISTINCT owner FROM dba_segments",
            "SELECT DISTINCT OWNER FROM dba_tables",
            "SELECT DISTINCT OWNER FROM all_tables",
        };

        var errors2 = new List<SqlError>();
        var foundOne = false;
        foreach (var sql in sqls)
        {
            var enumerable = GetSchemaObjects(errors2, sql, row => new DatabaseSchemaSchema(
                currentDatabaseName,
                row[0]
            )).Where(o => o.SchemaName != null);
            foreach (var obj in enumerable)
            {
                foundOne = true;
                yield return obj;
            }
        }

        if (!foundOne) errors.AddRange(errors2); // Only throw errors if we didn't find anything
    }

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        // TODO: whole method is expensive operation

        var currentDatabaseName = GetCurrentDatabaseName(errors);
        if (currentDatabaseName == null) yield break;

        var currentSchemaName = GetCurrentSchemaName();

        var sqls = new[]
        {
            "SELECT DISTINCT OWNER,TABLE_NAME FROM dba_tables",
            "SELECT DISTINCT OWNER,TABLE_NAME FROM all_tables",
            "SELECT DISTINCT NULL AS OWNER,TABLE_NAME FROM user_tables",
        };

        var errors2 = new List<SqlError>();
        var foundOne = false;
        foreach (var sql in sqls)
        {
            var enumerable = GetSchemaObjects(errors2, sql, row => new DatabaseSchemaTable(
                currentDatabaseName,
                row[0] ?? currentSchemaName,
                row[1].CheckNotNull("TABLE_NAME")
            ));
            foreach (var obj in enumerable)
            {
                foundOne = true;
                yield return obj;
            }
        }

        if (!foundOne) errors.AddRange(errors2); // Only throw errors if we didn't find anything
    }


    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        // TODO: whole method is expensive operation

        var currentDatabaseName = GetCurrentDatabaseName(errors);
        if (currentDatabaseName == null) yield break;

        var currentSchemaName = GetCurrentSchemaName();


        var colsDict = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "OWNER",
            [1] = "TABLE_NAME",
            [2] = "COLUMN_NAME",
            [3] = "DATA_TYPE",
            [4] = "DATA_LENGTH",
            [5] = "DATA_PRECISION",
            [6] = "DATA_SCALE",
            [7] = "NULLABLE",
            [8] = "COLUMN_ID",
            [9] = "DATA_DEFAULT",
            [10] = "CHAR_LENGTH",
        };

        var sqls = new[]
        {
            "SELECT " + colsDict.OrderBy(kvp => kvp.Key).Select(o => o.Value).ToStringDelimited(",") + " FROM dba_tab_columns",
            "SELECT " + colsDict.OrderBy(kvp => kvp.Key).Select(o => o.Value).ToStringDelimited(",") + " FROM all_tab_columns",
            "SELECT NULL AS OWNER," + colsDict.OrderBy(kvp => kvp.Key).Skip(1).Select(o => o.Value).ToStringDelimited(",") + " FROM user_tab_columns",
        };

        DatabaseSchemaTableColumn Parse(string?[] row)
        {
            // ReSharper disable InconsistentNaming
            var CHAR_LENGTH = row[10].TrimOrNull().ToIntNullable() ?? 0;
            var DATA_LENGTH = row[4].TrimOrNull().ToIntNullable() ?? 0;
            // ReSharper restore InconsistentNaming

            var table = new DatabaseSchemaTable(
                currentDatabaseName,
                row[0].TrimOrNull() != null ? row[0] : currentSchemaName,
                row[1].CheckNotNull(colsDict[1])
            );

            var col = new DatabaseSchemaColumn(
                row[2].CheckNotNullTrimmed(colsDict[2]),
                row[3].CheckNotNullTrimmed(colsDict[3]),
                GetDbType(row[3].CheckNotNullTrimmed(colsDict[3])) ?? DbType.String,
                row[7]!.ToBool(),
                row[8]!.ToInt(),
                CHAR_LENGTH > 0 ? CHAR_LENGTH : DATA_LENGTH,
                row[5].ToIntNullable(),
                row[6].ToIntNullable(),
                StringComparer.OrdinalIgnoreCase.Equals(row[9].TrimOrNull(), "null") ? null : row[9]
            );

            return new(table, col);
        }

        var errors2 = new List<SqlError>();
        var foundOne = false;
        foreach (var sql in sqls)
        {
            var enumerable = GetSchemaObjects(errors2, sql, Parse);
            foreach (var obj in enumerable)
            {
                foundOne = true;
                yield return obj;
            }
        }

        if (!foundOne) errors.AddRange(errors2); // Only throw errors if we didn't find anything
    }

    public override bool GetTableExists(DatabaseSchemaTable table)
    {
        var ps = this.NextParameter(table);
        var values = new List<DatabaseParameterValue> { ps.Table };
        var sql = $"SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME={ps.Table.Name}";

        if (table.Schema.SchemaName != null)
        {
            values.Add(ps.Schema);
            sql = $"SELECT COUNT(*) FROM ALL_TABLES WHERE TABLE_NAME={ps.Table.Name} AND OWNER={ps.Schema.Name}";
        }

        return this.QueryScalarInt(sql, values.ToArray()).CheckNotNull("COUNT") > 0;
    }

    public override bool DropTable(DatabaseSchemaTable table) => DropTable(table, false, false);

    public virtual bool DropTable(DatabaseSchemaTable table, bool cascadeConstraints, bool purge)
    {
        if (!GetTableExists(table)) return false;
        var sql = new StringBuilder();
        sql.Append($"DROP TABLE {Escape(table)}");
        if (cascadeConstraints) sql.Append(" CASCADE CONSTRAINTS");
        if (purge) sql.Append(" PURGE");
        NonQuery(sql.ToString());
        return true;
    }
}
