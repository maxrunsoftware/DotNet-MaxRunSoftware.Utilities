// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using System.Runtime.CompilerServices;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsType
{
    public static bool IsAssignableTo<T>(this Type type) => type.IsAssignableTo(typeof(T));

    #region Property

    public static PropertyInfo FindProperty(this Type type, string name, BindingFlags flags = Constant.BindingFlag_Lookup_Default)
    {
        type.CheckNotNull(nameof(type));
        name.CheckNotNullTrimmed(nameof(name));

        var infos = type.GetProperties(flags);
        foreach (var sc in Constant.StringComparisons)
        {
            foreach (var info in infos)
            {
                if (string.Equals(name, info.Name, sc)) return info;
            }
        }

        // OK, so maybe we are trying to find Explicit declared property
        name = "." + name;
        foreach (var sc in Constant.StringComparisons)
        {
            var explicitList = infos.Where(info => info.Name.EndsWith(name, sc)).ToList();
            if (explicitList.Count == 1) return explicitList[0];
            if (explicitList.Count > 1) throw new MissingMemberException($"Found multiple explicit implementations of property {type.FullNameFormatted()}.{name} -> " + explicitList.Select(o => o.Name).OrderBy(o => o, StringComparer.OrdinalIgnoreCase).ToStringDelimited(" | "));
        }

        // Could not find property so throw exception
        throw new MissingMemberException($"Could not find property {type.FullNameFormatted()}.{name} -> " + infos.Select(o => o.Name).OrderBy(o => o, StringComparer.OrdinalIgnoreCase).ToStringDelimited(" | "));
    }

    public static object? GetPropertyValue(this Type type, string name, object instance, BindingFlags flags = Constant.BindingFlag_Lookup_Default) => type.FindProperty(name, flags).GetValue(instance);

    #endregion Property

    #region Field

    public static object? GetFieldValue(this Type type, string name, object instance, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
    {
        var info = type.GetField(name, flags);
        if (info == null) throw new MissingFieldException(type.FullNameFormatted(), name);
        return info.GetValue(instance);
    }

    #endregion Field

    public static bool IsNullable(this Type type, out Type underlyingType)
    {
        var t = Nullable.GetUnderlyingType(type);
        if (t == null)
        {
            underlyingType = type;
            return false;
        }

        underlyingType = t;
        return true;
    }

    public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) != null;

    /// <summary>Is this type a generic type</summary>
    /// <param name="type"></param>
    /// <returns>True if generic, otherwise False</returns>
    public static bool IsGeneric(this Type type) => type.IsGenericType && type.Name.Contains('`');

    //TODO: Figure out why IsGenericType isn't good enough and document (or remove) this condition
    public static Type AsNullable(this Type type)
    {
        // https://stackoverflow.com/a/23402284
        type.CheckNotNull(nameof(type));
        if (!type.IsValueType) return type;

        if (Nullable.GetUnderlyingType(type) != null) return type; // Already nullable

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) return type; // Already nullable

        return typeof(Nullable<>).MakeGenericType(type);
    }

    private static string NameFormatted(this Type type, bool fullName, bool fullNameForGenericArguments)
    {
        if (Constant.Type_PrimitiveAlias.TryGetValue(type, out var s)) return s;

        var name = fullName ? type.FullName ?? type.Name : type.Name;
        if (string.IsNullOrEmpty(name)) return name;

        if (!type.IsGeneric()) return name;

        // https://stackoverflow.com/a/25287378
        /*
            return string.Format(
                "{0}<{1}>",
                name.Substring(0, name.LastIndexOf("`", StringComparison.InvariantCulture)),
                string.Join(", ", type.GetGenericArguments().Select(FullNameFormatted)));
        */

        var sb = new StringBuilder();
        sb.Append(name.Substring(0, name.LastIndexOf("`", StringComparison.InvariantCulture)));
        sb.Append('<');
        sb.Append(type.GetGenericArguments().Select(o => NameFormatted(o, fullNameForGenericArguments, fullNameForGenericArguments)).ToStringDelimited(", "));
        sb.Append('>');
        return sb.ToString();
    }

    public static string FullNameFormatted(this Type type, bool fullNameForGenericArguments = true) => NameFormatted(type, true, fullNameForGenericArguments);

    public static string NameFormatted(this Type type, bool fullNameForGenericArguments = false) => NameFormatted(type, false, fullNameForGenericArguments);

    public static IEnumerable<Type> GetTypesOf<T>(this Assembly assembly, bool allowAbstract = false, bool allowInterface = false, bool requireNoArgConstructor = false, bool namespaceSystem = false)
    {
        foreach (var t in assembly.GetTypes())
        {
            if (t.Namespace == null) continue;

            if (t.Namespace.StartsWith("System.", StringComparison.OrdinalIgnoreCase) && namespaceSystem == false) continue;

            if (t.IsInterface && allowInterface == false) continue;

            if (t.IsAbstract && t.IsInterface == false && allowAbstract == false) continue;

            if (requireNoArgConstructor && t.GetConstructor(Type.EmptyTypes) == null) continue;

            if (typeof(T).IsAssignableFrom(t) == false) continue;

            yield return t;
        }
    }

    #region Equals

    // @formatter:off
    /*

    using System;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    public class Program
    {
	    public static void Main()
	    {
		    Console.WriteLine();
		    var example   = "public static bool Equals<T1, T2>(this Type type) => typeof(T1) == type || typeof(T2) == type;";
		    var template  = "public static bool Equals<#1>(this Type type) => #2;";

		    var template1 = "T#";
		    var template1Delimiter = ", ";
		    var template2 = "typeof(T#) == type";
		    var template2Delimiter = " || ";

		    for(int i=2; i<=16; i++)
		    {
			    var t1 = string.Join(template1Delimiter, Enumerable.Range(1, i).Select(i => template1.Replace("#",i.ToString()) ) );
			    var t2 = string.Join(template2Delimiter, Enumerable.Range(1, i).Select(i => template2.Replace("#",i.ToString()) ) );
			    var t = template;
			    t = t.Replace("#1", t1);
			    t = t.Replace("#2", t2);
			    Console.WriteLine(t);
		    }
		    Console.WriteLine();
	    }
    }

    */
    // @formatter:on

    // @formatter:off
    public static bool Equals<T1>(this Type type) => typeof(T1) == type;
    public static bool Equals<T1, T2>(this Type type) => typeof(T1) == type || typeof(T2) == type;
    public static bool Equals<T1, T2, T3>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type;
    public static bool Equals<T1, T2, T3, T4>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type;
    public static bool Equals<T1, T2, T3, T4, T5>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type || typeof(T12) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type || typeof(T12) == type || typeof(T13) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type || typeof(T12) == type || typeof(T13) == type || typeof(T14) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type || typeof(T12) == type || typeof(T13) == type || typeof(T14) == type || typeof(T15) == type;
    public static bool Equals<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Type type) => typeof(T1) == type || typeof(T2) == type || typeof(T3) == type || typeof(T4) == type || typeof(T5) == type || typeof(T6) == type || typeof(T7) == type || typeof(T8) == type || typeof(T9) == type || typeof(T10) == type || typeof(T11) == type || typeof(T12) == type || typeof(T13) == type || typeof(T14) == type || typeof(T15) == type || typeof(T16) == type;
    // @formatter:on

    #endregion Equals

    #region DefaultValue

    public static object? GetDefaultValue(this Type? type)
    {
        if (type == null) return null;

        if (type.IsPrimitive || type.IsValueType || type.IsEnum)
        {
            //return Expression.Lambda<Func<object>>(Expression.Convert(Expression.Default(type), typeof(object))).Compile()();
            return Activator.CreateInstance(type);
        }

        return null;
    }

    #endregion DefaultValue

    /*
    public static TAttribute[] GetCustomAttributes<TAttribute>(this FieldInfo field, bool inherit) where TAttribute : Attribute
    {
        var list = new List<TAttribute>();
        foreach (var attr in field.GetCustomAttributes(inherit))
        {
            if (attr is TAttribute a) list.Add(a);
        }
        return list.ToArray();
    }
    */

    public static TAttribute[] GetEnumItemAttributes<TAttribute>(this Type enumType, string enumItemName) where TAttribute : Attribute
    {
        enumType.CheckIsEnum(nameof(enumType));
        enumItemName = enumItemName.CheckNotNullTrimmed(nameof(enumItemName));

        foreach (var sc in Constant.StringComparers)
        {
            foreach (var name in enumType.GetEnumNames())
            {
                if (sc.Equals(name, enumItemName))
                {
                    var field = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
                    if (field != null) return field.GetCustomAttributes<TAttribute>(false).ToArray();
                }
            }
        }

        return Array.Empty<TAttribute>();
    }

    public static TAttribute? GetEnumItemAttribute<TAttribute>(this Type enumType, string enumItemName) where TAttribute : Attribute => GetEnumItemAttributes<TAttribute>(enumType, enumItemName).FirstOrDefault();

    private static readonly Dictionary<Type, Dictionary<string, object>> GET_ENUM_VALUE_CACHE = new();
    private static readonly object GET_ENUM_VALUE_CACHE_LOCK = new();

    public static object? GetEnumValue(this Type enumType, string enumItemName)
    {
        enumType.CheckIsEnum(nameof(enumType));
        enumItemName = enumItemName.CheckNotNullTrimmed(nameof(enumItemName));

        lock (GET_ENUM_VALUE_CACHE_LOCK)
        {
            if (!GET_ENUM_VALUE_CACHE.TryGetValue(enumType, out var enumItemDic))
            {
                enumItemDic = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                foreach (var enumValue in enumType.GetEnumValues())
                {
                    var enumValueString = enumValue.ToString();
                    if (enumValueString != null) enumItemDic[enumValueString] = enumValue;
                }

                GET_ENUM_VALUE_CACHE.Add(enumType, enumItemDic);
            }

            return enumItemDic.TryGetValue(enumItemName, out var enumObject) ? enumObject : null;
        }
    }

    /*
    private static Func<string, object> GetParseEnumDelegate(Type tEnum)
    {
        // https://stackoverflow.com/a/41594057
        var eValue = Expression.Parameter(typeof(string), "value"); // (String value)
        var tReturn = typeof(object);

        return
            Expression.Lambda<Func<string, object>>(
                Expression.Block(tReturn,
                    Expression.Convert( // We need to box the result (tEnum -> Object)
                        Expression.Switch(tEnum, eValue,
                            Expression.Block(tEnum,
                                Expression.Throw(Expression.New(typeof(Exception).GetConstructor(Type.EmptyTypes)!)),
                                Expression.Default(tEnum)
                            ),
                            null,
                            Enum.GetValues(tEnum).Cast<object>().Select(v => Expression.SwitchCase(
                                Expression.Constant(v),
                                Expression.Constant(v.ToString())
                            )).ToArray()
                        ), tReturn
                    )
                ), eValue
            ).Compile();
    }

    private static Func<string, TEnum> GetParseEnumDelegate<TEnum>()
    {
        // https://stackoverflow.com/a/41594057
        var eValue = Expression.Parameter(typeof(string), "value"); // (String value)
        var tEnum = typeof(TEnum);

        return
            Expression.Lambda<Func<string, TEnum>>(
                Expression.Block(tEnum,
                    Expression.Switch(tEnum, eValue,
                        Expression.Block(tEnum,
                            Expression.Throw(Expression.New(typeof(Exception).GetConstructor(Type.EmptyTypes))),
                            Expression.Default(tEnum)
                        ),
                        null,
                        Enum.GetValues(tEnum).Cast<object>().Select(v => Expression.SwitchCase(
                            Expression.Constant(v),
                            Expression.Constant(v.ToString())
                        )).ToArray()
                    )
                ), eValue
            ).Compile();
    }
    */

    /// <summary>
    /// https://stackoverflow.com/a/51441889
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsStatic(this PropertyInfo info) => info.GetAccessors(true).Any(x => x.IsStatic);

    public static Type[] BaseTypes(this Type type)
    {
        var list = new List<Type>();
        while (true)
        {
            var typeBase = type.BaseType;
            if (typeBase == null) break;
            if (typeBase == type) break; // Should not happen but just to be safe
            list.Add(typeBase);
            type = typeBase;
        }

        return list.ToArray();
    }

    public static bool HasNoArgConstructor(this Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) => type.GetConstructors(flags).Any(o => o.GetParameters().Length == 0);

    public static bool IsStatic(this Type type) => type.IsAbstract && type.IsSealed;

    public static bool IsAnonymous(this Type type)
    {
        if (!string.IsNullOrEmpty(type.Namespace)) return false;
        if (!type.Name.Contains("Anon")) return false;
        if (!type.Name.Contains("Type")) return false;
        if (!type.GetCustomAttributes(typeof(CompilerGeneratedAttribute)).Any()) return false;
        return true;
    }

    public static bool IsCompilerGenerated(this Type type) => type.GetCustomAttributes(typeof(CompilerGeneratedAttribute)).Any();

    public static string GetEnumNames(this Type enumType, string delimiter, bool isSorted = false) => isSorted ? Enum.GetNames(enumType).OrderByOrdinalIgnoreCaseThenOrdinal().ToStringDelimited(delimiter) : Enum.GetNames(enumType).ToStringDelimited(delimiter);

    public static bool IsRealType(this Type type)
    {
        if (type.IsGenericParameter) return false;
        if (type.IsAnonymous()) return false;
        if (type.IsCompilerGenerated()) return false;

        if (typeof(Delegate).IsAssignableFrom(type)) return false;

        var invalidPrefixes = new[] { "<>c__" };
        var invalidSuffixes = new[] { "<>c", "&", "[]" };

        var name = type.Name.TrimOrNull();
        if (name == null) return false;

        if (invalidPrefixes.Any(invalid => name.Equals(invalid) || name.StartsWith(invalid))) return false;
        if (invalidSuffixes.Any(invalid => name.Equals(invalid) || name.EndsWith(invalid))) return false;

        if (type.IsArray)
        {
            var arrayType = type.GetElementType();
            if (arrayType == null) return false;
            if (!IsRealType(arrayType)) return false;
        }

        return true;
    }

    #region GetMethod

    public static MethodInfo? GetMethod(Type type, string name, params Type[] parameters)
    {
        return type.GetMethod(name, parameters);
    }

    // @formatter:off
    /*

    using System;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    public class Program
    {
	    public static void Main()
	    {
		    Console.WriteLine();
		    var template = "public static MethodInfo? GetMethod<#>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<#>());";
		    for(int i=1; i<=64; i++)
		    {
			    var e = Enumerable.Range(1, i).Select(i => $"T{i}");
			    var v = string.Join(", ", e);
			    var t = template.Replace("#", v);
			    Console.WriteLine(t);
		    }
		    Console.WriteLine();
	    }
    }

    */
    // @formatter:off


    // @formatter:off
    public static MethodInfo? GetMethod<T1>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1>());
    public static MethodInfo? GetMethod<T1, T2>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2>());
    public static MethodInfo? GetMethod<T1, T2, T3>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63>());
    public static MethodInfo? GetMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63, T64>(Type type, string name) => GetMethod(type, name, Util.GenericTypes<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, T31, T32, T33, T34, T35, T36, T37, T38, T39, T40, T41, T42, T43, T44, T45, T46, T47, T48, T49, T50, T51, T52, T53, T54, T55, T56, T57, T58, T59, T60, T61, T62, T63, T64>());
    // @formatter:on

    #endregion GetMethod

}
