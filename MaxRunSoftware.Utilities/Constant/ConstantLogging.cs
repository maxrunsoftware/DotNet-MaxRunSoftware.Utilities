// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MaxRunSoftware.Utilities;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    private static readonly object CONSTANT_LOGGING_LOCK = new();
    private static ILoggerFactory loggerFactory = new ConsoleLogFactory();

    public static ILoggerFactory LoggerFactory
    {
        get
        {
            lock (CONSTANT_LOGGING_LOCK)
            {
                return loggerFactory;
            }
        }
        set
        {
            lock (CONSTANT_LOGGING_LOCK)
            {
                loggerFactory = value;
                // TODO: Forward all previous ConsoleLogFactory logs to new ILoggerFactory if new ILoggerFactory is not a ConsoleLogFactory
            }

        }
    }

    public static readonly ILogger LoggerNull = NullLogger.Instance;

    private class ConsoleLogFactory : ILoggerFactory
    {
        public void Dispose() { }
        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);
        public void AddProvider(ILoggerProvider provider) => throw new NotImplementedException();

        private void Log<TState>(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString(DateTimeToStringFormat.ISO_8601));
            sb.Append($" [{logLevel}] {categoryName} ({eventId.Name}:{eventId.Id}) --> ");
            sb.Append(formatter(state, exception));
            Console.WriteLine(sb.ToString());
        }

        private class NoopDisposable : IDisposable { public void Dispose() { } }

        private class Logger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => logFactory.Log(categoryName, logLevel, eventId, state, exception, formatter);
            public bool IsEnabled(LogLevel logLevel) => true;
            public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();

            private readonly ConsoleLogFactory logFactory;
            private readonly string categoryName;
            public Logger(ConsoleLogFactory logFactory, string categoryName)
            {
                this.logFactory = logFactory;
                this.categoryName = categoryName;
            }
        }
    }


    public static ILogger CreateLogger(Type type)
    {
        return LoggerFactory.CreateLogger(type);
    }

    public static ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.CreateLogger<T>();
    }

}