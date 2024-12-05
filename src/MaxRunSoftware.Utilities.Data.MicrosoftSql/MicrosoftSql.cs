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

using Microsoft.Data.SqlClient;

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable RedundantStringInterpolation
// ReSharper disable StringLiteralTypo
public class MicrosoftSql : Sql
{
    public static SqlConnection CreateConnection(string connectionString) => new(connectionString);

    public MicrosoftSql(string connectionString, ILoggerFactory loggerProvider) : this(CreateConnection(connectionString), loggerProvider) { }
    public MicrosoftSql(IDbConnection connection, ILoggerFactory loggerProvider) : base(connection, loggerProvider)
    {
        DefaultDataTypeString = DatabaseTypes.Get(MicrosoftSqlType.NVarChar).TypeName + "(MAX)";
        DefaultDataTypeInteger = DatabaseTypes.Get(MicrosoftSqlType.Int).TypeName;
        DefaultDataTypeDateTime = DatabaseTypes.Get(MicrosoftSqlType.DateTime).TypeName;
        DialectEscapeLeft = '[';
        DialectEscapeRight = ']';

        //ExcludedDatabases.AddRange(new[] {"master", "model", "msdb", "tempdb"}.Select(o => new DatabaseSchemaDatabase(o)));

        // https://docs.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers?view=sql-server-ver16
        IdentifierCharactersValid.AddRange((Constant.Chars_Alphanumeric_String + "@$#_").ToCharArray());

        // https://docs.microsoft.com/en-us/sql/t-sql/language-elements/reserved-keywords-transact-sql
        ReservedWords.AddRange(ReservedWordsParse(MicrosoftSqlReservedWords.WORDS));
    }

    public override DatabaseAppType DatabaseAppType => DatabaseAppType.MicrosoftSql;

    protected override Type DatabaseTypesEnum => typeof(MicrosoftSqlType);

    public MicrosoftSqlServerProperties GetServerProperties() => new(this);


    #region Schema

    protected override string? GetCurrentDatabaseName() => this.QueryScalarString($"SELECT DB_NAME();");

    protected override string? GetCurrentSchemaName() => this.QueryScalarString($"SELECT SCHEMA_NAME();");

