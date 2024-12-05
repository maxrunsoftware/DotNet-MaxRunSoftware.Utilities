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

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// Specifies SQL Server-specific data type of a field, property, for use in a
/// <see cref="T:System.Data.SqlClient.SqlParameter" />.
/// </summary>
[PublicAPI]
[DatabaseTypes(DatabaseAppType.SqliteSql, ExternalEnum = typeof(SqliteType))]
public enum SqliteSqlType
{
    /// <summary>
    /// A signed integer
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitetype?view=msdata-sqlite-8.0.0
    /// </summary>
    [DatabaseType(DbType.Int64, DatabaseTypeNames = new[] { "INTEGER", "INT" })]
    Integer	= 1,
    
    /// <summary>
    /// A floating point value
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitetype?view=msdata-sqlite-8.0.0
    /// </summary>
    [DatabaseType(DbType.Double, DatabaseTypeNames = new[] { "REAL" })]
    Real = 2,	
    
    /// <summary>
    /// A text string
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitetype?view=msdata-sqlite-8.0.0
    /// </summary>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "TEXT" })]
    Text = 3,	

    /// <summary>
    /// A blob of data
    /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitetype?view=msdata-sqlite-8.0.0
    /// </summary>
    [DatabaseType(DbType.Binary, DatabaseTypeNames = new[] { "BLOB" })]
    Blob = 4,	
}

public static class SqliteSqlTypeExtensions
{
    public static SqliteSqlType ToSqliteSqlType(this DbType dbType) =>
        dbType switch
        {
            DbType.AnsiString => SqliteSqlType.Text,
            DbType.Binary => SqliteSqlType.Blob,
            DbType.Byte => SqliteSqlType.Integer,
            DbType.Boolean => SqliteSqlType.Integer,
            DbType.Currency => SqliteSqlType.Text,
            DbType.Date => SqliteSqlType.Text,
            DbType.DateTime => SqliteSqlType.Text,
            DbType.Decimal => SqliteSqlType.Text,
            DbType.Double => SqliteSqlType.Real,
            DbType.Guid => SqliteSqlType.Text,
            DbType.Int16 => SqliteSqlType.Integer,
            DbType.Int32 => SqliteSqlType.Integer,
            DbType.Int64 => SqliteSqlType.Integer,
            DbType.Object => SqliteSqlType.Text,
            DbType.SByte => SqliteSqlType.Integer,
            DbType.Single => SqliteSqlType.Real,
            DbType.String => SqliteSqlType.Text,
            DbType.Time => SqliteSqlType.Text,
            DbType.UInt16 => SqliteSqlType.Integer,
            DbType.UInt32 => SqliteSqlType.Integer,
            DbType.UInt64 => SqliteSqlType.Integer,
            DbType.VarNumeric => SqliteSqlType.Text,
            DbType.AnsiStringFixedLength => SqliteSqlType.Text,
            DbType.StringFixedLength => SqliteSqlType.Text,
            DbType.Xml => SqliteSqlType.Text,
            DbType.DateTime2 => SqliteSqlType.Text,
            DbType.DateTimeOffset => SqliteSqlType.Text,
            _ => throw new NotImplementedException(),
        };
}
