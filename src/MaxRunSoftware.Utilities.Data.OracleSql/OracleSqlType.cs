﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.data.oracleclient.oracletype?view=netframework-4.8">System.Data.OracleClient.OracleType</see>
/// https://docs.oracle.com/cd/B14117_01/server.101/b10758/sqlqr06.htm
/// https://docs.oracle.com/cd/A58617_01/server.804/a58241/ch5.htm
/// https://docs.oracle.com/cd/B19306_01/win.102/b14307/featSafeType.htm
/// </summary>
[PublicAPI]
[DatabaseTypes(DatabaseAppType.OracleSql)]
public enum OracleSqlType
{
    /// <summary>
    /// An Oracle BFILE data type that contains a reference to binary data with a maximum size of 4 gigabytes that is stored in
    /// an external file. Use the OracleClient OracleBFile data type with the Value property.
    /// </summary>
    [DatabaseType(DbType.Binary)] BFile = 1,

    /// <summary>
    /// An Oracle BLOB data type that contains binary data with a maximum size of 4 gigabytes. Use the OracleClient OracleLob
    /// data type in Value.
    /// </summary>
    [DatabaseType(DbType.Binary)] Blob = 2,

    /// <summary>
    /// An integral type representing unsigned 8-bit integers with values between 0 and 255. This is not a native Oracle data
    /// type, but is provided to improve performance when binding input parameters. Use the .NET Byte data type in Value.
    /// </summary>
    [DatabaseType(DbType.Byte, AliasFor = Number)]
    Byte = 23,

    /// <summary>
    /// An Oracle CHAR data type that contains a fixed-length character string with a maximum size of 2,000 bytes. Use the .NET
    /// String or OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.AnsiStringFixedLength)]
    Char = 3,

    /// <summary>
    /// An Oracle CLOB data type that contains character data, based on the default character set on the server, with a maximum
    /// size of 4 gigabytes. Use the OracleClient OracleLob data type in Value.
    /// </summary>
    [DatabaseType(DbType.AnsiString)] Clob = 4,

    /// <summary>
    /// An Oracle REF CURSOR. The OracleDataReader object is not available.
    /// </summary>
    [DatabaseType(DbType.Object, AliasFor = NClob)]
    Cursor = 5,

    /// <summary>
    /// An Oracle DATE data type that contains a fixed-length representation of a date and time, ranging from January 1, 4712
    /// B.C. to December 31, A.D. 4712, with the default format dd-mmm-yy. For A.D. dates, DATE maps to DateTime. To bind B.C.
    /// dates, use a String parameter and the Oracle TO_DATE or TO_CHAR conversion functions for input and output parameters
    /// respectively. Use the .NET DateTime or OracleClient OracleDateTime data type in Value.
    /// </summary>
    [DatabaseType(DbType.DateTime, DatabaseTypeNames = new[] { "DATE" })]
    DateTime = 6,

    /// <summary>
    /// A double-precision floating-point value. This is not a native Oracle data type, but is provided to improve performance
    /// when binding input parameters. For information about conversion of Oracle numeric values to common language runtime
    /// (CLR) data types, see OracleNumber. Use the .NET Double or OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.Double, AliasFor = Number, DatabaseTypeNames = new[] { "DOUBLE PRECISION" })]
    Double = 30,

    /// <summary>
    /// A single-precision floating-point value. This is not a native Oracle data type, but is provided to improve performance
    /// when binding input parameters. For information about conversion of Oracle numeric values to common language runtime
    /// data types, see OracleNumber. Use the .NET Single or OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.Single, AliasFor = Number)]
    Float = 29,

    /// <summary>
    /// An integral type representing signed 16-bit integers with values between -32768 and 32767. This is not a native Oracle
    /// data type, but is provided to improve performance when binding input parameters. For information about conversion of
    /// Oracle numeric values to common language runtime (CLR) data types, see OracleNumber. Use the .NET Int16 or OracleClient
    /// OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.Int16, AliasFor = Number, DatabaseTypeNames = new[] { "SMALLINT" })]
    Int16 = 27,

    /// <summary>
    /// An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647. This is not a
    /// native Oracle data type, but is provided for performance when binding input parameters. For information about
    /// conversion of Oracle numeric values to common language runtime data types, see OracleNumber. Use the .NET Int32 or
    /// OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.Int32, AliasFor = Number, DatabaseTypeNames = new[] { "INTEGER", "INT" })]
    Int32 = 28,

