using System.Collections.Concurrent;

namespace MaxRunSoftware.Utilities.Common;

public delegate string LoggerDelegateFormatter(Type stateType, object state, Exception? exception);

public delegate bool LoggerDelegateIsEnabled(string categoryName, LogLevel logLevel);

public delegate void LoggerDelegateEvent(LogEvent logEvent);

public delegate ILogger LoggerProviderDelegate(string categoryName);

public record LogScope(
    string CategoryName,
    LogState LogState,
    int Depth,
    ILogger Logger
);

public record LogEvent(
    string CategoryName,
    LogLevel LogLevel,
    DateTimeOffset Timestamp,
    EventId EventId,
    Type StateType,
    object? StateObject,
    LogState? State,
    Exception? Exception,
    LoggerDelegateFormatter Formatter,
    string Text,
    IImmutableStack<LogScope> Scopes
);


public class LogState(string message, IReadOnlyList<KeyValuePair<string, object?>> arguments)
{
    public string Message { get; } = message;
    public IReadOnlyList<KeyValuePair<string, object?>> Arguments { get; } = arguments;
    
    public static LogState? Create(object? state)
    {
        if (state == null) return null;
        var message = state.ToString();
        if (LoggerUtils.IsNullMessage(message)) return null;
        
        var list = new List<KeyValuePair<string, object?>>();
        if (state is string stateString)
        {
            list.Add(new(stateString, null));
        }
        else if (state is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item is KeyValuePair<string, object?> kvp)
                {
                    list.Add(new(kvp.Key, kvp.Value));
                }
            }
        }
        
        return new(message!, list.Count == 0 ? Array.Empty<KeyValuePair<string, object?>>() : list);
    }
}


public abstract class LoggerBase(string categoryName) : ILogger
{
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

    public string CategoryName { get; } = categoryName;
    
    #region ILogger

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var msg = formatter(state, exception);
        if (LoggerUtils.IsNullMessage(msg)) return;
        var scopesImmutable = GetScopesImmutable();
        var stateObj = LogState.Create(state);
        
        var logEvent = new LogEvent(
            CategoryName,
            logLevel, 
            DateTimeOffset.Now, 
            eventId,
            typeof(TState),
            state,
            stateObj,
            exception,
            FormatterDelegate,
            msg,
            scopesImmutable
        );
        Log(logEvent);
        return;
        
        string FormatterDelegate(Type _, object o, Exception? e) => formatter((TState)o, e);
    }

    public abstract bool IsEnabled(LogLevel logLevel);

    public virtual IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        var logState = LogState.Create(state);
        if (logState == null) return Util.CreateDisposable();
        var logScope = new LogScope(CategoryName, logState, Scopes.Count, this);
        var logScopeDisposable = new LogScopeDisposable(logScope, () => Scopes.TryPop(out _));
        Scopes.Push(logScopeDisposable);
        return logScopeDisposable;
    }

    #endregion ILogger

    protected abstract void Log(LogEvent logEvent);

    protected class LogScopeDisposable(LogScope logScope, Action onScopeEnd) : IDisposable
    {
        public LogScope LogScope { get; } = logScope;
        
        public void Dispose() => onScopeEnd();
    }
}

internal static class LoggerUtils
{
    public static bool IsNullMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return true;
        var messageTrimmed = message.Trim();
        if (messageTrimmed.Length == 0) return true;
        if (messageTrimmed.Length is 4 or 6 && messageTrimmed.Contains("null", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}