    protected override IEnumerable<DatabaseSchemaDatabase> GetDatabases(List<SqlError> errors)
    {
        var cols = new Dictionary<int, string>
        {
            [0] = "name",
        };
        var sql = new StringBuilder();
        sql.Append($"SELECT DISTINCT {EscapeColumns(cols)} FROM sys.databases;");

        return GetSchemaObjects(errors, sql, row => new DatabaseSchemaDatabase(
            row[0].CheckNotNull(cols[0])
        ));
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
        sql.Append($" SELECT DISTINCT {EscapeColumns("c", cols)}");
        sql.Append($" FROM {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.COLUMNS c");
        sql.Append($" INNER JOIN {filter.DatabaseNameEscaped}.INFORMATION_SCHEMA.TABLES t ON t.TABLE_CATALOG=c.TABLE_CATALOG AND t.TABLE_SCHEMA=c.TABLE_SCHEMA AND t.TABLE_NAME=c.TABLE_NAME");
        sql.Append($" WHERE t.TABLE_TYPE='BASE TABLE'");
        sql.Append($" AND c.TABLE_CATALOG='{filter.DatabaseNameUnescaped}'");
        if (filter.SchemaNameUnescaped != null) sql.Append($" AND c.TABLE_SCHEMA='{filter.SchemaNameUnescaped}'");
        if (filter.TableNameUnescaped != null) sql.Append($" AND c.TABLE_NAME='{filter.TableNameUnescaped}'");
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

    public override bool GetTableExists(DatabaseSchemaTable table)
    {
        var ps = this.NextParameter(table);
        var values = new List<DatabaseParameterValue> { ps.Table };

        var sql = new StringBuilder();
        sql.Append($" SELECT COUNT(*)");
        sql.Append($" FROM {Escape(table.Schema.Database)}.INFORMATION_SCHEMA.TABLES");
        sql.Append($" WHERE TABLE_NAME={ps.Table.Name}");
        if (table.Schema.SchemaName != null)
        {
            values.Add(ps.Schema);
            sql.Append($" AND TABLE_SCHEMA={ps.Schema.Name}");
        }

        return this.QueryScalarInt(sql.ToString(), values.ToArray()).CheckNotNull("COUNT") > 0;
    }

    #endregion Schema

    public virtual bool DropDatabase(string database)
    {
        var databaseEscaped = Escape(database);
        var databaseUnescaped = Unescape(database);

        var sql = @$"
        IF EXISTS (SELECT * FROM master.sys.databases WHERE NAME=N'{databaseUnescaped}')
        BEGIN
            ALTER DATABASE {databaseEscaped} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE {databaseEscaped};
            SELECT CAST(1 AS BIT);
        END
        ELSE
        BEGIN
            SELECT CAST(0 AS BIT);
        END
        ";

        var o = QueryScalar(sql);
        if (o == null) throw new NullReferenceException();
        if (o is bool oBool) return oBool;
        return o.ToString()!.ToBool();
    }


    /*
    public string DefaultDatabasePathMDF
    {
        get
        {

        }
    }

    private (string PathMDF, string PathLDF) GetDatabasePaths()
    {
        string? ExecuteScalarIgnoreException(string sqls)
        {
            foreach (var sql in sqls.SplitOnNewline().TrimOrNull().WhereNotNull())
            {
                try
                {
                    var o = this.QueryScalarString(sql).TrimOrNull();
                    if (o != null) return o;
                }
                catch (Exception e)
                {
                    log.LogTrace(e, nameof(GetDatabasePaths) + "." + nameof(ExecuteScalarIgnoreException) + " failed: {Sql}", sql);
                }
            }
            return null;
        }

        string? RemoveDatabaseFileName(string? path)
        {
            if (path == null) return null;
            var countW = path.CountOccurrences("\\");
            var countL = path.CountOccurrences("/");

        }

        var pathMDF = ServerProperties.InstanceDefaultDataPath.TrimOrNull();
        var pathLDF = ServerProperties.InstanceDefaultLogPath.TrimOrNull();

        if (pathMDF != null && pathLDF == null) pathLDF = pathMDF;
        if (pathMDF == null && pathLDF != null) pathMDF = pathLDF;

        if (pathMDF == null || pathLDF == null)
        {
            var databaseIdNullable = ExecuteScalarIgnoreException(@"
                SELECT database_id FROM master.sys.databases WHERE name='master'
                SELECT MIN(database_id) FROM master.sys.master_files WHERE name='master'
                SELECT MIN(database_id) FROM master.sys.master_files
            ").TrimOrNull().ToIntNullable();
            if (databaseIdNullable == null) throw new Exception("Could not determine 'master' database_id");
            var databaseId = databaseIdNullable.Value;

            if (pathMDF == null)
            {
                pathMDF = ExecuteScalarIgnoreException(@$"
                    SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='ROWS' AND database_id={databaseId} ORDER BY file_id
                    SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='ROWS' AND name='master' ORDER BY file_id
                ").TrimOrNull();
                if (pathMDF != null)
                {

                }
            }


        }
        if (path == null)
        {


        }
    }
    public virtual void CreateDatabase(
        string database,
        string? mdfFileName = null,
        string? mdfFileDirectory = null,
        string? ldfFileName = null,
        string? ldfFileDirectory = null
    ) {



        if (pathData == null)
        {

        }
        var sqls = @"


            SELECT SERVERPROPERTY('InstanceDefaultDataPath')
            SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='ROWS' AND database_id=@database_id ORDER BY file_id
            SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='ROWS' AND name='master' ORDER BY file_id

            SELECT SERVERPROPERTY('InstanceDefaultLogPath')
            SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='LOG' AND database_id=@database_id ORDER BY file_id
            SELECT TOP 1 physical_name FROM master.sys.master_files WHERE type_desc='LOG' AND name='mastlog' ORDER BY file_id
        ";
        var databaseEscaped = this.Escape(database);
        var databaseUnescaped = this.Unescape(database);

        var sqlsMasterDatabaseFiles
        var sql1 = "SELECT * FROM master.sys.master_files WHERE database_id=1 AND file_id=1;";
        SELECT * FROM master.sys.master_files WHERE name='master' AND type_desc='ROWS';
        "
        var dataPath = ServerProperties.InstanceDefaultDataPath.TrimOrNull();
        var logPath = ServerProperties.InstanceDefaultLogPath.TrimOrNull();



        var sql = @$"
            DECLARE @path_mdf NVARCHAR(4000) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS NVARCHAR(4000))
            DECLARE @path_ldf NVARCHAR(4000) = CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS NVARCHAR(4000))

            SET @path_mdf = COALESCE(@path_mdf, @path_ldf)
            SET @path_ldf = COALESCE(@path_ldf, @path_mdf)

            IF @path_mdf IS NULL AND @path_ldf IS NULL
            BEGIN
                SET @path_mdf = ( SELECT SUBSTRING(filename, 1, CHARINDEX(N'master.mdf', LOWER(filename)) - 1) FROM master.sys.sysaltfiles WHERE dbid=1 AND fileid=1 )
                SET @path_ldf = @path_mdf
            END
            IF @path_mdf IS NOT NULL AND LEN(@path_mdf) > 0 AND RIGHT(@path_mdf, 1) NOT IN ('/', '\') SET @path_mdf = @path_mdf + '/'
            IF @path_ldf IS NOT NULL AND LEN(@path_ldf) > 0 AND RIGHT(@path_ldf, 1) NOT IN ('/', '\') SET @path_ldf = @path_ldf + '/'

            SET @path_mdf = COALESCE(@path_mdf, '') + '{db}.mdf'
            SET @path_ldf = COALESCE(@path_ldf, '') + '{db}.ldf'

            DECLARE @SQL_CREATE NVARCHAR(MAX) = N'
              CREATE DATABASE [{db}]
                  ON PRIMARY (NAME = N''{db}''     , FILENAME = N''$MDF$'' , SIZE = 16MB, FILEGROWTH = 16MB)
              LOG ON         (NAME = N''{db}_log'' , FILENAME = N''$LDF$'' , SIZE = 16MB, FILEGROWTH = 16MB)
            '
            SET @SQL_CREATE = REPLACE(@SQL_CREATE, '$MDF$', @path_mdf)
            SET @SQL_CREATE = REPLACE(@SQL_CREATE, '$LDF$', @path_ldf)
            SET @SQL_CREATE = TRIM(@SQL_CREATE)

            EXEC(@SQL_CREATE);
        ";
    }
    */
}