    /// <summary>
    /// An Oracle INTERVAL DAY TO SECOND data type (Oracle 9i or later) that contains an interval of time in days, hours,
    /// minutes, and seconds, and has a fixed size of 11 bytes. Use the .NET TimeSpan or OracleClient OracleTimeSpan data type
    /// in Value.
    /// </summary>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "INTERVAL DAY TO SECOND" })]
    IntervalDayToSecond = 7,

    /// <summary>
    /// An Oracle INTERVAL YEAR TO MONTH data type (Oracle 9i or later) that contains an interval of time in years and months,
    /// and has a fixed size of 5 bytes. Use the .NET Int32 or OracleClient OracleMonthSpan data type in Value.
    /// </summary>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "INTERVAL YEAR TO MONTH" })]
    IntervalYearToMonth = 8,

    /// <summary>
    /// An Oracle LONGRAW data type that contains variable-length binary data with a maximum size of 2 gigabytes. Use the .NET
    /// Byte[] or OracleClient OracleBinary data type in Value.
    /// When you update a column with the LONG RAW data type, an exception is thrown when you enter a value of null in the
    /// column. The Oracle LONG RAW data type is a deprecated type in Oracle version 8.0. To avoid this error, use the BLOB
    /// data type instead of LONG RAW.
    /// </summary>
    [DatabaseType(DbType.Binary, DatabaseTypeNames = new[] { "LONG RAW" })]
    LongRaw = 9,

    /// <summary>
    /// An Oracle LONG data type that contains a variable-length character string with a maximum size of 2 gigabytes. Use the
    /// .NET String or OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.AnsiString, DatabaseTypeNames = new[] { "LONG" })]
    LongVarChar = 10,

    /// <summary>
    /// An Oracle NCHAR data type that contains fixed-length character string to be stored in the national character set of the
    /// database, with a maximum size of 2,000 bytes (not characters) when stored in the database. The size of the value
    /// depends on the national character set of the database. See your Oracle documentation for more information. Use the .NET
    /// String or OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.StringFixedLength, DatabaseTypeNames = new[] { nameof(NChar), "NATIONAL CHARACTER", "NATIONAL CHAR" })]
    NChar = 11,

    /// <summary>
    /// An Oracle NCLOB data type that contains character data to be stored in the national character set of the database, with
    /// a maximum size of 4 gigabytes (not characters) when stored in the database. The size of the value depends on the
    /// national character set of the database. See your Oracle documentation for more information. Use the .NET String or
    /// OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.String)] NClob = 12,

    /// <summary>
    /// An Oracle NUMBER data type that contains variable-length numeric data with a maximum precision and scale of 38. This
    /// maps to Decimal. To bind an Oracle NUMBER that exceeds what MaxValue can contain, either use an OracleNumber data type,
    /// or use a String parameter and the Oracle TO_NUMBER or TO_CHAR conversion functions for input and output parameters
    /// respectively. Use the .NET Decimal or OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.Decimal, DatabaseTypeNames = new[] { nameof(Number), "DECIMAL", "REAL" })]
    Number = 13,

    /// <summary>
    /// An Oracle NVARCHAR2 data type that contains a variable-length character string stored in the national character set of
    /// the database, with a maximum size of 4,000 bytes (not characters) when stored in the database. The size of the value
    /// depends on the national character set of the database. See your Oracle documentation for more information. Use the .NET
    /// String or OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.String, DatabaseTypeNames = new[] { "NVARCHAR2" })]
    NVarChar = 14,

    /// <summary>
    /// An Oracle RAW data type that contains variable-length binary data with a maximum size of 2,000 bytes. Use the .NET
    /// Byte[] or OracleClient OracleBinary data type in Value.
    /// </summary>
    [DatabaseType(DbType.Binary)] Raw = 15,

    /// <summary>
    /// The base64 string representation of an Oracle ROWID data type. Use the .NET String or OracleClient OracleString data
    /// type in Value.
    /// </summary>
    [DatabaseType(DbType.String)] RowId = 16,

    /// <summary>
    /// An integral type representing signed 8 bit integers with values between -128 and 127. This is not a native Oracle data
    /// type, but is provided to improve performance when binding input parameters. Use the .NET SByte data type in Value.
    /// </summary>
    [DatabaseType(DbType.SByte, AliasFor = Number)]
    SByte = 26,

