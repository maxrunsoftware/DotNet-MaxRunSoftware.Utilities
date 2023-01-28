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

namespace MaxRunSoftware.Utilities.Common.Tests;

public class TestBase
{
    private static readonly ImmutableDictionary<LogLevel, string> LOG_LEVEL_NAMES = new Dictionary<LogLevel, string>()
    {
        [LogLevel.None] = "[None] ",
        [LogLevel.Trace] = "[Trace]",
        [LogLevel.Debug] = "[Debug]",
        [LogLevel.Information] = "[Info] ",
        [LogLevel.Warning] = "[Warn] ",
        [LogLevel.Error] = "[Error]",
        [LogLevel.Critical] = "[CRITICAL]",
    }.ToImmutableDictionary();

    protected virtual string LogConverterDefault(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        Exception? exception,
        string formattedStateException)
    {
        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString(DateTimeToStringFormat.HH_MM_SS_FFF));
        sb.Append($" {LOG_LEVEL_NAMES[logLevel]} ({categoryName})");
        if (!string.IsNullOrWhiteSpace(eventId.Name) || eventId.Id > 0)
        {
            sb.Append($" ({eventId.Name}:{eventId.Id})");
        }
        sb.Append($"  ");
        sb.Append(formattedStateException);
        if (exception != null)
        {
            sb.AppendLine();
            sb.AppendLine(exception.ToString());
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public delegate string LogConverterDelegate(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        Exception? exception,
        string formattedStateException);

    private bool logDisabled;

    protected virtual void LogDisable()
    {
        logDisabled = true;
    }

    protected virtual void LogEnable()
    {
        logDisabled = false;
    }

    protected LogConverterDelegate LogConverter { get; set; }
    protected LogLevel LogLevel { get; set; }

    protected readonly ITestOutputHelper output;
    protected readonly ILoggerProvider loggerProvider;
    protected readonly ILogger log;
    public TestBase(ITestOutputHelper testOutputHelper)
    {
        output = testOutputHelper;
        LogConverter = LogConverterDefault;
        LogEventDelegate d = (categoryName, logLevel, eventId, state, exception, formattedStateException) =>
        {
            var msg = LogConverter(categoryName, logLevel, eventId, state, exception, formattedStateException);
            if (!logDisabled)
            {
                output.WriteLine(msg);
            }
        };
        var logEventHandlerWrapperFunc = new LogEventHandlerWrapperFunc(() => d);
        Func<ILogEventHandler> funcLogEventHandler = () => logEventHandlerWrapperFunc;
        loggerProvider = new LoggerProviderFunc(funcLogEventHandler, () => LogLevel);
        log = loggerProvider.CreateLogger(GetType());

        /*
        var type = testOutputHelper.GetType();
        var testField = type.GetField("test", BindingFlags.Instance | BindingFlags.NonPublic);
        var test = (ITest)testField!.GetValue(testOutputHelper);
        var testCase = test!.TestCase;
        var testMethod = testCase.TestMethod;
        var method = testMethod.Method;
        var testClass = testMethod.TestClass;
        var clazz = testClass.Class;
        */
    }


}
