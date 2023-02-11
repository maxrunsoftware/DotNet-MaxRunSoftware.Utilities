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

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;

namespace MaxRunSoftware.Utilities.Common;

public delegate string LoggerFormatterDelegate(Type stateType, object state, Exception? exception);

public delegate bool LoggerIsEnabled(string categoryName, LogLevel logLevel);

public delegate void LoggerEventHandler(LogEvent logEvent);

public class LoggerProvider
{
    public static ILoggerProvider Create(LoggerIsEnabled isEnabled, LoggerEventHandler eventHandler) =>
        new LoggerProviderDelegating(isEnabled, eventHandler);

    private sealed class LoggerProviderDelegating : ILoggerProvider
    {
        private readonly LoggerIsEnabled isEnabled;
        private readonly LoggerEventHandler eventHandler;
        public LoggerProviderDelegating(LoggerIsEnabled isEnabled, LoggerEventHandler eventHandler)
        {
            this.isEnabled = isEnabled;
            this.eventHandler = eventHandler;
        }
        public void Dispose() {}
        public ILogger CreateLogger(string categoryName) => new LoggerDelegating(categoryName, this);

        private sealed class LoggerDelegating : LoggerBase
        {
            private readonly LoggerProviderDelegating provider;
            public LoggerDelegating(string categoryName, LoggerProviderDelegating provider) : base(categoryName) => this.provider = provider;
            public override bool IsEnabled(LogLevel logLevel) => provider.isEnabled(CategoryName, logLevel);
            protected override void Log(LogEvent logEvent) => provider.eventHandler(logEvent);
        }
    }
}

public class LoggerProviderFunc : ILoggerProvider
{
    protected readonly Func<string, ILogger> loggerFactory;
    public LoggerProviderFunc(Func<string, ILogger> loggerFactory) => this.loggerFactory = loggerFactory;
    public virtual void Dispose() { }
    public virtual ILogger CreateLogger(string categoryName) => loggerFactory(categoryName);
}

public abstract class LoggerBase : ILogger
{
    private static readonly AsyncLocal<ConcurrentStack<ScopeWriter>> scopes = new();
    private static ConcurrentStack<ScopeWriter> Scopes
    {
        get
        {
            var stack = scopes.Value;
            if (stack == null)
            {
                stack = new();
                scopes.Value = stack;
            }
            return stack;
        }
    }

    public string CategoryName { get; }
    protected LoggerBase(string categoryName)
    {
        CategoryName = categoryName;
    }

    #region ILogger

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        this.BeginScope("", new object());
        var msg = formatter(state, exception);
        Log(new(CategoryName, logLevel, eventId, typeof(TState), state, exception, (_, o, e) => formatter((TState)o, e), msg));
    }

    public abstract bool IsEnabled(LogLevel logLevel);

    public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull => BeginScope(typeof(TState), state);

    #endregion ILogger

    protected abstract void Log(LogEvent logEvent);

    protected virtual IDisposable BeginScope(Type stateType, object state) => DisposableNoopInstance;

    // ReSharper disable once InconsistentNaming
    private static readonly DisposableNoop DisposableNoopInstance = new();
    private sealed class DisposableNoop : IDisposable { public void Dispose() { } }
}

public class LoggerFunc : LoggerBase
{
    protected readonly LoggerIsEnabled isEnabled;
    protected readonly LoggerEventHandler eventHandler;

    public LoggerFunc(string categoryName, LoggerIsEnabled isEnabled, LoggerEventHandler eventHandler) : base(categoryName)
    {
        this.isEnabled = isEnabled;
        this.eventHandler = eventHandler;
    }
    public override bool IsEnabled(LogLevel logLevel) => isEnabled(CategoryName, logLevel);
    protected override void Log(LogEvent logEvent) => eventHandler(logEvent);
}
