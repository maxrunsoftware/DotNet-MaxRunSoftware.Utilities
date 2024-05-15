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

using System.Net;
using System.Numerics;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    /// <summary>
    /// Map of DotNet types to DbType
    /// </summary>
    public static readonly FrozenDictionary<Type, DbType> Type_DbType = new[]
    {
        (typeof(bool), DbType.Boolean),
        (typeof(bool?), DbType.Boolean),
        
        (typeof(byte), DbType.Byte),
        (typeof(byte?), DbType.Byte),
        (typeof(sbyte), DbType.SByte),
        (typeof(sbyte?), DbType.SByte),
        
        (typeof(short), DbType.Int16),
        (typeof(short?), DbType.Int16),
        (typeof(ushort), DbType.UInt16),
        (typeof(ushort?), DbType.UInt16),
        
        (typeof(char), DbType.StringFixedLength),
        (typeof(char?), DbType.StringFixedLength),
        (typeof(char[]), DbType.StringFixedLength),
        
        (typeof(int), DbType.Int32),
        (typeof(int?), DbType.Int32),
        (typeof(uint), DbType.UInt32),
        (typeof(uint?), DbType.UInt32),
        
        (typeof(long), DbType.Int64),
        (typeof(long?), DbType.Int64),
        (typeof(ulong), DbType.UInt64),
        (typeof(ulong?), DbType.UInt64),
        
        (typeof(float), DbType.Single),
        (typeof(float?), DbType.Single),
        (typeof(double), DbType.Double),
        (typeof(double?), DbType.Double),
        (typeof(decimal), DbType.Decimal),
        (typeof(decimal?), DbType.Decimal),
        
        (typeof(byte[]), DbType.Binary),
        
        (typeof(Guid), DbType.Guid),
        (typeof(Guid?), DbType.Guid),
        
        (typeof(string), DbType.String),
        
        (typeof(IPAddress), DbType.String),
        (typeof(Uri), DbType.String),
        
        (typeof(BigInteger), DbType.Decimal),
        (typeof(BigInteger?), DbType.Decimal),
        
        (typeof(DateTime), DbType.DateTime),
        (typeof(DateTime?), DbType.DateTime),
        (typeof(DateTimeOffset), DbType.DateTimeOffset),
        (typeof(DateTimeOffset?), DbType.DateTimeOffset),
        
        (typeof(object), DbType.Object),
    }.ConstantToFrozenDictionaryTry();
    
    
    /// <summary>
    /// Map of DbType to DotNet types
    /// </summary>
    public static readonly FrozenDictionary<DbType, Type> DbType_Type = new[]
    {
        (DbType.AnsiString, typeof(string)),
        (DbType.AnsiStringFixedLength, typeof(char[])),
        (DbType.Binary, typeof(byte[])),
        (DbType.Boolean, typeof(bool)),
        (DbType.Byte, typeof(byte)),
        (DbType.Currency, typeof(decimal)),
        (DbType.Date, typeof(DateTime)),
        (DbType.DateTime, typeof(DateTime)),
        (DbType.DateTime2, typeof(DateTime)),
        (DbType.DateTimeOffset, typeof(DateTimeOffset)),
        (DbType.Decimal, typeof(decimal)),
        (DbType.Double, typeof(double)),
        (DbType.Guid, typeof(Guid)),
        (DbType.Int16, typeof(short)),
        (DbType.Int32, typeof(int)),
        (DbType.Int64, typeof(long)),
        (DbType.Object, typeof(object)),
        (DbType.SByte, typeof(sbyte)),
        (DbType.Single, typeof(float)),
        (DbType.String, typeof(string)),
        (DbType.StringFixedLength, typeof(char[])),
        (DbType.Time, typeof(DateTime)),
        (DbType.UInt16, typeof(ushort)),
        (DbType.UInt32, typeof(uint)),
        (DbType.UInt64, typeof(ulong)),
        (DbType.VarNumeric, typeof(decimal)),
        (DbType.Xml, typeof(string)),
    }.ConstantToFrozenDictionaryTry();
    
    
    public static readonly FrozenSet<DbType> DbTypes_Numeric = new[] {
        DbType.Byte,
        DbType.Currency,
        DbType.Decimal,
        DbType.Double,
        DbType.Int16,
        DbType.Int32,
        DbType.Int64,
        DbType.SByte,
        DbType.Single,
        DbType.UInt16,
        DbType.UInt32,
        DbType.UInt64,
        DbType.VarNumeric
    }.ToFrozenSet();


    public static readonly FrozenSet<DbType> DbTypes_String = new[] {
        DbType.AnsiString,
        DbType.AnsiStringFixedLength,
        DbType.String,
        DbType.StringFixedLength,
        DbType.Xml
    }.ToFrozenSet();

    
    public static readonly FrozenSet<DbType> DbTypes_DateTime = new[] {
        DbType.Date,
        DbType.DateTime,
        DbType.DateTime2,
        DbType.DateTimeOffset,
        DbType.Time
    }.ToFrozenSet();
}
