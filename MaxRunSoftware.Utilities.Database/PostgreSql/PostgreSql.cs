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

using Npgsql;

namespace MaxRunSoftware.Utilities.Database;

// ReSharper disable RedundantStringInterpolation

public class PostgreSql : Sql
{
    public static NpgsqlConnection CreateConnection(string connectionString) => new(connectionString);

    public PostgreSql(string connectionString) : this(CreateConnection(connectionString)) { }
    public PostgreSql(IDbConnection connection) : base(connection) { }

    public static DatabaseDialectSettings DialectSettingsDefaultInstance { get; set; } = new DatabaseDialectSettings
        {
            DefaultDataTypeString = DatabaseTypes.Get(PostgreSqlType.Text).TypeName,
            DefaultDataTypeInteger = DatabaseTypes.Get(PostgreSqlType.Integer).TypeName,
            DefaultDataTypeDateTime = DatabaseTypes.Get(PostgreSqlType.Timestamp).TypeName,
            EscapeLeft = '"',
            EscapeRight = '"'
        }
        // https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS
        .AddIdentifierCharactersValid((Constant.Chars_Alphanumeric_String + "@$#_").ToCharArray())
        // https://www.postgresql.org/docs/current/sql-keywords-appendix.html
        .AddReservedWords(PostgreSqlReservedWords.WORDS);

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.PostgreSql;
    protected override Type DatabaseTypesEnum => typeof(PostgreSqlType);
    public override DatabaseDialectSettings DialectSettingsDefault => DialectSettingsDefaultInstance;

    #region Schema

    public override DatabaseSchemaSchema GetCurrentSchema()
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "DB_NAME()",
            [1] = "SCHEMA_NAME()",
        };
        return this.QueryStrings($"SELECT {EscapeColumns(cols, true)};")
            .Select(row => new DatabaseSchemaSchema(row, cols))
            .First();
    }

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "name",
        };
        var sql = new StringBuilder();
        sql.Append($"SELECT DISTINCT {EscapeColumns(cols)} FROM sys.databases;");
        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(row, cols));
    }


    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "CATALOG_NAME",
            [1] = "SCHEMA_NAME",
        };
        var sql = new StringBuilder();
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.SCHEMATA");
        sql.Append($" WHERE CATALOG_NAME='{filter.DatabaseNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaSchema(row, cols));
    }

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var sql = new StringBuilder();
        var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "TABLE_CATALOG",
            [1] = "TABLE_SCHEMA",
            [2] = "TABLE_NAME",
        };
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.TABLES");
        sql.Append($" WHERE TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND TABLE_CATALOG='{filter.DatabaseNameUnescaped}'");
        if (filter.SchemaNameUnescaped != null) sql.Append($" AND TABLE_SCHEMA='{filter.SchemaNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(row, cols));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter)
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
            [10] = "COLUMN_DEFAULT"
        };
        sql.Append($" SELECT DISTINCT {EscapeColumns("c",cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.COLUMNS c");
        sql.Append($" INNER JOIN {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.TABLES t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_CATALOG='{filter.DatabaseNameUnescaped}'");
        if (filter.SchemaNameUnescaped != null) sql.Append($" AND c.TABLE_SCHEMA='{filter.SchemaNameUnescaped}'");
        if (filter.TableNameUnescaped != null) sql.Append($" AND c.TABLE_NAME='{filter.TableNameUnescaped}'");
        sql.Append(';');


        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            new DatabaseSchemaTable(row, cols),
            row[3].CheckNotNullTrimmed(cols[3]),
            row[4].CheckNotNullTrimmed(cols[4]),
            GetDbType(row[4].CheckNotNullTrimmed(cols[4])) ?? DbType.String,
            row[5]!.ToBool(),
            row[6]!.ToInt(),
            row[7].ToLongNullable(),
            row[8].ToIntNullable(),
            row[9].ToIntNullable(),
            row[10]
        ));
    }

    public override bool DropTable(DatabaseSchemaTable table)
    {
        if (!GetTableExists(table)) return false;

        var dst = Escape(table);
        using (var cmd = CreateCommand())
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = $"DROP TABLE {dst};";
            cmd.ExecuteNonQuery();
        }

        return true;
    }

    public override string Escape(DatabaseSchemaSchema schema) => this.Escape(schema.Database.Name, schema.Name);


    #endregion Schema
}
