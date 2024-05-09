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

// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsLogging
{
    #region LogMethodTypeConverters
    
    private static readonly ConcurrentDictionary<Type, Func<object, string>> logMethodTypeConverters = new();
    
    public static void SetLogMethodTypeConverter(Type type, Func<object, string>? converter)
    {
        if (converter == null)
        {
            logMethodTypeConverters.Remove(type, out _);
        }
        else
        {
            logMethodTypeConverters[type] = converter;
        }
    }
    
    public static void SetLogMethodTypeConverter<T>(Func<T, string>? converter)
    {
        if (converter == null)
        {
            SetLogMethodTypeConverter(typeof(T), null);
        }
        else
        {
            logMethodTypeConverters[typeof(T)] = (object o) => converter((T)o);
        }
    }
    
    #endregion LogMethodTypeConverters

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
        
        if (logMethodTypeConverters.Count == 0)
        {
            argsFinal.AddRange(args);
        }
        else
        {
            foreach (var arg in args)
            {
                if (arg == null)
                {
                    argsFinal.Add(arg);
                }
                else
                {
                    var argType = arg.GetType();
                    if (logMethodTypeConverters.TryGetValue(argType, out var argConverter))
                    {
                        try
                        {
                            var s = argConverter(arg);
                            argsFinal.Add(s);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("Argument converter for type " + argType.FullNameFormatted() + " failed for value: " + arg);
                            Console.Error.WriteLine(e);
                            argsFinal.Add(arg);
                        }
                    }
                    else
                    {
                        argsFinal.Add(arg);
                    }
                }
            }
        }
        

        // ReSharper disable TemplateIsNotCompileTimeConstantProblem
#pragma warning disable CA2254
        log.Log(logLevel, exception, messageFinal.ToString(), argsFinal.ToArray());
#pragma warning restore CA2254
        // ReSharper restore TemplateIsNotCompileTimeConstantProblem
    }
    
    public static bool IsActiveFor(this LogLevel logEventLevel, LogLevel loggerLevel) => logEventLevel switch
    {
        LogLevel.Trace => loggerLevel is LogLevel.Trace,
        LogLevel.Debug => loggerLevel is LogLevel.Trace or LogLevel.Debug,
        LogLevel.Information => loggerLevel is LogLevel.Trace or LogLevel.Debug or LogLevel.Information,
        LogLevel.Warning => loggerLevel is LogLevel.Trace or LogLevel.Debug or LogLevel.Information or LogLevel.Warning,
        LogLevel.Error => loggerLevel is LogLevel.Trace or LogLevel.Debug or LogLevel.Information or LogLevel.Warning or LogLevel.Error,
        LogLevel.Critical => loggerLevel is LogLevel.Trace or LogLevel.Debug or LogLevel.Information or LogLevel.Warning or LogLevel.Error or LogLevel.Critical,
        LogLevel.None => false,
        _ => throw new ArgumentOutOfRangeException(nameof(loggerLevel), loggerLevel, null),
    };
    
    
    public static ILogger GetLogger(this IServiceProvider services, Type type)
    {
        var genericType = typeof(ILogger<>).MakeGenericType([type]);
        var loggerObj = services.GetRequiredService(genericType);
        var logger = (ILogger)loggerObj;
        return logger;
    }

}
