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

namespace MaxRunSoftware.Utilities.Common;

public abstract class LoggerBase : ILogger
{
    protected LoggerBase(string categoryName)
    {
        CategoryName = categoryName;
    }

    // ReSharper disable once InconsistentNaming
    private static readonly AsyncLocal<ConcurrentStack<LogScopeDisposable>> scopes = new();

    protected static ConcurrentStack<LogScopeDisposable> Scopes
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

    protected static IImmutableStack<LogScope> GetScopesImmutable() => ImmutableStack.Create(
        Scopes.ToArray()
            .Reverse()
            .Select(o => o.LogScope)
            .ToArray()
    );

    public string CategoryName { get; }

    #region ILogger

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var msg = formatter(state, exception);
        if (LogState.IsNullMessage(msg)) return;
        var scopesImmutable = GetScopesImmutable();
        var stateObj = LogState.Create(state);
        LoggerDelegateFormatter formatterDelegate = (_, o, e) => formatter((TState)o, e);

        var logEvent = new LogEvent(
            CategoryName,
            logLevel, eventId,
            typeof(TState),
            state,
            stateObj,
            exception,
            formatterDelegate,
            msg,
            scopesImmutable
        );
        Log(logEvent);
    }

    public abstract bool IsEnabled(LogLevel logLevel);

    public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        var logState = LogState.Create(state);
        if (logState == null) return DisposableNoopInstance;
        var logScope = new LogScope(CategoryName, logState, Scopes.Count, this);
        var logScopeDisposable = new LogScopeDisposable(logScope, () => Scopes.TryPop(out _));
        Scopes.Push(logScopeDisposable);
        return logScopeDisposable;
    }

    #endregion ILogger

    protected abstract void Log(LogEvent logEvent);

    protected class LogScopeDisposable : IDisposable
    {
        public LogScope LogScope { get; }
        private readonly Action onScopeEnd;

        public LogScopeDisposable(LogScope logScope, Action onScopeEnd)
        {
            LogScope = logScope;
            this.onScopeEnd = onScopeEnd;
        }

        public void Dispose() => onScopeEnd();
    }

    // ReSharper disable once InconsistentNaming
    protected static readonly DisposableNoop DisposableNoopInstance = new();

    protected sealed class DisposableNoop : IDisposable
    {
        public void Dispose() { }
    }
}
