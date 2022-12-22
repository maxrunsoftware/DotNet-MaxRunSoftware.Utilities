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

public class MicrosoftSql : Sql
{
    public MicrosoftSql(IDbConnection connection) : base(connection) { }

    public static SqlDialectSettings DialectSettingsDefaultInstance { get; set; } = new SqlDialectSettings
        {
            DefaultDataTypeString = nameof(MicrosoftSqlType.NVarChar) + "(MAX)", // GetSqlDbType(SqlMsSqlType.NVarChar).SqlTypeName + "(MAX)";
            DefaultDataTypeInteger = nameof(MicrosoftSqlType.Int), // GetSqlDbType(SqlMsSqlType.Int).SqlTypeName;
            DefaultDataTypeDateTime = nameof(MicrosoftSqlType.DateTime), // GetSqlDbType(SqlMsSqlType.DateTime).SqlTypeName;
            EscapeLeft = '[',
            EscapeRight = ']',
        }.AddDatabaseUserExcluded("master", "model", "msdb", "tempdb")
        // https://docs.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers?view=sql-server-ver16
        .AddIdentifierCharactersValid((Constant.Chars_Alphanumeric_String + "@$#_").ToCharArray())
        // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
        .AddReservedWords(MicrosoftSqlReservedWords.WORDS);

    public override Type DatabaseTypeEnum => typeof(MicrosoftSqlType);
    public override SqlDialectSettings DialectSettingsDefault => DialectSettingsDefaultInstance;

    #region Schema

    private List<string> GetDatabaseNames(string? database)
    {
        database = database.TrimOrNull();
        if (database != null) return new List<string> { database };
        return GetDatabases().Select(o => o.DatabaseName).ToList();
    }

    public override string? GetCurrentDatabaseName() => QueryScalarString("SELECT DB_NAME();").TrimOrNull();

    public override string? GetCurrentSchemaName() => QueryScalarString("SELECT SCHEMA_NAME();").TrimOrNull();

    public override IEnumerable<SqlSchemaDatabase> GetDatabases()
    {
        var sql = new StringBuilder();
        sql.Append("SELECT DISTINCT [name] FROM sys.databases;");
        return QueryTableColumnStrings(sql.ToString(), 0)
                .WhereNotNull()
                .Select(o => new SqlSchemaDatabase(o))
                .Where(o => !IsExcludedDatabase(o.DatabaseName))
                .ToList()
            ;

    }

    public override IEnumerable<SqlSchemaSchema> GetSchemas(string? database)
    {
        var exceptions = new List<(string, Exception)>();
        foreach (var dbName in GetDatabaseNames(database))
        {
            var sql = new StringBuilder();
            sql.Append($" SELECT DISTINCT [CATALOG_NAME],[SCHEMA_NAME]");
            sql.Append($" FROM {Escape(dbName)}.INFORMATION_SCHEMA.SCHEMATA");
            if (database != null) sql.Append($" WHERE CATALOG_NAME='{Unescape(database)}'");
            sql.Append(';');

            var tbl = QueryTableStrings(sql.ToString(), out var exception);
            if (exception != null)
            {
                exceptions.Add((sql.ToString(), exception));
                continue;
            }

            foreach (var row in tbl)
            {
                SqlSchemaSchema? so = null;
                try
                {
                    so = new SqlSchemaSchema(databaseName: row[0], schemaName: row[1]);
                    if (IsExcludedDatabase(so.DatabaseName)) continue;
                    if (IsExcludedSchema(so.SchemaName)) continue;
                }
                catch (Exception e)
                {
                    exceptions.Add((sql.ToString(), e));
                }

                if (so == null) continue;
                yield return so;
            }
        }

        if (exceptions.IsNotEmpty()) throw CreateExceptionInSqlStatements(exceptions);
    }

    public override IEnumerable<SqlSchemaTable> GetTables(string? database, string? schema)
    {
        var exceptions = new List<(string, Exception)>();
        foreach (var dbName in GetDatabaseNames(database))
        {
            var sql = new StringBuilder();
            sql.Append($" SELECT DISTINCT [TABLE_CATALOG],[TABLE_SCHEMA],[TABLE_NAME]");
            sql.Append($" FROM {Escape(dbName)}.INFORMATION_SCHEMA.TABLES");
            sql.Append($" WHERE TABLE_TYPE='BASE TABLE'");
            if (database != null) sql.Append($" AND TABLE_CATALOG='{Unescape(database)}'");
            if (schema != null) sql.Append($" AND TABLE_SCHEMA='{Unescape(schema)}'");
            sql.Append(';');

            var tbl = QueryTableStrings(sql.ToString(), out var exception);
            if (exception != null)
            {
                exceptions.Add((sql.ToString(), exception));
                continue;
            }

            foreach (var row in tbl)
            {
                SqlSchemaTable? so = null;
                try
                {
                    so = new SqlSchemaTable(databaseName: row[0], schemaName: row[1], tableName: row[2]);
                    if (schema == null && IsExcludedSchema(so.SchemaName)) continue;
                    if (schema != null && !schema.EqualsIgnoreCase(so.SchemaName)) continue;
                }
                catch (Exception e)
                {
                    exceptions.Add((sql.ToString(), e));
                }

                if (so == null) continue;
                yield return so;
            }
        }

        if (exceptions.IsNotEmpty()) throw CreateExceptionInSqlStatements(exceptions);
    }