    /// <summary>
    /// An Oracle TIMESTAMP (Oracle 9i or later) that contains date and time (including seconds), and ranges in size from 7 to
    /// 11 bytes. Use the .NET DateTime or OracleClient OracleDateTime data type in Value.
    /// </summary>
    [DatabaseType(DbType.DateTime)] Timestamp = 18,

    /// <summary>
    /// An Oracle TIMESTAMP WITH LOCAL TIMEZONE (Oracle 9i or later) that contains date, time, and a reference to the original
    /// time zone, and ranges in size from 7 to 11 bytes. Use the .NET DateTime or OracleClient OracleDateTime data type in
    /// Value.
    /// </summary>
    [DatabaseType(DbType.DateTime, DatabaseTypeNames = new[] { "TIMESTAMP WITH LOCAL TIME ZONE" })]
    TimestampLocal = 19,

    /// <summary>
    /// An Oracle TIMESTAMP WITH TIMEZONE (Oracle 9i or later) that contains date, time, and a specified time zone, and has a
    /// fixed size of 13 bytes. Use the .NET DateTime or OracleClient OracleDateTime data type in Value.
    /// </summary>
    [DatabaseType(DbType.DateTime, DatabaseTypeNames = new[] { "TIMESTAMP WITH TIME ZONE" })]
    // ReSharper disable once InconsistentNaming
    TimestampWithTZ = 20,

    /// <summary>
    /// An integral type representing unsigned 16-bit integers with values between 0 and 65535. This is not a native Oracle
    /// data type, but is provided to improve performance when binding input parameters. For information about conversion of
    /// Oracle numeric values to common language runtime (CLR) data types, see OracleNumber. Use the .NET UInt16 or
    /// OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.UInt16, AliasFor = Number)]
    UInt16 = 24,

    /// <summary>
    /// An integral type representing unsigned 32-bit integers with values between 0 and 4294967295. This is not a native
    /// Oracle data type, but is provided to improve performance when binding input parameters. For information about
    /// conversion of Oracle numeric values to common language runtime (CLR) data types, see OracleNumber. Use the .NET UInt32
    /// or OracleClient OracleNumber data type in Value.
    /// </summary>
    [DatabaseType(DbType.UInt32, AliasFor = Number)]
    UInt32 = 25,

    /// <summary>
    /// An Oracle VARCHAR2 data type that contains a variable-length character string with a maximum size of 4,000 bytes. Use
    /// the .NET String or OracleClient OracleString data type in Value.
    /// </summary>
    [DatabaseType(DbType.AnsiString, DatabaseTypeNames = new[] { "VARCHAR2" })]
    VarChar = 22,
}

public static class SqlOracleTypeExtensions
{
    public static OracleSqlType ToOracleSqlType(this DbType dbType) =>
        dbType switch
        {
            DbType.AnsiString => OracleSqlType.Clob,
            DbType.Binary => OracleSqlType.Blob,
            DbType.Byte => OracleSqlType.Byte, // TODO: Check on this
            DbType.Boolean => OracleSqlType.VarChar,
            DbType.Currency => OracleSqlType.Number,
            DbType.Date => OracleSqlType.DateTime,
            DbType.DateTime => OracleSqlType.DateTime,
            DbType.Decimal => OracleSqlType.Number,
            DbType.Double => OracleSqlType.Double,
            DbType.Guid => OracleSqlType.VarChar,
            DbType.Int16 => OracleSqlType.Int16,
            DbType.Int32 => OracleSqlType.Int32,
            DbType.Int64 => OracleSqlType.Number,
            DbType.Object => OracleSqlType.NClob,
            DbType.SByte => OracleSqlType.SByte, // TODO: Check on this
            DbType.Single => OracleSqlType.Float,
            DbType.String => OracleSqlType.NClob,
            DbType.Time => OracleSqlType.DateTime,
            DbType.UInt16 => OracleSqlType.UInt16,
            DbType.UInt32 => OracleSqlType.UInt32,
            DbType.UInt64 => OracleSqlType.Number,
            DbType.VarNumeric => OracleSqlType.Number,
            DbType.AnsiStringFixedLength => OracleSqlType.Char,
            DbType.StringFixedLength => OracleSqlType.NChar,
            DbType.Xml => OracleSqlType.NClob,
            DbType.DateTime2 => OracleSqlType.DateTime,
            DbType.DateTimeOffset => OracleSqlType.DateTime,
            _ => throw new NotImplementedException(),
        };
}
