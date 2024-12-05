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

using NpgsqlTypes;

// ReSharper disable CommentTypo
// ReSharper disable IdentifierTypo

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// Specifies SQL Server-specific data type of a field, property, for use in a
/// <see cref="T:System.Data.SqlClient.SqlParameter" />.
/// </summary>
[PublicAPI]
[DatabaseTypes(DatabaseAppType.PostgreSql, ExternalEnum = typeof(NpgsqlDbType))]
public enum PostgreSqlType
{
    #region Numeric Types

    /// <summary>
    /// Corresponds to the PostgreSQL 8-byte "bigint" type.
    /// </summary>
    [DatabaseType(DbType.Int64, DatabaseTypeNames = new[] { nameof(Bigint), "int8" })]
    Bigint = 1,

    /// <summary>
    /// Corresponds to the PostgreSQL 8-byte floating-point "double" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    [DatabaseType(DbType.Double, DatabaseTypeNames = new[] { "double precision", "float8" })]
    Double = 8,

    /// <summary>
    /// Corresponds to the PostgreSQL 4-byte "integer" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    [DatabaseType(DbType.Int32, DatabaseTypeNames = new[] { nameof(Integer), "int", "int4" })]
    Integer = 9,

    /// <summary>
    /// Corresponds to the PostgreSQL arbitrary-precision "numeric" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    [DatabaseType(DbType.Decimal, DatabaseTypeNames = new[] { nameof(Numeric), "decimal" })]
    Numeric = 13,

    /// <summary>
    /// Corresponds to the PostgreSQL floating-point "real" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    [DatabaseType(DbType.Single, DatabaseTypeNames = new[] { nameof(Real), "float4" })]
    Real = 17,

    /// <summary>
    /// Corresponds to the PostgreSQL 2-byte "smallint" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>
    [DatabaseType(DbType.Int16, DatabaseTypeNames = new[] { nameof(Smallint), "int2" })]
    Smallint = 18,

    /// <summary>
    /// Corresponds to the PostgreSQL "money" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-money.html</remarks>
    [DatabaseType(DbType.Currency)]
    Money = 12,

    #endregion

    #region Boolean Type

    /// <summary>
    /// Corresponds to the PostgreSQL "boolean" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>
    [DatabaseType(DbType.Boolean, DatabaseTypeNames = new[] { nameof(Boolean), "bool" })]
    Boolean = 2,

    #endregion

    #region Geometric types

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "box" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Box = 3,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "circle" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Circle = 5,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "line" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Line = 10,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "lseg" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    LSeg = 11,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "path" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Path = 14,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "point" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Point = 15,

    /// <summary>
    /// Corresponds to the PostgreSQL geometric "polygon" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>
    [DatabaseType(DbType.Object)]
    Polygon = 16,

    #endregion

    #region Character Types

    /// <summary>
    /// Corresponds to the PostgreSQL "char(n)" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    [DatabaseType(DbType.StringFixedLength, DatabaseTypeNames = new[] { "character", nameof(Char) })]
    Char = 6,

    /// <summary>
    /// Corresponds to the PostgreSQL "text" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    [DatabaseType(DbType.String)]
    Text = 19,

    /// <summary>
    /// Corresponds to the PostgreSQL "varchar" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "character varying", nameof(Varchar) })]
    Varchar = 22,

    #endregion

    #region Binary Data Types

    /// <summary>
    /// Corresponds to the PostgreSQL "bytea" type, holding a raw byte string.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-binary.html</remarks>
    [DatabaseType(DbType.Binary)]
    Bytea = 4,

    #endregion

    #region Date/Time Types

    /// <summary>
    /// Corresponds to the PostgreSQL "date" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.Date)]
    Date = 7,

    /// <summary>
    /// Corresponds to the PostgreSQL "time" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.Time, DatabaseTypeNames = new[] { nameof(Time), "time without time zone" })]
    Time = 20,

    /// <summary>
    /// Corresponds to the PostgreSQL "timestamp" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.DateTime, DatabaseTypeNames = new[] { nameof(Timestamp), "timestamp without time zone" })]
    Timestamp = 21,

    // /// <summary>
    // /// Corresponds to the PostgreSQL "timestamp with time zone" type.
    // /// </summary>
    // /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    // [Obsolete("Use TimestampTz instead")]  // NOTE: Don't remove this (see #1694)
    // TimestampTZ = TimestampTz,

    /// <summary>
    /// Corresponds to the PostgreSQL "timestamp with time zone" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.DateTimeOffset, DatabaseTypeNames = new[] { "timestamp with time zone", nameof(TimestampTz) })]
    TimestampTz = 26,

    /// <summary>
    /// Corresponds to the PostgreSQL "interval" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.String)] // TODO: not sure what to use for DbType
    Interval = 30,

    // /// <summary>
    // /// Corresponds to the PostgreSQL "time with time zone" type.
    // /// </summary>
    // /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    // [Obsolete("Use TimeTz instead")]  // NOTE: Don't remove this (see #1694)
    // TimeTZ = TimeTz,

    /// <summary>
    /// Corresponds to the PostgreSQL "time with time zone" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    [DatabaseType(DbType.Int32, DatabaseTypeNames = new[] { "time with time zone", nameof(TimeTz) })]
    TimeTz = 31,

    // /// <summary>
    // /// Corresponds to the obsolete PostgreSQL "abstime" type.
    // /// </summary>
    // /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
    // [Obsolete("The PostgreSQL abstime time is obsolete.")]
    // Abstime = 33,

    #endregion

