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

namespace MaxRunSoftware.Utilities.Common;

public class LoggerProvider
{
    public static ILoggerProvider Create(LoggerDelegateIsEnabled isEnabled, LoggerDelegateEvent eventHandler) =>
        new LoggerProviderDelegating(isEnabled, eventHandler);

    public static ILoggerProvider Create(Func<string, ILogger> loggerFactory) =>
        new LoggerProviderFunc(category => loggerFactory(category));

    public static ILoggerProvider Create(LoggerProviderDelegate loggerFactory) =>
        new LoggerProviderFunc(loggerFactory);

    private sealed class LoggerProviderDelegating : ILoggerProvider
    {
        private readonly LoggerDelegateIsEnabled isEnabled;
        private readonly LoggerDelegateEvent eventHandler;
        public LoggerProviderDelegating(LoggerDelegateIsEnabled isEnabled, LoggerDelegateEvent eventHandler)
        {
            this.isEnabled = isEnabled;
            this.eventHandler = eventHandler;
        }
        public void Dispose() { }
        public ILogger CreateLogger(string categoryName) => new LoggerDelegating(categoryName, this);

        private sealed class LoggerDelegating : LoggerBase
        {
            private readonly LoggerProviderDelegating provider;
            public LoggerDelegating(string categoryName, LoggerProviderDelegating provider) : base(categoryName) => this.provider = provider;
            public override bool IsEnabled(LogLevel logLevel) => provider.isEnabled(CategoryName, logLevel);
            protected override void Log(LogEvent logEvent) => provider.eventHandler(logEvent);
        }
    }

    private sealed class LoggerProviderFunc : ILoggerProvider
    {
        private readonly LoggerProviderDelegate loggerFactory;
        public LoggerProviderFunc(LoggerProviderDelegate loggerFactory) => this.loggerFactory = loggerFactory;
        public void Dispose() { }
        public ILogger CreateLogger(string categoryName) => loggerFactory(categoryName);
    }
}
