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

namespace MaxRunSoftware.Utilities.Data;

/// <summary>
/// Specifies SQL Server-specific data type of a field, property, for use in a
/// <see cref="T:System.Data.SqlClient.SqlParameter" />.
/// </summary>
[PublicAPI]
[DatabaseTypes(DatabaseAppType.MicrosoftSql)]
public enum MicrosoftSqlType
{
    /// <summary>
    /// <see cref="T:System.Int64" />. A 64-bit signed integer.
    /// </summary>
    [DatabaseType(DbType.Int64)] BigInt = 0,

    /// <summary>
    /// <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />. A fixed-length stream of binary data ranging
    /// between 1 and 8,000 bytes.
    /// </summary>
    [DatabaseType(DbType.Binary)] Binary = 1,

    /// <summary>
    /// <see cref="T:System.Boolean" />. An unsigned numeric value that can be 0, 1, or <see langword="null" />.
    /// </summary>
    [DatabaseType(DbType.Boolean)] Bit = 2,

    /// <summary>
    /// <see cref="T:System.String" />. A fixed-length stream of non-Unicode characters ranging between 1 and 8,000 characters.
    /// </summary>
    [DatabaseType(DbType.AnsiStringFixedLength)]
    Char = 3,

    /// <summary>
    /// <see cref="T:System.DateTime" />. Date and time data ranging in value from January 1, 1753 to December 31, 9999 to an
    /// accuracy of 3.33 milliseconds.
    /// </summary>
    [DatabaseType(DbType.DateTime)] DateTime = 4,

    /// <summary>
    /// <see cref="T:System.Decimal" />. A fixed precision and scale numeric value between -10 38 -1 and 10 38 -1.
    /// </summary>
    [DatabaseType(DbType.Decimal)] Decimal = 5,

    /// <summary>
    /// <see cref="T:System.Double" />. A floating point number within the range of -1.79E +308 through 1.79E +308.
    /// </summary>
    [DatabaseType(DbType.Double)] Float = 6,

    /// <summary>
    /// <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />. A variable-length stream of binary data ranging
    /// from 0 to 2 31 -1 (or 2,147,483,647) bytes.
    /// </summary>
    [DatabaseType(DbType.Binary)] Image = 7,

    /// <summary>
    /// <see cref="T:System.Int32" />. A 32-bit signed integer.
    /// </summary>
    [DatabaseType(DbType.Int32)] Int = 8,

    /// <summary>
    /// <see cref="T:System.Decimal" />. A currency value ranging from -2 63 (or -9,223,372,036,854,775,808) to 2 63 -1 (or
    /// +9,223,372,036,854,775,807) with an accuracy to a ten-thousandth of a currency unit.
    /// </summary>
    [DatabaseType(DbType.Currency)] Money = 9,

    /// <summary>
    /// <see cref="T:System.String" />. A fixed-length stream of Unicode characters ranging between 1 and 4,000 characters.
    /// </summary>
    [DatabaseType(DbType.StringFixedLength)] NChar = 10,

    /// <summary>
    /// <see cref="T:System.String" />. A variable-length stream of Unicode data with a maximum length of 2 30 - 1 (or
    /// 1,073,741,823) characters.
    /// </summary>
    [DatabaseType(DbType.String)] NText = 11,

    /// <summary>
    /// <see cref="T:System.String" />. A variable-length stream of Unicode characters ranging between 1 and 4,000 characters.
    /// Implicit conversion fails if the string is greater than 4,000 characters. Explicitly set the object when working with
    /// strings longer than 4,000 characters. Use <see cref="F:System.Data.SqlDbType.NVarChar" /> when the database column is
    /// <see langword="nvarchar(max)" />.
    /// </summary>
    [DatabaseType(DbType.String)] NVarChar = 12,

    /// <summary>
    /// <see cref="T:System.Single" />. A floating point number within the range of -3.40E +38 through 3.40E +38.
    /// </summary>
    [DatabaseType(DbType.Single)] Real = 13,

    /// <summary>
    /// <see cref="T:System.Guid" />. A globally unique identifier (or GUID).
    /// </summary>
    [DatabaseType(DbType.Guid)] UniqueIdentifier = 14,

    /// <summary>
    /// <see cref="T:System.DateTime" />. Date and time data ranging in value from January 1, 1900 to June 6, 2079 to an
    /// accuracy of one minute.
    /// </summary>
    [DatabaseType(DbType.DateTime)] SmallDateTime = 0xF,

    /// <summary>
    /// <see cref="T:System.Int16" />. A 16-bit signed integer.
    /// </summary>
    [DatabaseType(DbType.Int16)] SmallInt = 0x10,

    /// <summary>
    /// <see cref="T:System.Decimal" />. A currency value ranging from -214,748.3648 to +214,748.3647 with an accuracy to a
    /// ten-thousandth of a currency unit.
    /// </summary>
    [DatabaseType(DbType.Currency)] SmallMoney = 17,

    /// <summary>
    /// <see cref="T:System.String" />. A variable-length stream of non-Unicode data with a maximum length of 2 31 -1 (or
    /// 2,147,483,647) characters.
    /// </summary>
    [DatabaseType(DbType.AnsiString)] Text = 18,

    /// <summary>
    /// <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />. Automatically generated binary numbers, which are
    /// guaranteed to be unique within a database. <see langword="timestamp" /> is used typically as a mechanism for
    /// version-stamping table rows. The storage size is 8 bytes.
    /// </summary>
    [DatabaseType(DbType.Binary)] Timestamp = 19,

