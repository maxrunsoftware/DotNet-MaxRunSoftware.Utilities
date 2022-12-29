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

using System.Diagnostics.CodeAnalysis;

namespace MaxRunSoftware.Utilities.Database;

public class OracleSql : SqlBase
{
    public OracleSql(IDbConnection connection) : base(connection) { }

    public static DatabaseDialectSettings DialectSettingsDefaultInstance { get; set; } = new DatabaseDialectSettings
        {
            DefaultDataTypeString = DatabaseTypes.Get(OracleSqlType.NClob).TypeName,
            DefaultDataTypeInteger = DatabaseTypes.Get(OracleSqlType.Int32).TypeName,
            DefaultDataTypeDateTime = DatabaseTypes.Get(OracleSqlType.DateTime).TypeName,
            EscapeLeft = '"',
            EscapeRight = '"',
        }
        // https://docs.oracle.com/cd/B19306_01/server.102/b14200/sql_elements008.htm
        .AddIdentifierCharactersValid((Constant.Chars_Alphanumeric_String + "$_#").ToCharArray())
        // https://docs.oracle.com/cd/B19306_01/em.102/b40103/app_oracle_reserved_words.htm
        .AddReservedWords(OracleSqlReservedWords.WORDS);

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.OracleSql;
    protected override Type DatabaseTypesEnum => typeof(OracleSqlType);
    public override DatabaseDialectSettings DialectSettingsDefault => DialectSettingsDefaultInstance;

    public bool DropTableCascadeConstraints { get; set; }
    public bool DropTablePurge { get; set; }


    private Tuple<string?, List<SqlError>> GetCurrentName(string[] sqls, string objectType)
    {
        var errors = new List<SqlError>();


        foreach (var sql in sqls)
        {
            var tbl = this.QueryStrings(sql, out var exception);
            if (exception != null)
            {
                errors.Add(new SqlError(sql, exception));
                continue;
            }

            var s = tbl.Select(row => row[0]).TrimOrNull().WhereNotNull().FirstOrDefault();
            if (s == null) continue;
            currentDatabaseCached = new DatabaseSchemaDatabase(s);
            return new Tuple<string?, List<SqlError>>(s, new List<SqlError>());
        }

        if (errors.IsEmpty())
        {
            try
            {
                throw new NullReferenceException($"Could not determine current {objectType} name");
            }
            catch (Exception e)
            {
                errors.Add(new SqlError(sqls.ToStringDelimited("; ") + ";", e));
            }
        }

        return new Tuple<string?, List<SqlError>>(null, errors);
    }

    private DatabaseSchemaDatabase? currentDatabaseCached;
    private bool GetCurrentDatabase(List<SqlError> errors, [MaybeNullWhen(false)] out DatabaseSchemaDatabase currentDatabase)
    {
        if (currentDatabaseCached != null)
        {
            currentDatabase = currentDatabaseCached;
            return true;
        }

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

        var result = GetCurrentName(sqls, "database");
        if (result.Item1 != null)
        {
            currentDatabaseCached = new DatabaseSchemaDatabase(result.Item1);
            currentDatabase = currentDatabaseCached;
            return true;
        }

        errors.AddRange(result.Item2);
        currentDatabase = null;
        return false;
    }


    public override DatabaseSchemaDatabase GetCurrentDatabase()
    {
        if (currentDatabaseCached != null) return currentDatabaseCached;
        var errors = new List<SqlError>();
        if (GetCurrentDatabase(errors, out var o)) return o;
        throw CreateExceptionInSqlStatements(errors);
    }

    private bool GetCurrentSchema(List<SqlError> errors, [MaybeNullWhen(false)] out DatabaseSchemaSchema currentSchema)
    {
        if (!GetCurrentDatabase(errors, out var currentDatabase))
        {
            currentSchema = null;
            return false;
        }

        // ReSharper disable StringLiteralTypo
        string[] sqls =
        {
            "select SYS_CONTEXT('USERENV','CURRENT_SCHEMA') from dual",
            "select user from dual",
            "select SYS_CONTEXT('USERENV','SESSION_USER') from dual",
        };
        // ReSharper restore StringLiteralTypo

        var result = GetCurrentName(sqls, "schema");
        if (result.Item1 != null)
        {
            currentSchema = new DatabaseSchemaSchema(currentDatabase, result.Item1);
            return true;
        }

        errors.AddRange(result.Item2);
        currentSchema = null;
        return false;
    }

