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

using System.Numerics;

namespace MaxRunSoftware.Utilities.Common;

public static class DataReaderResultRowExtensions
{
    public static object? GetObject(this DataReaderResultRow row, int index) => row[index];

    public static object? GetObject(this DataReaderResultRow row, DataReaderResultRowCollection rowCollection, string name) => row[rowCollection.Result.Columns[name].Index];

    public static object? GetObject(this DataReaderResultRow row, DataReaderResultColumn column) => row[column.Index];

    public static string? GetString(this DataReaderResultRow row, int index) => GetObject(row, index).ToStringGuessFormat();

    public static string? GetString(this DataReaderResultRow row, DataReaderResultRowCollection rowCollection, string name) => GetObject(row, rowCollection, name).ToStringGuessFormat();

    public static string? GetString(this DataReaderResultRow row, DataReaderResultColumn column) => GetObject(row, column).ToStringGuessFormat();

    public static T? Get<T>(this DataReaderResultRow row, int index) => GetConvert<T>(GetObject(row, index));

    public static T? Get<T>(this DataReaderResultRow row, DataReaderResultRowCollection rowCollection, string name) => GetConvert<T>(GetObject(row, rowCollection, name));

    public static T? Get<T>(this DataReaderResultRow row, DataReaderResultColumn column) => GetConvert<T>(GetObject(row, column));

    private static T? GetConvert<T>(object? o)
    {
        if (o == null) return default;
        var returnType = typeof(T);

        if (returnType == typeof(object)) return (T)o;

        if (returnType == typeof(byte[])) return (T)o;

        if (returnType == typeof(string)) return (T?)(object?)o.ToStringGuessFormat();

        if (returnType == typeof(char[])) return (T?)(object?)o.ToStringGuessFormat();

        var os = o.ToString().TrimOrNull();
        if (os == null) return default;

        if (CONVERTERS.TryGetValue(returnType, out var converter)) return (T?)converter(os);

        return (T)o;
    }


    private static readonly Dictionary<Type, Func<string, object?>> CONVERTERS = CreateConverters();

    private static Dictionary<Type, Func<string, object?>> CreateConverters()
    {
        var d = new Dictionary<Type, Func<string, object?>>();

        d.Add(typeof(bool), o => o.ToBool());
        d.Add(typeof(bool?), o => o.ToBoolNullable());

        d.Add(typeof(byte), o => o.ToByte());
        d.Add(typeof(byte?), o => o.ToByteNullable());

        d.AddRange(o => o.ToSByte(), typeof(sbyte), typeof(sbyte?));
        d.Add(typeof(byte), o => o.ToByte());
        d.Add(typeof(byte?), o => o.ToByteNullable());

        d.AddRange(o => o.ToShort(), typeof(short), typeof(short?));
        d.AddRange(o => o.ToUShort(), typeof(ushort), typeof(ushort?));

        d.AddRange(o => o.ToInt(), typeof(int), typeof(int?));
        d.AddRange(o => o.ToUInt(), typeof(uint), typeof(uint?));

        d.AddRange(o => o.ToLong(), typeof(long), typeof(long?));
        d.AddRange(o => o.ToULong(), typeof(ulong), typeof(ulong?));

        d.AddRange(o => o.ToFloat(), typeof(float), typeof(float?));
        d.AddRange(o => o.ToDouble(), typeof(double), typeof(double?));
        d.AddRange(o => o.ToDecimal(), typeof(decimal), typeof(decimal?));

        d.AddRange(o => BigInteger.Parse(o), typeof(BigInteger), typeof(BigInteger?));

        d.AddRange(o => o[0], typeof(char), typeof(char?));

        d.AddRange(o => o.ToGuid(), typeof(Guid), typeof(Guid?));

        return new(d);
    }
}
