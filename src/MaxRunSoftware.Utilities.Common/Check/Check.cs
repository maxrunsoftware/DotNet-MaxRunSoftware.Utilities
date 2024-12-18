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

using System.Runtime.CompilerServices;

namespace MaxRunSoftware.Utilities.Common;

public enum CheckType { Argument, Variable, Property, Field }

public static class Check
{
    private static volatile string defaultUndefinedName = "!UNDEFINED!";
    public static string DefaultUndefinedName { get => defaultUndefinedName; set => defaultUndefinedName = value.TrimOrNull() ?? throw new ArgumentNullException(nameof(value)); }

    private static volatile CheckType defaultCheckType = CheckType.Argument;
    public static CheckType DefaultCheckType { get => defaultCheckType; set => defaultCheckType = value; }

    private static volatile bool defaultUseArgumentExpressionForName;
    public static bool DefaultUseArgumentExpressionForName { get => defaultUseArgumentExpressionForName; set => defaultUseArgumentExpressionForName = value; }


    /// <summary>
    /// Checks if value is null, and if it is throw a <see cref="CheckNotNullException" />.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression"><i>DO NOT PROVIDE, COMPILER GENERATED</i></param>
    /// <returns>The value</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    [ContractAnnotation("obj: null => halt")]
    public static T CheckNotNull<T>(
        [NoEnumeration] this T? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) where T : class => obj ?? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);


    /// <summary>
    /// Checks if value is null, and if it is throw a <see cref="CheckNotNullException" />.
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The value</returns>
    [ContractAnnotation("obj: null => halt")]
    public static T CheckNotNull<T>(
        [NoEnumeration] this T? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) where T : struct => obj ?? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);


    /// <summary>
    /// Trims a string and checks if the length is 0, and if it is throw a <see cref="CheckNotNullException" />.
    /// </summary>
    /// <param name="obj">The string to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The trimmed string</returns>
    [ContractAnnotation("obj: null => halt")]
    public static string CheckNotNullTrimmed(
        [NoEnumeration] this string? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj.TrimOrNull() ?? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);


    /// <summary>
    /// Checks if value is null or length is 0, and if it is throw a <see cref="CheckNotNullException" /> or
    /// <see cref="CheckNotEmptyException" />.
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    /// <typeparam name="TItem">Collection item type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The value</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    [ContractAnnotation("obj: null => halt")]
    public static T CheckNotEmpty<T, TItem>(
        this T? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) where T : IReadOnlyCollection<TItem?> => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : obj.Count == 0
            ? throw CheckNotEmptyException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if value is null or length is 0, and if it is throw a <see cref="CheckNotNullException" /> or
    /// <see cref="CheckNotEmptyException" />.
    /// </summary>
    /// <typeparam name="T">Array type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The value</returns>
    //[return: System.Diagnostics.CodeAnalysis.NotNull]
    [ContractAnnotation("obj: null => halt")]
    public static T?[] CheckNotEmpty<T>(
        this T?[]? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : obj.Length == 0
            ? throw CheckNotEmptyException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if value is not null and greater than or equal to a value, and if it is not throw a
    /// <see cref="CheckNotNullException" /> or
    /// <see cref="CheckMinException" />.
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="minInclusive">Value must be greater than or equal to this</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The value</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    [ContractAnnotation("obj: null => halt")]
    public static T CheckMin<T>(
        this T? obj,
        T minInclusive,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) where T : IComparable<T?> => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : obj.CompareTo(minInclusive) < 0
            ? throw CheckMinException.Create(type, name, parent, obj, minInclusive, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if value is not null and less than a value, and if it is not throw a <see cref="CheckNotNullException" /> or
    /// <see cref="CheckMaxException" />.
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    /// <param name="obj">The value to check</param>
    /// <param name="maxInclusive">Value must be greater than or equal to this</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The value</returns>
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    [ContractAnnotation("obj: null => halt")]
    public static T CheckMax<T>(
        this T? obj,
        T maxInclusive,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) where T : IComparable<T?> => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : obj.CompareTo(maxInclusive) > 0
            ? throw CheckMaxException.Create(type, name, parent, obj, maxInclusive, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if path is not null and is a file that exists, and if it is not throw a <see cref="CheckNotNullException" /> or
    /// <see cref="CheckFileExistsException" />.
    /// </summary>
    /// <param name="obj">The path to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The file path</returns>
    [ContractAnnotation("obj: null => halt")]
    public static string CheckFileExists(
        this string? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : !File.Exists(obj)
            ? throw CheckFileExistsException.Create(type, name, parent, obj, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if path is not null and is a directory that exists, and if it is not throw a
    /// <see cref="CheckNotNullException" /> or
    /// <see cref="CheckDirectoryExistsException" />.
    /// </summary>
    /// <param name="obj">The path to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The directory path</returns>
    [ContractAnnotation("obj: null => halt")]
    public static string CheckDirectoryExists(
        this string? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : !Directory.Exists(obj)
            ? throw CheckDirectoryExistsException.Create(type, name, parent, obj, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if type is not null and is an enum, and if it is not throw a <see cref="CheckNotNullException" /> or
    /// <see cref="CheckIsEnumException" />.
    /// </summary>
    /// <param name="obj">The type to check</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The type which is an enum</returns>
    [ContractAnnotation("obj: null => halt")]
    public static Type CheckIsEnum(
        this Type? obj,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : !obj.IsEnum
            ? throw CheckIsEnumException.Create(type, name, parent, obj, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;


    /// <summary>
    /// Checks if type is not null and is assignable to a targetType, and if it is not throw a
    /// <see cref="CheckNotNullException" /> or
    /// <see cref="CheckIsAssignableToException" />.
    /// </summary>
    /// <param name="obj">The type to check</param>
    /// <param name="targetType">The target type to see if obj can be assigned to</param>
    /// <param name="name">The nameof argument</param>
    /// <param name="type">The value type</param>
    /// <param name="parent">The parent type of the property or field</param>
    /// <param name="callerFilePath">COMPILER GENERATED</param>
    /// <param name="callerLineNumber">COMPILER GENERATED</param>
    /// <param name="callerMemberName">COMPILER GENERATED</param>
    /// <param name="callerArgumentExpression">COMPILER GENERATED</param>
    /// <returns>The type which is assignable to the targetType</returns>
    [ContractAnnotation("obj: null => halt")]
    public static Type CheckIsAssignableTo(
        this Type? obj,
        Type targetType,
        string? name = null,
        CheckType? type = null,
        Type? parent = null,
        [CallerFilePath] string? callerFilePath = null,
        [CallerLineNumber] int? callerLineNumber = null,
        [CallerMemberName] string? callerMemberName = null,
        [CallerArgumentExpression("obj")] string? callerArgumentExpression = null
    ) => obj == null
        ? throw CheckNotNullException.Create(type, name, parent, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
        : !obj.IsAssignableTo(targetType)
            ? throw CheckIsAssignableToException.Create(type, name, parent, obj, targetType, callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression)
            : obj;
}

public static class CheckUtil
{
    internal static (CheckType Type, string Name, CallerInfo CallerInfo, string Message) CreateExceptionDetail(
        CheckType? type,
        string? name,
        Type? parent,
        string message,
        string? callerFilePath,
        int? callerLineNumber,
        string? callerMemberName,
        string? callerArgumentExpression
    )
    {
        type ??= Check.DefaultCheckType;
        var c = new CallerInfo(callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        var n = name.TrimOrNull();
        if (n == null)
        {
            //if (Check.DefaultUseArgumentExpressionForName) n = c.ArgumentExpression;
            n = c.ArgumentExpression;
            n ??= Check.DefaultUndefinedName;
        }

        var p = parent?.NameFormatted().TrimOrNull();

        var m = new StringBuilder();
        // {Argument} {'arg' in MyMethod} {cannot be null} {stuff}
        // {Variable} {'arg' in MyMethod} {cannot be null} {stuff}
        // {Property} {Parent.Prop} {cannot be null} {stuff}
        // {Field} {Parent.field} {cannot be null} {stuff}

        m.Append(type.ToString()!);
        m.Append(' ');
        if (type is CheckType.Field or CheckType.Property)
        {
            if (p != null) m.Append(p + ".");
            m.Append(n);
        }
        else
        {
            m.Append("'" + n + "'");
            if (p != null) m.Append(" in " + p);
        }

        m.Append(' ');
        m.Append(message);
        m.Append($"  Name: '{n}'");
        if (p != null) m.Append($", Parent: {p}");

        m.Append(", " + c.GetType().Name + ": ");
        if (c.MemberName != null) m.Append($" {c.MemberName}");
        if (c.ArgumentExpression != null) m.Append($" {c.ArgumentExpression}");
        if (c.FilePath != null) m.Append($" {c.FilePath}");
        if (c.LineNumber != null) m.Append($" [{c.LineNumber.Value}]");

        return ((CheckType)type, n, c, m.ToString());
    }
}
