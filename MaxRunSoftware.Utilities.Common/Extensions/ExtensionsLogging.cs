// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsLogging
{
    #region LogMethod Overloads
    // @formatter:off

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogTraceMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Trace, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogTraceMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Trace, callerInfo, exception, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogDebugMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Debug, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogDebugMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Debug, callerInfo, exception, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogInformationMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Information, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogInformationMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Information, callerInfo, exception, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogWarningMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Warning, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogWarningMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Warning, callerInfo, exception, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogErrorMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Error, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogErrorMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Error, callerInfo, exception, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogCriticalMethod(this ILogger log, CallerInfoMethod callerInfo, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Critical, callerInfo, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary><param name="log">The log to make logging statements to</param><param name="callerInfo">Method caller info</param><param name="exception">The exception that happened, if any</param><param name="message">Log message</param><param name="args">Log message arguments</param>
    public static void LogCriticalMethod(this ILogger log, CallerInfoMethod callerInfo, Exception? exception, [StructuredMessageTemplate] string message, params object?[] args) => log.LogMethod(LogLevel.Critical, callerInfo, exception, message, args);

    // @formatter:on
    #endregion LogMethod Overloads

    /// <summary>Called by methods that wish to also log their caller info.</summary>
    /// <param name="log">The log to make logging statements to</param>
    /// <param name="logLevel">The log level to use</param>
    /// <param name="callerInfo">Method caller info</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Log message arguments</param>
    public static void LogMethod(
        this ILogger log,
        LogLevel logLevel,
        CallerInfoMethod callerInfo,
        [StructuredMessageTemplate] string message,
        params object?[] args
    ) => log.LogMethod(logLevel, callerInfo, null, message, args);

    /// <summary>Called by methods that wish to also log their caller info.</summary>
    /// <param name="log">The log to make logging statements to</param>
    /// <param name="logLevel">The log level to use</param>
    /// <param name="callerInfo">Method caller info</param>
    /// <param name="exception">The exception that happened, if any</param>
    /// <param name="message">Log message</param>
    /// <param name="args">Log message arguments</param>
    public static void LogMethod(
        this ILogger log,
        LogLevel logLevel,
        CallerInfoMethod callerInfo,
        Exception? exception,
        [StructuredMessageTemplate] string message,
        params object?[] args
    )
    {
        var messageFinal = new StringBuilder();
        var argsFinal = new List<object?>();

        messageFinal.Append("{MemberArgName}");
        argsFinal.Add(callerInfo.MemberName);

        messageFinal.Append('(');
        for (var i = 0; i < callerInfo.Args.Length; i++)
        {
            if (i > 0) messageFinal.Append(", ");
            messageFinal.Append("{MemberArgName" + i + "}: {MemberArgValue" + i + "}");
            argsFinal.Add(callerInfo.Args[i].Name);
            argsFinal.Add(callerInfo.Args[i].Value.ToStringGuessFormat());
        }

        messageFinal.Append(')');

        messageFinal.Append(" [Line# {MemberLineNumber}]");
        argsFinal.Add(callerInfo.LineNumber);

        messageFinal.Append("  ");
        messageFinal.Append(message);
        argsFinal.AddRange(args);

        // ReSharper disable TemplateIsNotCompileTimeConstantProblem
#pragma warning disable CA2254
        log.Log(logLevel, exception, messageFinal.ToString(), argsFinal.ToArray());
#pragma warning restore CA2254
        // ReSharper restore TemplateIsNotCompileTimeConstantProblem
    }


    private static string GetName(Type type) => TypeNameHelper.GetTypeDisplayName(
        type,
        includeGenericParameters: false,
        nestedTypeDelimiter: '.'
    );

    public static ILogger CreateLogger<T>(this ILoggerProvider provider) => provider.CreateLogger(GetName(typeof(T)));
    public static ILogger CreateLogger(this ILoggerProvider provider, Type type) => provider.CreateLogger(GetName(type));

    public static ILogger CreateLogger<T>(this ILoggerFactory factory) => factory.CreateLogger(GetName(typeof(T)));
    public static ILogger CreateLogger(this ILoggerFactory factory, Type type) => factory.CreateLogger(GetName(type));

    public static ILogger CreateLoggerNullable<T>(this ILoggerProvider? provider) => provider != null ? provider.CreateLogger(GetName(typeof(T))) : Constant.LoggerNull;
    public static ILogger CreateLoggerNullable(this ILoggerProvider? provider, Type type) => provider != null ? provider.CreateLogger(GetName(type)) : Constant.LoggerNull;

    public static ILogger CreateLoggerNullable<T>(this ILoggerFactory? factory) => factory != null ? factory.CreateLogger(GetName(typeof(T))) : Constant.LoggerNull;
    public static ILogger CreateLoggerNullable(this ILoggerFactory? factory, Type type) => factory != null ? factory.CreateLogger(GetName(type)) : Constant.LoggerNull;

    public static bool Contains(this LogLevel level, LogLevel other)
    {
        if (level == other) return true;
        if (level == LogLevel.None) return false;
        if (other == LogLevel.None) return false;
        if (other >= level) return true;
        return false;
    }

    /// <summary>
    /// namespace Microsoft.Extensions.Internal
    /// </summary>
    private static class TypeNameHelper
    {
        private const char DefaultNestedTypeDelimiter = '+';

        private static readonly Dictionary<Type, string> _builtInTypeNames = new Dictionary<Type, string>
        {
            { typeof(void), "void" },
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" }
        };

        /// <summary>
        /// Pretty print a type name.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <param name="fullName"><c>true</c> to print a fully qualified name.</param>
        /// <param name="includeGenericParameterNames"><c>true</c> to include generic parameter names.</param>
        /// <param name="includeGenericParameters"><c>true</c> to include generic parameters.</param>
        /// <param name="nestedTypeDelimiter">Character to use as a delimiter in nested type names</param>
        /// <returns>The pretty printed type name.</returns>
        public static string GetTypeDisplayName(Type type, bool fullName = true, bool includeGenericParameterNames = false, bool includeGenericParameters = true, char nestedTypeDelimiter = DefaultNestedTypeDelimiter)
        {
            var builder = new StringBuilder();
            ProcessType(builder, type, new(fullName, includeGenericParameterNames, includeGenericParameters, nestedTypeDelimiter));
            return builder.ToString();
        }

        private static void ProcessType(StringBuilder builder, Type type, in DisplayNameOptions options)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                ProcessGenericType(builder, type, genericArguments, genericArguments.Length, options);
            }
            else if (type.IsArray)
            {
                ProcessArrayType(builder, type, options);
            }
            else if (_builtInTypeNames.TryGetValue(type, out var builtInName))
            {
                builder.Append(builtInName);
            }
            else if (type.IsGenericParameter)
            {
                if (options.IncludeGenericParameterNames)
                {
                    builder.Append(type.Name);
                }
            }
            else
            {
                var name = options.FullName ? type.FullName! : type.Name;
                builder.Append(name);

                if (options.NestedTypeDelimiter != DefaultNestedTypeDelimiter)
                {
                    builder.Replace(DefaultNestedTypeDelimiter, options.NestedTypeDelimiter, builder.Length - name.Length, name.Length);
                }
            }
        }

        private static void ProcessArrayType(StringBuilder builder, Type type, in DisplayNameOptions options)
        {
            var innerType = type;
            while (innerType.IsArray)
            {
                innerType = innerType.GetElementType()!;
            }

            ProcessType(builder, innerType, options);

            while (type.IsArray)
            {
                builder.Append('[');
                builder.Append(',', type.GetArrayRank() - 1);
                builder.Append(']');
                type = type.GetElementType()!;
            }
        }

        private static void ProcessGenericType(StringBuilder builder, Type type, Type[] genericArguments, int length, in DisplayNameOptions options)
        {
            var offset = 0;
            if (type.IsNested)
            {
                offset = type.DeclaringType!.GetGenericArguments().Length;
            }

            if (options.FullName)
            {
                if (type.IsNested)
                {
                    ProcessGenericType(builder, type.DeclaringType!, genericArguments, offset, options);
                    builder.Append(options.NestedTypeDelimiter);
                }
                else if (!string.IsNullOrEmpty(type.Namespace))
                {
                    builder.Append(type.Namespace);
                    builder.Append('.');
                }
            }

            var genericPartIndex = type.Name.IndexOf('`');
            if (genericPartIndex <= 0)
            {
                builder.Append(type.Name);
                return;
            }

            builder.Append(type.Name, 0, genericPartIndex);

            if (options.IncludeGenericParameters)
            {
                builder.Append('<');
                for (var i = offset; i < length; i++)
                {
                    ProcessType(builder, genericArguments[i], options);
                    if (i + 1 == length)
                    {
                        continue;
                    }

                    builder.Append(',');
                    if (options.IncludeGenericParameterNames || !genericArguments[i + 1].IsGenericParameter)
                    {
                        builder.Append(' ');
                    }
                }
                builder.Append('>');
            }
        }

        private readonly record struct DisplayNameOptions(bool FullName, bool IncludeGenericParameters, bool IncludeGenericParameterNames, char NestedTypeDelimiter);

    }


}
