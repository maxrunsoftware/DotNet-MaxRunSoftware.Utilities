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

public class MySql : SqlBase
{
    // https://dev.mysql.com/doc/refman/8.0/en/information-schema-columns-table.html



    public MySql(IDbConnection connection) : base(connection) { }

    public static DatabaseDialectSettings DialectSettingsDefaultInstance { get; set; } = new DatabaseDialectSettings
        {
            DefaultDataTypeString = DatabaseTypes.Get(MySqlType.LongText).TypeName,
            DefaultDataTypeInteger = DatabaseTypes.Get(MySqlType.Int32).TypeName,
            DefaultDataTypeDateTime = DatabaseTypes.Get(MySqlType.DateTime).TypeName,
            EscapeLeft = '`',
            EscapeRight = '`',
        }
        // https://dev.mysql.com/doc/refman/8.0/en/identifiers.html
        .AddIdentifierCharactersValid((Constant.Chars_Alphanumeric_String + "$_").ToCharArray())
        // https://dev.mysql.com/doc/refman/8.0/en/keywords.html
        .AddReservedWords(MicrosoftSqlReservedWords.WORDS);

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.MySql;
    protected override Type DatabaseTypesEnum => typeof(MySqlType);
    public override DatabaseDialectSettings DialectSettingsDefault => DialectSettingsDefaultInstance;

    #region Schema

    public override DatabaseSchemaSchema GetCurrentSchema() => this.QueryStrings("SELECT DATABASE();")
        .Select(row => new DatabaseSchemaSchema(row[0].CheckNotNull("DATABASE()"), row[0].CheckNotNull("DATABASE()")))
        .First();


    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var sql = new StringBuilder();
        sql.Append("SELECT DISTINCT schema_name FROM information_schema.schemata;");
        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull("information_schema.schemata.schema_name")
        ));
    }

    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, DatabaseSchemaDatabase database) => GetDatabases(errors).Select(o => new DatabaseSchemaSchema(o.DatabaseName, o.DatabaseName));

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetTablesFilter filter)
    {
        var sql = new StringBuilder();
        sql.Append("SELECT DISTINCT TABLE_SCHEMA,TABLE_NAME");
        sql.Append(" FROM information_schema.tables");
        sql.Append(" WHERE TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND TABLE_SCHEMA='{filter.DatabaseName}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(

            databaseName: row[0].CheckNotNullTrimmed("TABLE_SCHEMA"),
            schemaName: row[0].CheckNotNullTrimmed("TABLE_SCHEMA"),
            tableName: row[1].CheckNotNullTrimmed("TABLE_NAME")
        ));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetTableColumnsFilter filter)
    {
        var sql = new StringBuilder();
        var colsDict = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "TABLE_SCHEMA",
            [1] = "TABLE_NAME",
            [2] = "COLUMN_NAME",
            [3] = "DATA_TYPE",
            [4] = "IS_NULLABLE",
            [5] = "ORDINAL_POSITION",
            [6] = "CHARACTER_MAXIMUM_LENGTH",
            [7] = "NUMERIC_PRECISION",
            [8] = "NUMERIC_SCALE",
            [9] = "COLUMN_DEFAULT",
        };
        var cols = colsDict.OrderBy(kvp => kvp.Key).Select(o => $"c.{Escape(o.Value)}").ToStringDelimited(",");
        sql.Append($" SELECT DISTINCT {cols}");
        sql.Append($" FROM information_schema.columns c");
        sql.Append($" INNER JOIN information_schema.tables t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_SCHEMA='{filter.DatabaseName}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            table: new DatabaseSchemaTable(
                databaseName: row[0].CheckNotNullTrimmed(colsDict[0]),
                schemaName: row[0].CheckNotNullTrimmed(colsDict[0]),
                tableName: row[1].CheckNotNullTrimmed(colsDict[1])
            ),
            columnName: row[2].CheckNotNullTrimmed(colsDict[2]),
            columnType: row[3].CheckNotNullTrimmed(colsDict[3]),
            columnDbType: GetDbType(row[3].CheckNotNullTrimmed(colsDict[3])) ?? DbType.String,
            isNullable: row[4]!.ToBool(),
            ordinal: row[5]!.ToInt(),
            characterLengthMax: row[6].ToLongNullable(),
            numericPrecision: row[7].ToIntNullable(),
            numericScale: row[8].ToIntNullable(),
            columnDefault: row[9]
        ));


    }

    public override bool DropTable(DatabaseSchemaTable table)
    {
        if (!GetTableExists(table)) return false;

        var dst = Escape(table.Schema.Database.DatabaseName, table.TableName);
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