    #region Network Address Types

    /// <summary>
    /// Corresponds to the PostgreSQL "inet" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    [DatabaseType(DbType.String)]
    Inet = 24,

    /// <summary>
    /// Corresponds to the PostgreSQL "cidr" type, a field storing an IPv4 or IPv6 network.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    [DatabaseType(DbType.String)]
    Cidr = 44,

    /// <summary>
    /// Corresponds to the PostgreSQL "macaddr" type, a field storing a 6-byte physical address.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    [DatabaseType(DbType.String)]
    MacAddr = 34,

    /// <summary>
    /// Corresponds to the PostgreSQL "macaddr8" type, a field storing a 6-byte or 8-byte physical address.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>
    [DatabaseType(DbType.String)]
    MacAddr8 = 54,

    #endregion

    #region Bit String Types

    /// <summary>
    /// Corresponds to the PostgreSQL "bit" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-bit.html</remarks>
    [DatabaseType(DbType.String)]
    Bit = 25,

    /// <summary>
    /// Corresponds to the PostgreSQL "varbit" type, a field storing a variable-length string of bits.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "bit varying", nameof(Varbit) })]
    Varbit = 39,

    #endregion

    #region Text Search Types

    /// <summary>
    /// Corresponds to the PostgreSQL "tsvector" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    [DatabaseType(DbType.String)]
    TsVector = 45,

    /// <summary>
    /// Corresponds to the PostgreSQL "tsquery" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    [DatabaseType(DbType.String)]
    TsQuery = 46,

    /// <summary>
    /// Corresponds to the PostgreSQL "regconfig" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>
    [DatabaseType(DbType.String)]
    Regconfig = 56,

    #endregion

    #region UUID Type

    /// <summary>
    /// Corresponds to the PostgreSQL "uuid" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-uuid.html</remarks>
    [DatabaseType(DbType.Guid)]
    Uuid = 27,

    #endregion

    #region XML Type

    /// <summary>
    /// Corresponds to the PostgreSQL "xml" type.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-xml.html</remarks>
    [DatabaseType(DbType.String)]
    Xml = 28,

    #endregion

    #region JSON Types

    /// <summary>
    /// Corresponds to the PostgreSQL "json" type, a field storing JSON in text format.
    /// </summary>
    /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-json.html</remarks>
    /// <seealso cref="Jsonb" />
    [DatabaseType(DbType.String)]
    Json = 35,

    /// <summary>
    /// Corresponds to the PostgreSQL "jsonb" type, a field storing JSON in an optimized binary.
    /// format.
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 9.4.
    /// See https://www.postgresql.org/docs/current/static/datatype-json.html
    /// </remarks>
    [DatabaseType(DbType.String)]
    Jsonb = 36,

    /// <summary>
    /// Corresponds to the PostgreSQL "jsonpath" type, a field storing JSON path in text format.
    /// format.
    /// </summary>
    /// <remarks>
    /// Supported since PostgreSQL 12.
    /// See https://www.postgresql.org/docs/current/datatype-json.html#DATATYPE-JSONPATH
    /// </remarks>
    [DatabaseType(DbType.String)]
    JsonPath = 57,

    #endregion

    #region Internal Types

    /// <summary>
    /// Corresponds to the PostgreSQL "pg_lsn" type, which can be used to store LSN (Log Sequence Number) data which
    /// is a pointer to a location in the WAL.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/datatype-pg-lsn.html and
    /// https://git.postgresql.org/gitweb/?p=postgresql.git;a=commit;h=7d03a83f4d0736ba869fa6f93973f7623a27038a
    /// </remarks>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "pg_lsn" })]
    PgLsn = 59,

    #endregion
}

public static class PostgreSqlTypeExtensions
{
    public static PostgreSqlType ToPostgreSqlType(this DbType dbType) =>
        dbType switch
        {
            DbType.AnsiString => PostgreSqlType.Varchar,
            DbType.Binary => PostgreSqlType.Bytea,
            DbType.Byte => PostgreSqlType.Smallint,
            DbType.Boolean => PostgreSqlType.Boolean,
            DbType.Currency => PostgreSqlType.Money,
            DbType.Date => PostgreSqlType.Date,
            DbType.DateTime => PostgreSqlType.Timestamp,
            DbType.Decimal => PostgreSqlType.Numeric,
            DbType.Double => PostgreSqlType.Double,
            DbType.Guid => PostgreSqlType.Uuid,
            DbType.Int16 => PostgreSqlType.Smallint,
            DbType.Int32 => PostgreSqlType.Integer,
            DbType.Int64 => PostgreSqlType.Bigint,
            DbType.Object => PostgreSqlType.Text,
            DbType.SByte => PostgreSqlType.Smallint,
            DbType.Single => PostgreSqlType.Real,
            DbType.String => PostgreSqlType.Text,
            DbType.Time => PostgreSqlType.Time,
            DbType.UInt16 => PostgreSqlType.Integer,
            DbType.UInt32 => PostgreSqlType.Bigint,
            DbType.UInt64 => PostgreSqlType.Numeric,
            DbType.VarNumeric => PostgreSqlType.Numeric,
            DbType.AnsiStringFixedLength => PostgreSqlType.Char,
            DbType.StringFixedLength => PostgreSqlType.Char,
            DbType.Xml => PostgreSqlType.Xml,
            DbType.DateTime2 => PostgreSqlType.TimestampTz,
            DbType.DateTimeOffset => PostgreSqlType.TimestampTz,
            _ => throw new NotImplementedException(),
        };
}