    public override DatabaseSchemaSchema GetCurrentSchema()
    {
        var errors = new List<SqlError>();
        if (GetCurrentSchema(errors, out var o)) return o;
        throw CreateExceptionInSqlStatements(errors);
    }



    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        if (GetCurrentDatabase(errors, out var currentDatabase))
        {
            yield return currentDatabase;
        }
    }

    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, DatabaseSchemaDatabase database)
    {
        // TODO: whole method is expensive operation

        if (!GetCurrentDatabase(errors, out var currentDatabase)) yield break;

        var sqls = new[]
        {
            new[] { "SELECT DISTINCT username FROM dba_users", "username" },
            new[] { "SELECT DISTINCT username FROM all_users", "username" },
            new[] { "SELECT DISTINCT owner FROM dba_objects", "owner" },
            new[] { "SELECT DISTINCT owner FROM dba_segments", "owner" },
            new[] { "SELECT DISTINCT OWNER FROM dba_tables", "OWNER" },
            new[] { "SELECT DISTINCT OWNER FROM all_tables", "OWNER" },
        };

        static DatabaseSchemaSchema Parse(DatabaseSchemaDatabase database, string?[] row, string columnName)
        {

            return new DatabaseSchemaSchema(
                database: database,
                schemaName: row[0].CheckNotNullTrimmed(columnName)
            );
        }

        var objsFound = new HashSet<DatabaseSchemaSchema>();
        foreach (var sql in sqls)
        {
            foreach (var obj in GetSchemaObjects(errors, sql[0], row => Parse(currentDatabase, row, sql[1])))
            {
                if (objsFound.Add(obj)) yield return obj;
            }
        }


    }

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetTablesFilter filter)
    {
        // TODO: whole method is expensive operation

        if (!GetCurrentSchema(errors, out var currentSchema)) yield break;

        var sqls = new[]
        {
            "SELECT DISTINCT OWNER,TABLE_NAME FROM dba_tables",
            "SELECT DISTINCT OWNER,TABLE_NAME FROM all_tables",
            "SELECT DISTINCT NULL AS OWNER,TABLE_NAME FROM user_tables"
        };

        DatabaseSchemaTable Parse(string?[] row)
        {
            var schemaName = row[0].TrimOrNull() ?? currentSchema.SchemaName;
            var tableName = row[1].CheckNotNullTrimmed("TABLE_NAME");
            return new DatabaseSchemaTable(
                schema: new DatabaseSchemaSchema(currentSchema.Database, schemaName),
                tableName: tableName
            );
        }


        var objsFound = new HashSet<DatabaseSchemaTable>();
        foreach (var sql in sqls)
        {
            foreach (var obj in GetSchemaObjects(errors, sql, Parse))
            {
                if (objsFound.Add(obj)) yield return obj;
            }
        }

    }


    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetTableColumnsFilter filter)
    {
        // TODO: whole method is expensive operation

        if (!GetCurrentSchema(errors, out var currentSchema)) yield break;

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
            "SELECT " + colsDict.OrderBy(kvp => kvp.Key).ToStringDelimited(",") + " FROM dba_tab_columns",
            "SELECT " + colsDict.OrderBy(kvp => kvp.Key).ToStringDelimited(",") + " FROM all_tab_columns",
            "SELECT NULL AS OWNER," + colsDict.OrderBy(kvp => kvp.Key).Skip(1).ToStringDelimited(",") + " FROM user_tab_columns"
        };

        DatabaseSchemaTableColumn Parse(string?[] row)
        {
            // ReSharper disable InconsistentNaming
            var CHAR_LENGTH = row[10].TrimOrNull().ToIntNullable() ?? 0;
            var DATA_LENGTH = row[4].TrimOrNull().ToIntNullable() ?? 0;
            // ReSharper restore InconsistentNaming

            return new DatabaseSchemaTableColumn(
                table: new DatabaseSchemaTable(
                    databaseName: currentSchema.Database.DatabaseName,
                    schemaName: row[0].TrimOrNull() ?? currentSchema.SchemaName,
                    tableName: row[1].CheckNotNullTrimmed(colsDict[1])
                ),
                columnName: row[2].CheckNotNullTrimmed(colsDict[2]),
                columnType: row[3].CheckNotNullTrimmed(colsDict[3]),
                columnDbType: GetDbType(row[3].CheckNotNullTrimmed(colsDict[3])) ?? DbType.String,

                isNullable: row[7]!.ToBool(),
                ordinal: row[8]!.ToInt(),

                characterLengthMax: CHAR_LENGTH > 0 ? CHAR_LENGTH : DATA_LENGTH,
                numericPrecision: row[5].ToIntNullable(),
                numericScale: row[6].ToIntNullable(),
                columnDefault: StringComparer.OrdinalIgnoreCase.Equals(row[9].TrimOrNull(), "null") ? null : row[9]
            );
        }


        var objsFound = new HashSet<DatabaseSchemaTableColumn>();
        foreach (var sql in sqls)
        {
            foreach (var obj in GetSchemaObjects(errors, sql, Parse))
            {
                if (objsFound.Add(obj)) yield return obj;
            }
        }
    }

    public override bool DropTable(DatabaseSchemaTable table)
        {
            if (!GetTableExists(table)) return false;

            var dst = Escape(table.Schema.SchemaName, table.TableName);
            var sql = new StringBuilder();
            sql.Append($" DROP TABLE {dst}");
            if (DropTableCascadeConstraints) sql.Append(" CASCADE CONSTRAINTS");
            if (DropTablePurge) sql.Append(" PURGE");

            using (var cmd = CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql.ToString();
                cmd.ExecuteNonQuery();
            }

            return true;
        }


}
