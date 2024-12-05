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
