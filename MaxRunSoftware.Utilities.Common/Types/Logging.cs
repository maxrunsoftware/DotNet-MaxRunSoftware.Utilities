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

namespace MaxRunSoftware.Utilities.Common;

public interface ILogEventHandler
{
    void HandleEvent<TState>(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    );
}



public abstract class LogEventHandlerBase : ILogEventHandler
{
    public virtual void HandleEvent<TState>(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        HandleEvent(
            categoryName: categoryName,
            logLevel: logLevel,
            eventId: eventId,
            state: state,
            exception: exception,
            formattedStateException: formatter(state, exception)
        );
    }

    public abstract void HandleEvent(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        Exception? exception,
        string formattedStateException
    );
}

public delegate void LogEventDelegate(
    string categoryName,
    LogLevel logLevel,
    EventId eventId,
    object? state,
    Exception? exception,
    string formattedStateException
);

    public class LogEventHandlerWrapper : LogEventHandlerBase
    {
        private readonly LogEventDelegate func;
        public LogEventHandlerWrapper(LogEventDelegate func) => this.func = func;
        public override void HandleEvent(string categoryName, LogLevel logLevel, EventId eventId, object? state, Exception? exception, string formattedStateException) =>
            func(categoryName, logLevel, eventId, state, exception, formattedStateException);
    }

public class LogEventHandlerWrapperFunc : LogEventHandlerBase
{
    private readonly Func<LogEventDelegate> func;
    public LogEventHandlerWrapperFunc(Func<LogEventDelegate> func) => this.func = func;
    public override void HandleEvent(string categoryName, LogLevel logLevel, EventId eventId, object? state, Exception? exception, string formattedStateException) =>
        func()(categoryName, logLevel, eventId, state, exception, formattedStateException);
}
    public class LoggerProvider : ILoggerProvider
    {
        public ILogEventHandler EventHandler { get; set; }
        public LogLevel LogLevel { get; set; }
        public LoggerProvider(ILogEventHandler eventHandler, LogLevel logLevel)
        {
            EventHandler = eventHandler;
            LogLevel = logLevel;
        }
        public virtual void Dispose() { }
        public virtual ILogger CreateLogger(string categoryName) => new Logger(categoryName, EventHandler, LogLevel);
    }
public class LoggerProviderFunc : ILoggerProvider
{
    public Func<ILogEventHandler> EventHandler { get; set; }
    public Func<LogLevel> LogLevel { get; set; }
    public LoggerProviderFunc(Func<ILogEventHandler> eventHandler, Func<LogLevel> logLevel)
    {
        EventHandler = eventHandler;
        LogLevel = logLevel;
    }
    public virtual void Dispose() { }
    public virtual ILogger CreateLogger(string categoryName) => new LoggerFunc(() => categoryName, EventHandler, LogLevel);
}

    public class Logger : ILogger
    {
        public string CategoryName { get; set; }
        public ILogEventHandler EventHandler { get; set; }
        public LogLevel LogLevel { get; set; }
        public Logger(string categoryName, ILogEventHandler eventHandler, LogLevel logLevel)
        {
            CategoryName = categoryName;
            EventHandler = eventHandler;
            LogLevel = logLevel;
        }

        public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                EventHandler.HandleEvent(
                    categoryName: CategoryName,
                    logLevel: logLevel,
                    eventId: eventId,
                    state: state,
                    exception: exception,
                    formatter: formatter
                );
            }
        }

        public virtual bool IsEnabled(LogLevel logLevel) => LogLevel.Contains(logLevel);

        public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull => DisposableNoopInstance;

        // ReSharper disable once InconsistentNaming
        private static readonly DisposableNoop DisposableNoopInstance = new();
        private sealed class DisposableNoop : IDisposable { public void Dispose() { } }
    }


public class LoggerFunc : ILogger
{
    public Func<string> CategoryName { get; set; }
    public Func<ILogEventHandler> EventHandler { get; set; }
    public Func<LogLevel> LogLevel { get; set; }
    public LoggerFunc(Func<string> categoryName, Func<ILogEventHandler> eventHandler, Func<LogLevel> logLevel)
    {
        CategoryName = categoryName;
        EventHandler = eventHandler;
        LogLevel = logLevel;
    }

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            EventHandler().HandleEvent(
                categoryName: CategoryName(),
                logLevel: logLevel,
                eventId: eventId,
                state: state,
                exception: exception,
                formatter: formatter
            );
        }
    }

    public virtual bool IsEnabled(LogLevel logLevel) => LogLevel().Contains(logLevel);

    public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull => DisposableNoopInstance;

    // ReSharper disable once InconsistentNaming
    private static readonly DisposableNoop DisposableNoopInstance = new();
    private sealed class DisposableNoop : IDisposable { public void Dispose() { } }
}