    /// <summary>
    /// <see cref="T:System.Byte" />. An 8-bit unsigned integer.
    /// </summary>
    [DatabaseType(DbType.Byte)] TinyInt = 20,

    /// <summary>
    /// <see cref="T:System.Array" /> of type <see cref="T:System.Byte" />. A variable-length stream of binary data ranging
    /// between 1 and 8,000 bytes. Implicit conversion fails if the byte array is greater than 8,000 bytes. Explicitly set the
    /// object when working with byte arrays larger than 8,000 bytes.
    /// </summary>
    [DatabaseType(DbType.Binary)] VarBinary = 21,

    /// <summary>
    /// <see cref="T:System.String" />. A variable-length stream of non-Unicode characters ranging between 1 and 8,000
    /// characters. Use <see cref="F:System.Data.SqlDbType.VarChar" /> when the database column is
    /// <see langword="varchar(max)" />.
    /// </summary>
    [DatabaseType(DbType.AnsiString)] VarChar = 22,

    /// <summary>
    /// <see cref="T:System.Object" />. A special data type that can contain numeric, string, binary, or date data as well as
    /// the SQL Server values Empty and Null, which is assumed if no other type is declared.
    /// </summary>
    [DatabaseType(DbType.Object)] Variant = 23,

    /// <summary>
    /// An XML value. Obtain the XML as a string using the
    /// <see cref="M:System.Data.SqlClient.SqlDataReader.GetValue(System.Int32)" /> method or
    /// <see cref="P:System.Data.SqlTypes.SqlXml.Value" /> property, or as an <see cref="T:System.Xml.XmlReader" /> by calling
    /// the <see cref="M:System.Data.SqlTypes.SqlXml.CreateReader" /> method.
    /// </summary>
    [DatabaseType(DbType.Xml)] Xml = 25,

    /// <summary>
    /// A SQL Server user-defined type (UDT).
    /// </summary>
    [DatabaseType(DbType.String)] Udt = 29,

    /// <summary>
    /// A special data type for specifying structured data contained in table-valued parameters.
    /// </summary>
    [DatabaseType(DbType.Object)] Structured = 30,

    /// <summary>
    /// Date data ranging in value from January 1,1 AD through December 31, 9999 AD.
    /// </summary>
    [DatabaseType(DbType.Date)] Date = 0x1F,

    /// <summary>
    /// Time data based on a 24-hour clock. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100
    /// nanoseconds. Corresponds to a SQL Server <see langword="time" /> value.
    /// </summary>
    [DatabaseType(DbType.Time)] Time = 0x20,

    /// <summary>
    /// Date and time data. Date value range is from January 1,1 AD through December 31, 9999 AD. Time value range is 00:00:00
    /// through 23:59:59.9999999 with an accuracy of 100 nanoseconds.
    /// </summary>
    [DatabaseType(DbType.DateTime2)] DateTime2 = 33,

    /// <summary>
    /// Date and time data with time zone awareness. Date value range is from January 1,1 AD through December 31, 9999 AD. Time
    /// value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds. Time zone value range is -14:00
    /// through +14:00.
    /// </summary>
    [DatabaseType(DbType.DateTimeOffset)] DateTimeOffset = 34,
}

public static class MicrosoftSqlTypeExtensions
{
    public static MicrosoftSqlType ToMicrosoftSqlType(this DbType dbType) =>
        dbType switch
        {
            DbType.AnsiString => MicrosoftSqlType.VarChar,
            DbType.Binary => MicrosoftSqlType.Binary,
            DbType.Byte => MicrosoftSqlType.TinyInt,
            DbType.Boolean => MicrosoftSqlType.Bit,
            DbType.Currency => MicrosoftSqlType.Money,
            DbType.Date => MicrosoftSqlType.Date,
            DbType.DateTime => MicrosoftSqlType.DateTime,
            DbType.Decimal => MicrosoftSqlType.Decimal,
            DbType.Double => MicrosoftSqlType.Float,
            DbType.Guid => MicrosoftSqlType.UniqueIdentifier,
            DbType.Int16 => MicrosoftSqlType.SmallInt,
            DbType.Int32 => MicrosoftSqlType.Int,
            DbType.Int64 => MicrosoftSqlType.BigInt,
            DbType.Object => MicrosoftSqlType.NVarChar,
            DbType.SByte => MicrosoftSqlType.SmallInt,
            DbType.Single => MicrosoftSqlType.Real,
            DbType.String => MicrosoftSqlType.NVarChar,
            DbType.Time => MicrosoftSqlType.Time,
            DbType.UInt16 => MicrosoftSqlType.Int,
            DbType.UInt32 => MicrosoftSqlType.BigInt,
            DbType.UInt64 => MicrosoftSqlType.Decimal,
            DbType.VarNumeric => MicrosoftSqlType.Decimal,
            DbType.AnsiStringFixedLength => MicrosoftSqlType.Char,
            DbType.StringFixedLength => MicrosoftSqlType.NChar,
            DbType.Xml => MicrosoftSqlType.Xml,
            DbType.DateTime2 => MicrosoftSqlType.DateTime2,
            DbType.DateTimeOffset => MicrosoftSqlType.DateTimeOffset,
            _ => throw new NotImplementedException(),
        };
}
