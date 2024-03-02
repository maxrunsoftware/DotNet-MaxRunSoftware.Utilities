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

using MySql.Data.MySqlClient;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable RedundantStringInterpolation
public class MySql : Sql
{
    // https://dev.mysql.com/doc/refman/8.0/en/information-schema-columns-table.html

    public static MySqlConnection CreateConnection(string connectionString) => new(connectionString);

    public MySql(string connectionString, ILoggerProvider loggerProvider) : this(CreateConnection(connectionString), loggerProvider) { }
    public MySql(IDbConnection connection, ILoggerProvider loggerProvider) : base(connection, loggerProvider)
    {
        DefaultDataTypeString = DatabaseTypes.Get(MySqlType.LongText).TypeName;
        DefaultDataTypeInteger = DatabaseTypes.Get(MySqlType.Int32).TypeName;
        DefaultDataTypeDateTime = DatabaseTypes.Get(MySqlType.DateTime).TypeName;
        DialectEscapeLeft = '`';
        DialectEscapeRight = '`';

        // https://dev.mysql.com/doc/refman/8.0/en/identifiers.html
        IdentifierCharactersValid.AddRange((Constant.Chars_Alphanumeric_String + "$_").ToCharArray());

        // https://dev.mysql.com/doc/refman/8.0/en/keywords.html
        ReservedWords.AddRange(ReservedWordsParse(MySqlReservedWords.WORDS));
    }

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.MySql;
    protected override Type DatabaseTypesEnum => typeof(MySqlType);

    public MySqlServerProperties GetServerProperties() => new(this);

    #region Schema

    protected override string? GetCurrentDatabaseName() => this.QueryScalarString($"SELECT DATABASE();");

    protected override string? GetCurrentSchemaName() => this.QueryScalarString($"SELECT SCHEMA();");

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "schema_name",
        };
        var sql = new StringBuilder();
        sql.Append($"SELECT DISTINCT {EscapeColumns(cols)} FROM information_schema.schemata;");

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull(cols[0])
        ));
    }

    protected override IEnumerable<DatabaseSchemaSchema> GetSchemas(List<SqlError> errors, GetSchemaInfoFilter filter) => GetDatabases(errors).Select(o => new DatabaseSchemaSchema(o.DatabaseName, null));

    protected override IEnumerable<DatabaseSchemaTable> GetTables(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "TABLE_SCHEMA",
            [1] = "TABLE_SCHEMA",
            [2] = "TABLE_NAME",
        };
        var sql = new StringBuilder();
        sql.Append($" SELECT DISTINCT {EscapeColumns(cols)}");
        sql.Append($" FROM information_schema.tables");
        sql.Append($" WHERE TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND TABLE_SCHEMA='{filter.DatabaseNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTable(
            row[0].CheckNotNull(cols[0]),
            null,
            row[2].CheckNotNull(cols[2])
        ));
    }

    protected override IEnumerable<DatabaseSchemaTableColumn> GetTableColumns(List<SqlError> errors, GetSchemaInfoFilter filter)
    {
        var sql = new StringBuilder();
        var cols = new Dictionary<int, string> // Just using dict<int> to document index of each column
        {
            [0] = "TABLE_SCHEMA",
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
        sql.Append($" SELECT DISTINCT {EscapeColumns("c", cols)}");
        sql.Append($" FROM information_schema.columns c");
        sql.Append($" INNER JOIN information_schema.tables t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_SCHEMA='{filter.DatabaseNameUnescaped}'");
        if (filter.TableNameUnescaped != null) sql.Append($" AND c.TABLE_NAME='{filter.TableNameUnescaped}'");
        sql.Append(';');

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaTableColumn(
            new(
                row[0].CheckNotNull(cols[0]),
                null,
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
