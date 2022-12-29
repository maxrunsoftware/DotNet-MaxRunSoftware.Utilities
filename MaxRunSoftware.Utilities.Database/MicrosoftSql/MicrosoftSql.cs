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

public class MicrosoftSql : SqlBase
{
    public MicrosoftSql(IDbConnection connection) : base(connection) { }

    public static DatabaseDialectSettings DialectSettingsDefaultInstance { get; set; } = new DatabaseDialectSettings
        {
            DefaultDataTypeString = DatabaseTypes.Get(MicrosoftSqlType.NVarChar).TypeName + "(MAX)", // GetSqlDbType(SqlMsSqlType.NVarChar).SqlTypeName + "(MAX)";
            DefaultDataTypeInteger = DatabaseTypes.Get(MicrosoftSqlType.Int).TypeName, // GetSqlDbType(SqlMsSqlType.Int).SqlTypeName;
            DefaultDataTypeDateTime = DatabaseTypes.Get(MicrosoftSqlType.DateTime).TypeName, // GetSqlDbType(SqlMsSqlType.DateTime).SqlTypeName;
            EscapeLeft = '[',
            EscapeRight = ']',
        } //.AddDatabaseUserExcluded("master", "model", "msdb", "tempdb")
        // https://docs.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers?view=sql-server-ver16
        .AddIdentifierCharactersValid((Constant.Chars_Alphanumeric_String + "@$#_").ToCharArray())
        // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
        .AddReservedWords(MicrosoftSqlReservedWords.WORDS);

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.MicrosoftSql;
    protected override Type DatabaseTypesEnum => typeof(MicrosoftSqlType);
    public override DatabaseDialectSettings DialectSettingsDefault => DialectSettingsDefaultInstance;

    #region Schema

    public override DatabaseSchemaSchema GetCurrentSchema() => this.QueryStrings("SELECT DB_NAME(),SCHEMA_NAME();")
            .Select(row => new DatabaseSchemaSchema(row[0].CheckNotNull("DB_NAME()"), row[1].CheckNotNull("SCHEMA_NAME()")))
            .First();

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var sql = new StringBuilder();
        sql.Append("SELECT DISTINCT [name] FROM sys.databases;");
        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull("sys.databases.name")
        ));
    }


    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, DatabaseSchemaDatabase database)
    {
            var sql = new StringBuilder();
            sql.Append($" SELECT DISTINCT [CATALOG_NAME],[SCHEMA_NAME]");
            sql.Append($" FROM {Escape(database.DatabaseName)}.INFORMATION_SCHEMA.SCHEMATA");
            sql.Append($" WHERE CATALOG_NAME='{Unescape(database.DatabaseName)}'");
            sql.Append(';');

            return GetSchemaObjects(errors, sql, row => new DatabaseSchemaSchema(
                databaseName: row[0].CheckNotNullTrimmed("CATALOG_NAME"),
                schemaName: row[1].CheckNotNullTrimmed("SCHEMA_NAME")
            ));
    }

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetTablesFilter filter)
    {


        var sql = new StringBuilder();
        sql.Append($" SELECT DISTINCT [TABLE_CATALOG],[TABLE_SCHEMA],[TABLE_NAME]");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.TABLES");
        sql.Append($" WHERE TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND TABLE_CATALOG='{filter.DatabaseName}'");
        if (filter.SchemaName != null) sql.Append($" AND TABLE_SCHEMA='{filter.SchemaName}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(
            databaseName: row[0].CheckNotNullTrimmed("TABLE_CATALOG"),
            schemaName: row[1].CheckNotNullTrimmed("TABLE_SCHEMA"),
            tableName: row[2].CheckNotNullTrimmed("TABLE_NAME")
        ));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetTableColumnsFilter filter)
    {
        var sql = new StringBuilder();
        var colsDict = new Dictionary<int, string> // Just using dict<int> to document index of each column
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
        };
        var cols = colsDict.OrderBy(kvp => kvp.Key).Select(o => $"c.{Escape(o.Value)}").ToStringDelimited(",");
        sql.Append($" SELECT DISTINCT {cols}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.COLUMNS c");
        sql.Append($" INNER JOIN {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.TABLES t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_CATALOG='{filter.DatabaseName}'");
        if (filter.SchemaName != null) sql.Append($" AND c.TABLE_SCHEMA='{filter.SchemaName}'");
        if (filter.TableName != null) sql.Append($" AND c.TABLE_NAME='{filter.TableName}'");
        sql.Append(';');



        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            table: new DatabaseSchemaTable(
                databaseName: row[0].CheckNotNullTrimmed(colsDict[0]),
                schemaName: row[1].CheckNotNullTrimmed(colsDict[1]),
                tableName: row[2].CheckNotNullTrimmed(colsDict[2])
                ),
            columnName: row[3].CheckNotNullTrimmed(colsDict[3]),
            columnType: row[4].CheckNotNullTrimmed(colsDict[4]),
            columnDbType: GetDbType(row[4].CheckNotNullTrimmed(colsDict[4])) ?? DbType.String,
            isNullable: row[5]!.ToBool(),
            ordinal: row[6]!.ToInt(),
            characterLengthMax: row[7].ToLongNullable(),
            numericPrecision: row[8].ToIntNullable(),
            numericScale: row[9].ToIntNullable(),
            columnDefault: row[10]
        ));


    }

    public override bool DropTable(DatabaseSchemaTable table)
    {
        if (!GetTableExists(table)) return false;

        var dst = Escape(table.Schema.Database.DatabaseName, table.Schema.SchemaName, table.TableName);
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
