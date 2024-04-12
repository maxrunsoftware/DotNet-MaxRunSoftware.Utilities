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

using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;

[DebuggerStepThrough]
public static class Extensions
{
    /// <summary>
    /// Attempts to convert the DotNet type to a DbType
    /// </summary>
    /// <param name="type">The DotNet type</param>
    /// <param name="defaultDbType">The default DbType</param>
    /// <returns>The DbType</returns>
    public static DbType GetDbType(this Type type, DbType defaultDbType) => Constant.Type_DbType.TryGetValue(type, out var dbType) ? dbType : defaultDbType;

    /// <summary>
    /// Attempts to convert the DotNet type to a DbType
    /// </summary>
    /// <param name="type">The DotNet type</param>
    /// <returns>The DbType</returns>
    public static DbType? GetDbType(this Type type) => Constant.Type_DbType.TryGetValue(type, out var dbType) ? dbType : null;

    /// <summary>
    /// Converts a DbType to a DotNet type
    /// </summary>
    /// <param name="dbType">The DbType</param>
    /// <returns>The DotNet type</returns>
    public static Type GetDotNetType(this DbType dbType) => Constant.DbType_Type.TryGetValue(dbType, out var type) ? type : typeof(string);
    
    /// <summary>
    /// Gets a typed value from a serialized object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializationInfo"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static T? GetValue<T>(this SerializationInfo serializationInfo, string name)
    {
        var o = serializationInfo.GetValue(name, typeof(T));
        if (o == null) return default;
        return (T)o;
    }

    #region IDisposable

    public static void DisposeSafely(this IDisposable? disposable, ILoggerFactory loggerProvider) => disposable.DisposeSafely(loggerProvider.CreateLogger(typeof(Extensions)));

    public static void DisposeSafely(this IDisposable? disposable, ILogger logger, LogLevel logLevel = LogLevel.Warning)
    {
        logger.CheckNotNull(nameof(logger));
        if (disposable == null) return;

        try
        {
            disposable.Dispose();
        }
        catch (Exception e)
        {
            logger.Log(logLevel, e, "Exception calling {Type}." + nameof(IDisposable.Dispose) + "()", disposable.GetType().FullNameFormatted());
        }
    }

    #endregion IDisposable

    #region Regex

    public static string[] MatchAll(this Regex regex, string input) => regex.Matches(input).Select(o => o.Value).ToArray();

    public static string? MatchFirst(this Regex regex, string input) => regex.Matches(input).Select(o => o.Value).FirstOrDefault();

    #endregion Regex

    #region RoundAwayFromZero

    public static decimal Round(this decimal value, MidpointRounding rounding, int decimalPlaces) => Math.Round(value, decimalPlaces, rounding);

    public static double Round(this double value, MidpointRounding rounding, int decimalPlaces) => Math.Round(value, decimalPlaces, rounding);

    public static double Round(this float value, MidpointRounding rounding, int decimalPlaces) => Math.Round(value, decimalPlaces, rounding);

    #endregion RoundAwayFromZero

    #region Equals Floating Point

    public static bool Equals(this double left, double right, byte digitsOfPrecision) => Math.Abs(left - right) < Math.Pow(10d, -1d * digitsOfPrecision);

    public static bool Equals(this float left, float right, byte digitsOfPrecision) => Math.Abs(left - right) < Math.Pow(10f, -1f * digitsOfPrecision);

    #endregion Equals Floating Point

    #region Conversion

    public static T ToNotNull<T>(T? nullable, T ifNull) where T : struct => nullable ?? ifNull;

    public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this Tuple<TKey, TValue> tuple) => new(tuple.Item1, tuple.Item2);

    public static ILookup<TKey, TElement> ToLookup<TKey, TEnumerable, TElement>(this IDictionary<TKey, TEnumerable> dictionary) where TEnumerable : IEnumerable<TElement> => dictionary.SelectMany(p => p.Value, Tuple.Create).ToLookup(p => p.Item1.Key, p => p.Item2);

    public static Tuple<TKey, TValue> ToTuple<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair) => Tuple.Create(keyValuePair.Key, keyValuePair.Value);

    #endregion Conversion

    public static Lazy<T> Initialize<T>(this Lazy<T> lazy)
    {
        if (lazy == null) throw new ArgumentNullException(nameof(lazy));
        _ = lazy.Value;
        return lazy;
    }
}
