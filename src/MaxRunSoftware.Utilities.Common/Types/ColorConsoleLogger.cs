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

using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// https://github.com/dotnet/docs/tree/main/docs/core/extensions/snippets/configuration/console-custom-logging
/// https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
/// https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
/// </summary>
public sealed class ColorConsoleLogger : ILogger
{
    private static readonly ConcurrentDictionary<string, Tuple<TerminalColor?, TerminalColor?>> cacheColorNames = new(StringComparer.OrdinalIgnoreCase);
    private readonly string name;
    private readonly Func<ColorConsoleLoggerConfiguration> getCurrentConfig;
    
    public ColorConsoleLogger(string name, Func<ColorConsoleLoggerConfiguration> getCurrentConfig)
    {
        this.name = name;
        this.getCurrentConfig = getCurrentConfig;
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => getCurrentConfig().Colors.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var config = getCurrentConfig();
        if (config.EventId != 0 && config.EventId != eventId.Id) return;
        
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = config.Colors[logLevel][0].Value;
        Console.Write($"[{eventId.Id,2}: {logLevel,-12}]");

        Console.ForegroundColor = originalColor;
        Console.Write($"     {name} - ");

        Console.ForegroundColor = config.Colors[logLevel][1].Value;
        Console.Write($"{formatter(state, exception)}");
            
        Console.ForegroundColor = originalColor;
        Console.WriteLine();
        
    }
}

public sealed class ColorConsoleLoggerConfiguration
{
    public int EventId { get; set; }
    
    public Dictionary<LogLevel, ConsoleColor?[]> Colors { get; set; } = Constant.LogLevel_ConsoleColor
        .ToDictionary(o => o.Key, o => new ConsoleColor?[] { o.Value.Item1, o.Value.Item2, });
}

[UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private ColorConsoleLoggerConfiguration _currentConfig;

    private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public ColorConsoleLoggerProvider(IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new(name, GetCurrentConfig));

    private ColorConsoleLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}

public static class ColorConsoleLoggerExtensions
{
    public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<ColorConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(builder.Services);

        return builder;
    }

    public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder, Action<ColorConsoleLoggerConfiguration> configure)
    {
        builder.AddColorConsoleLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
