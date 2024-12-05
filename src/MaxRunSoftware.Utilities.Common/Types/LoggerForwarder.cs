using System.Collections.Concurrent;

namespace MaxRunSoftware.Utilities.Common;


public interface ILoggerForwarderHandler
{
    public void AddLogEvent(LogEvent logEvent);
}

public class LoggerForwarderProvider(IEnumerable<ILoggerForwarderHandler> handlers) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, LoggerForwarder> loggers = new();

    public ICollection<ILoggerForwarderHandler> Handlers { get; set; } = handlers.ToList();
    public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    
    public virtual ILogger CreateLogger(string categoryName) => loggers.GetOrAdd(categoryName, name => new(name, this));
    
    protected virtual void Add(LogEvent logEvent)
    {
        // https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging/src/Logger.cs
        List<Exception>? exceptions = null;
        foreach (var handler in Handlers)
        {
            try
            {
                handler.AddLogEvent(logEvent);
            }
            catch (Exception ex)
            {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }
        
        if (exceptions != null && exceptions.Count > 0)
        {
            throw new AggregateException(message: "An error occurred while writing to " + nameof(Handlers) + ".", innerExceptions: exceptions);
        }
    }
    
    public virtual void Dispose()
    {
        loggers.Clear();
        Handlers = new List<ILoggerForwarderHandler>();
    }
    
    private class LoggerForwarder(string categoryName, LoggerForwarderProvider loggerForwarderProvider) : LoggerBase(categoryName)
    {
        public override bool IsEnabled(LogLevel logLevel) => logLevel.IsActiveFor(loggerForwarderProvider.LogLevel);
        
        protected override void Log(LogEvent logEvent) => loggerForwarderProvider.Add(logEvent);
    }
}