    public override IEnumerable<SqlSchemaTableColumn> GetTableColumns(string? database, string? schema, string? table)
    {
        var exceptions = new List<(string, Exception)>();
        foreach (var dbName in GetDatabaseNames(database))
        {
            var sql = new StringBuilder();
            var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
            {
                [0] = "TABLE_CATALOG",
                [1] = "TABLE_SCHEMA",
                [2] = "TABLE_NAME",
                [3] = "COLUMN_NAME",
                [4] = "DATA_TYPE",
                [5] = "IS_NULLABLE",
                [6] = "ORDINAL_POSITION",
                [7] = "CHARACTER_MAXIMUM_LENGTH",
                [8] = "NUMERIC_PRECISION",
                [9] = "NUMERIC_SCALE",
                [10] = "COLUMN_DEFAULT",
            }.OrderBy(kvp => kvp.Key).Select(o => $"c.{Escape(o.Value)}").ToStringDelimited(",");
            sql.Append($" SELECT DISTINCT {cols}");
            sql.Append($" FROM {Escape(dbName)}.INFORMATION_SCHEMA.COLUMNS c");
            sql.Append($" INNER JOIN {Escape(dbName)}.INFORMATION_SCHEMA.TABLES t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
            sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
            if (database != null) sql.Append($" AND c.TABLE_CATALOG='{Unescape(database)}'");
            if (schema != null) sql.Append($" AND c.TABLE_SCHEMA='{Unescape(schema)}'");
            if (table != null) sql.Append($" AND c.TABLE_NAME='{Unescape(table)}'");
            sql.Append(';');

            var tbl = QueryTableStrings(sql.ToString(), out var exception);
            if (exception != null)
            {
                exceptions.Add((sql.ToString(), exception));
                continue;
            }

            foreach (var row in tbl)
            {
                SqlSchemaTableColumn? so = null;
                try
                {
                    var columnType = row[4].TrimOrNull();

                    static T GetAttributeOfType<T>(Enum enumVal) where T : System.Attribute
                    {
                        var type = typeof(Micro);
                        var memInfo = type.GetMember(enumVal.ToString());
                        var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
                        return (attributes.Length > 0) ? (T)attributes[0] : null;
                    }

                    var dbTypeItem = GetSqlDbType(columnType);
                    var dbType = dbTypeItem?.DbType ?? DbType.String;
                    so = new SqlSchemaTableColumn(
                        databaseName: row[0],
                        schemaName: row[1],
                        tableName: row[2],
                        columnName: row[3],
                        columnType: columnType,
                        columnDbType: dbType,
                        isNullable: row[5]!.ToBool(),
                        ordinal: row[6]!.ToInt(),
                        characterLengthMax: row[7].ToLongNullable(),
                        numericPrecision: row[8].ToIntNullable(),
                        numericScale: row[9].ToIntNullable(),
                        columnDefault: row[10]
                    );

                    if (schema == null && IsExcludedSchema(so.SchemaName)) continue;
                    if (schema != null && !schema.EqualsIgnoreCase(so.SchemaName)) continue;
                    if (table != null && !table.EqualsIgnoreCase(so.TableName)) continue;
                }
                catch (Exception e)
                {
                    exceptions.Add((sql.ToString(), e));
                }

                if (so == null) continue;
                yield return so;
            }
        }

        if (exceptions.IsNotEmpty()) throw CreateExceptionInSqlStatements(exceptions);
    }


    public override bool GetTableExists(string? database, string? schema, string table)
    {
        database = database.TrimOrNull() ?? GetCurrentDatabaseName();
        if (database == null) throw new Exception("Could not determine current SQL database name");

        schema = schema.TrimOrNull() ?? GetCurrentSchemaName();
        if (schema == null) throw new Exception("Could not determine current SQL schema name");

        table = table.CheckNotNullTrimmed();
        table = Unescape(table).CheckNotNullTrimmed(nameof(table));

        return GetTables(database, schema).Any(o => o.TableName.EqualsIgnoreCase(table));
    }

    public override bool DropTable(string? database, string? schema, string table)
    {
        database = database.TrimOrNull() ?? GetCurrentDatabaseName();
        if (database == null) throw new Exception("Could not determine current SQL database name");

        schema = schema.TrimOrNull() ?? GetCurrentSchemaName();
        if (schema == null) throw new Exception("Could not determine current SQL schema name");

        table = table.CheckNotNullTrimmed();
        table = Unescape(table).CheckNotNullTrimmed(nameof(table));

        if (!GetTableExists(database, schema, table)) return false;

        var dst = Escape(database) + "." + Escape(schema) + "." + Escape(table);
        using (var cmd = CreateCommand())
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"DROP TABLE {dst};";
            cmd.ExecuteNonQuery();
        }
        return true;
    }

    #endregion Schema

}
