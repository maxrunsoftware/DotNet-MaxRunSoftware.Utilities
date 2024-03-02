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

using Npgsql;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable RedundantStringInterpolation
// ReSharper disable StringLiteralTypo
public class PostgreSql : Sql
{
    public static NpgsqlConnection CreateConnection(string connectionString) => new(connectionString);

    public PostgreSql(string connectionString, ILoggerProvider loggerProvider) : this(CreateConnection(connectionString), loggerProvider) { }
    public PostgreSql(IDbConnection connection, ILoggerProvider loggerProvider) : base(connection, loggerProvider)
    {
        DefaultDataTypeString = DatabaseTypes.Get(PostgreSqlType.Text).TypeName;
        DefaultDataTypeInteger = DatabaseTypes.Get(PostgreSqlType.Integer).TypeName;
        DefaultDataTypeDateTime = DatabaseTypes.Get(PostgreSqlType.Timestamp).TypeName;
        DialectEscapeLeft = '"';
        DialectEscapeRight = '"';

        // https://www.postgresql.org/docs/current/sql-syntax-lexical.html#SQL-SYNTAX-IDENTIFIERS
        IdentifierCharactersValid.AddRange((Constant.Chars_Alphanumeric_String + "@$#_").ToCharArray());

        // https://www.postgresql.org/docs/current/sql-keywords-appendix.html
        ReservedWords.AddRange(ReservedWordsParse(PostgreSqlReservedWords.WORDS));
    }

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.PostgreSql;
    protected override Type DatabaseTypesEnum => typeof(PostgreSqlType);

    #region Schema

    protected override string? GetCurrentDatabaseName() => this.QueryScalarString($"SELECT current_database();");

    protected override string? GetCurrentSchemaName() => this.QueryScalarString($"SELECT current_schema();");

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "datname",
        };
        var sql = new StringBuilder();
        sql.Append($"SELECT DISTINCT {EscapeColumns(cols)} FROM pg_catalog.pg_database WHERE datistemplate=false AND datallowconn=true;");

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull(cols[0])
        ));
    }


    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "catalog_name",
            [1] = "schema_name",
        };
        var sql = new StringBuilder();
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.information_schema.schemata");
        sql.Append($" WHERE catalog_name='{filter.DatabaseNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaSchema(
            row[0].CheckNotNull(cols[0]),
            row[1]
        ));
    }

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var sql = new StringBuilder();
        var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "table_catalog",
            [1] = "table_schema",
            [2] = "table_name",
        };
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.information_schema.tables");
        sql.Append($" WHERE table_type='BASE TABLE'");
        sql.Append($" AND table_catalog='{filter.DatabaseNameUnescaped}'");
        if (filter.SchemaNameUnescaped != null) sql.Append($" AND table_schema='{filter.SchemaNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(
            row[0].CheckNotNull(cols[0]),
            row[1],
            row[2].CheckNotNull(cols[2])
        ));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var sql = new StringBuilder();
        var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "table_catalog",
            [1] = "table_schema",
            [2] = "table_name",
            [3] = "column_name",
            [4] = "data_type",
            [5] = "is_nullable",
            [6] = "ordinal_position",
            [7] = "character_maximum_length",
            [8] = "numeric_precision",
            [9] = "numeric_scale",
            [10] = "column_default",
        };
        sql.Append($" SELECT DISTINCT {EscapeColumns("c", cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.information_schema.columns c");
        sql.Append($" INNER JOIN {filter.DatabaseNameEscaped}.information_schema.tables t ON t.table_catalog=c.table_catalog and t.table_schema=c.table_schema and t.table_name=c.table_name");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_CATALOG='{filter.DatabaseNameUnescaped}'");
        if (filter.SchemaNameUnescaped != null) sql.Append($" AND c.table_schema='{filter.SchemaNameUnescaped}'");
        if (filter.TableNameUnescaped != null) sql.Append($" AND c.table_name='{filter.TableNameUnescaped}'");
        sql.Append(';');


        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            new(
                row[0].CheckNotNull(cols[0]),
                row[1],
                row[2].CheckNotNull(cols[2])
            ),
            new(
                row[3].CheckNotNullTrimmed(cols[3]),
                row[4].CheckNotNullTrimmed(cols[4]),
                GetDbType(row[4].CheckNotNullTrimmed(cols[4])) ?? DbType.String,
                row[5]!.ToBool(),
                row[6]!.ToInt(),
                row[7].ToLongNullable(),
                row[8].ToIntNullable(),
                row[9].ToIntNullable(),
                row[10]
            )));
    }

    public override bool DropTable(DatabaseSchemaTable table)
    {
        if (!GetTableExists(table)) return false;
        NonQuery($"DROP TABLE {Escape(table)};");
        return true;
    }

    #endregion Schema
}
