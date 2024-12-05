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

using Microsoft.Data.Sqlite;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable RedundantStringInterpolation
// ReSharper disable StringLiteralTypo
public class SqliteSql : Sql
{
    public static SqliteConnection CreateConnection(string connectionString) => new(connectionString);

    public SqliteSql(string connectionString, ILoggerFactory loggerProvider) : this(CreateConnection(connectionString), loggerProvider) { }
    public SqliteSql(IDbConnection connection, ILoggerFactory loggerProvider) : base(connection, loggerProvider)
    {
        DefaultDataTypeString = DatabaseTypes.Get(SqliteSqlType.Text).TypeName;
        DefaultDataTypeInteger = DatabaseTypes.Get(SqliteSqlType.Integer).TypeName;
        DefaultDataTypeDateTime = DatabaseTypes.Get(SqliteSqlType.Text).TypeName;
        DialectEscapeLeft = '"';
        DialectEscapeRight = '"';

        // https://www.sqlite.org/draft/tokenreq.html
        // https://stackoverflow.com/a/51574648
        IdentifierCharactersValid.AddRange((Constant.Chars_Alphanumeric_String + "$_").ToCharArray());

        // https://sqlite.org/lang_keywords.html
        ReservedWords.AddRange(ReservedWordsParse(SqliteSqlReservedWords.WORDS));
    }

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.SqliteSql;
    protected override Type DatabaseTypesEnum => typeof(SqliteSqlType);

    #region Schema

    /// <summary>
    /// https://stackoverflow.com/a/18072783
    /// https://www.sqlite.org/pragma.html#pragma_database_list
    /// </summary>
    /// <returns>Current database name</returns>
    protected override string? GetCurrentDatabaseName() => this.QueryScalarString($"SELECT name FROM pragma_database_list;");

    protected override string? GetCurrentSchemaName() => this.QueryScalarString($"SELECT name FROM pragma_database_list;");

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "name",
        };
        var sql = new StringBuilder();
        sql.Append($"SELECT DISTINCT {EscapeColumns(cols)} FROM pragma_database_list;");

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull(cols[0])
        ));
    }


    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter) => GetDatabases(errors).Select(o => new DatabaseSchemaSchema(o.DatabaseName, null));

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "schema",
            [1] = "name",
        };
        var sql = new StringBuilder();
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM pragma_table_list");
        sql.Append($" WHERE type='table'");
        sql.Append($" AND schema='{filter.DatabaseNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(
            row[0].CheckNotNull(cols[0]),
            null,
            row[1].CheckNotNull(cols[1])
        ));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var sql = new StringBuilder();
        var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "database_name",
            [1] = "table_name",
            [2] = "column_name",
            [3] = "column_ordinal",
            [4] = "column_type",
            [5] = "is_nullable",
            [6] = "default_value",
        };
        
        var sqlStr =
            $"""
              SELECT DISTINCT
                  t.schema database_name
              ,   t.name table_name
              ,   i.name column_name
              ,   (i.cid + 1) column_ordinal
              ,   i.type column_type
              ,   i."notnull" is_nullable
              ,   i.dflt_value default_value
              FROM pragma_table_list t
              JOIN pragma_table_info(t.name) i ON 1=1
              WHERE 1=1
              AND t.type='table'
              AND t.schema='{filter.DatabaseNameUnescaped}'
              """;
        sql.Append(sqlStr);
        if (filter.TableNameUnescaped != null) sql.Append($" AND t.name='{filter.TableNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            new(
                databaseName: row[0].CheckNotNull(cols[0]),
                schemaName: null,
                tableName: row[1].CheckNotNull(cols[1])
            ),
            new(
                columnName: row[2].CheckNotNullTrimmed(cols[2]),
                ordinal: row[3]!.ToInt(),
                columnType: row[4].CheckNotNullTrimmed(cols[4]),
                columnDbType: GetDbType(row[4].CheckNotNullTrimmed(cols[4])) ?? DbType.String,
                isNullable: row[5]!.ToBool(),
                columnDefault: row[6]
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
