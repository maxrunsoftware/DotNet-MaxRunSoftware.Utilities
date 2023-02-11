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

using System.Drawing;
using MaxRunSoftware.Utilities.Common.JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Xunit.Sdk;

namespace MaxRunSoftware.Utilities.Common.Tests;

[PublicAPI]
public class SkippedTest
{
    public Type Clazz { get; }
    public string? Method { get; }
    public string Message { get; }

    public SkippedTest(Type clazz, string? method, string message)
    {
        Clazz = clazz;
        Method = method;
        Message = message;
    }

    public SkippedTest(Type clazz, string message) : this(clazz, null, message) { }

    public bool IsMatch(Type? testClazz, string? testMethod)
    {
        if (testClazz == null) return false;
        if (Clazz != testClazz) return false;
        if (Method == null) return true;
        if (string.Equals(Method, testMethod, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public void CheckSkip(Type? testClazz, string? testMethod) => Skip.If(IsMatch(testClazz, testMethod), Message);

    public static SkippedTest Create<T>(string methodName, string message) => new(typeof(T), methodName, message);
    public static SkippedTest Create<T>(string message) => new(typeof(T), null, message);
}

[PublicAPI]
public abstract class TestBase : IDisposable
{
    // ReSharper disable once IntroduceOptionalParameters.Global
    protected TestBase(ITestOutputHelper testOutputHelper) : this(testOutputHelper, null) { }
    protected TestBase(ITestOutputHelper testOutputHelper, IEnumerable<SkippedTest>? skippedTests)
    {
        testOutputHelperWrapper = new(testOutputHelper);
        IsColorEnabled = true;
        LogConverter = LogConverterDefault;

        LogEventDelegate d = (categoryName, logLevel, eventId, state, exception, formattedStateException) =>
        {
            var msg = LogConverter(categoryName, logLevel, eventId, state, exception, formattedStateException);
            if (logEnableForce > 0 || logDisableCounter.IsLogEnabled)
            {
                WriteLine(msg);
            }
        };
        var logEventHandlerWrapperFunc = new LogEventHandlerWrapperFunc(() => d);
        Func<ILogEventHandler> funcLogEventHandler = () => logEventHandlerWrapperFunc;
        LoggerProvider = new LoggerProviderFunc(funcLogEventHandler, () => LogLevel);
        log = LoggerProvider.CreateLogger(GetType());

        var tc = TestClassName ?? "UNKNOWN";
        var tcf = TestClassNameFull ?? "UNKNOWN";
        var tm = TestMethodNameWithArguments ?? "UNKNOWN";
        foreach (var skippedTest in skippedTests.OrEmpty())
        {
            if (!skippedTest.IsMatch(TestClass, TestMethodName)) continue;

            WriteLine($"Skipping Test: {tc}.{tm}");
            log.LogWarning("Skipping Test: {Class}.{Method}", tcf, tm);

            skippedTest.CheckSkip(TestClass, TestMethodName);
        }
        WriteLine($"{tcf}.{tm}");
        log.LogInformation("Running Test: {Class}.{Method}", tcf, tm);
    }

    #region TestOutputHelperWrapper

    protected class TestOutputHelperWrapper
    {
        private readonly FieldSlim buffer;
        public StringBuilder Buffer => (StringBuilder)buffer.GetValue(TestOutputHelper).CheckNotNull();

        private readonly FieldSlim messageBus;
        public IMessageBus MessageBus => (IMessageBus)messageBus.GetValue(TestOutputHelper).CheckNotNull();

        private readonly FieldSlim test;
        public ITest Test => (ITest)test.GetValue(TestOutputHelper).CheckNotNull();

        private readonly FieldSlim lockObject;
        public object LockObject => lockObject.GetValue(TestOutputHelper).CheckNotNull();

        private readonly MethodSlim guardInitialized;
        public void GuardInitialized() => guardInitialized.Invoke(TestOutputHelper);

        public TestOutputHelper TestOutputHelper { get; }

        public TestOutputHelperWrapper(ITestOutputHelper helper)
        {
            TestOutputHelper = (TestOutputHelper)helper;

            buffer = GetField<StringBuilder>(nameof(buffer));
            messageBus = GetField<IMessageBus>(nameof(messageBus));
            test = GetField<ITest>(nameof(test));
            lockObject = GetField<object>(nameof(lockObject));

            guardInitialized = GetMethod(nameof(GuardInitialized));

        }

        private FieldSlim GetField<T>(string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = TestOutputHelper.GetType().ToTypeSlim();
            var fieldType = typeof(T);
            var field = t.GetFieldSlim(name, flags);
            if (field == null && fieldType != typeof(object))
                field = t.GetFieldSlims(flags).SingleOrDefault(o => o.Type.Type.IsAssignableTo<T>());

            if (field != null) return field;

            throw new NullReferenceException($"Could not find field {t.Name}.{name} [{fieldType.NameFormatted()}]");
        }

        private MethodSlim GetMethod(string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = TestOutputHelper.GetType().ToTypeSlim();
            var method = t.GetMethodSlims(flags).SingleOrDefault(o => o.Name == name);

            if (method != null) return method;

            throw new NullReferenceException($"Could not find method {t.Name}.{name}");
        }

        public void QueueTestOutput(string output)
        {
            //output = EscapeInvalidHexChars(output);
            lock (LockObject)
            {
                GuardInitialized();
                Buffer.Append(output);
            }
            MessageBus.QueueMessage(new TestOutput(Test, output));
        }




    }

    protected readonly TestOutputHelperWrapper testOutputHelperWrapper;

    #endregion TestOutputHelperWrapper

    #region Logging

    private readonly Dictionary<string, string> logConverterDefaultCategoryCache = new();

    protected bool IsColorEnabled { get; set; }

    private string LogConverterDefaultCategory(string category)
    {
        if (category.Length == 0) return category;

        if (logConverterDefaultCategoryCache.TryGetValue(category, out var categoryClean)) return categoryClean;

            var classNameParts = new Stack<string>(GetType().FullNameFormatted().Split('.').Reverse());
            var categoryParts = new Stack<string>(category.Split('.').Reverse());
            while (categoryParts.Count > 1 && classNameParts.Count > 0)
            {
                var classNamePart = classNameParts.Pop();
                if (categoryParts.Peek() != classNamePart) break;
                categoryParts.Pop();
            }

            categoryClean = categoryParts.ToStringDelimited(".");
            logConverterDefaultCategoryCache[category] = categoryClean;
            return categoryClean;
    }

    // ReSharper disable ConvertClosureToMethodGroup
    protected IDictionary<LogLevel, Func<IConsoleFormat>> LogColors { get; } = new Dictionary<LogLevel, Func<IConsoleFormat>>()
    {
        [LogLevel.None] = () => ConsoleFormat.F(TerminalColor.Magenta),
        [LogLevel.Trace] = () => ConsoleFormat.F(TerminalColor.Blue),
        [LogLevel.Debug] = () => ConsoleFormat.F(TerminalColor.Blue_Bright),
        [LogLevel.Information] = () => ConsoleFormat.D(TerminalSGR.Reset),
        [LogLevel.Warning] = () => ConsoleFormat.F(TerminalColor.Yellow_Bright),
        [LogLevel.Error] = () => ConsoleFormat.F(TerminalColor.Red_Bright),
        [LogLevel.Critical] = () => ConsoleFormat.F(TerminalColor.White_Bright).B(TerminalColor.Red),
    };
    // ReSharper restore ConvertClosureToMethodGroup

    protected virtual string LogConverterDefaultColor(string text, LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace: return TerminalFormat.FormatTerminal4(TerminalFormat.Color_TerminalColor4[Color.])
            case LogLevel.Debug: break;
            case LogLevel.Information: break;
            case LogLevel.Warning: break;
            case LogLevel.Error: break;
            case LogLevel.Critical: break;
            case LogLevel.None: break;
            default: throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    protected virtual string LogConverterDefault(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        Exception? exception,
        string formattedStateException)
    {
        if (IsColorEnabled) ConsoleColorCrayon.Enable();
        else ConsoleColorCrayon.Disable();

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString(DateTimeToStringFormat.HH_MM_SS_FFF));

        var logLevelString = logLevel switch
        {
            LogLevel.None => "[None] ",
            LogLevel.Trace => "[Trace]",
            LogLevel.Debug => "[Debug]",
            LogLevel.Information => "[Info] ",
            LogLevel.Warning => "[Warn] ",
            LogLevel.Error => "[Error]",
            LogLevel.Critical => "[CRITICAL]",
            _ => throw new NotImplementedException($"{logLevel}"),
        };
        sb.Append($" {logLevelString}");

        sb.Append($" {LogConverterDefaultCategory(categoryName)}");

        if (!string.IsNullOrWhiteSpace(eventId.Name) || eventId.Id > 0) sb.Append($" ({eventId.Name}:{eventId.Id})");

        sb.Append($"  {formattedStateException}");

        if (exception != null) sb.AppendLine().AppendLine(exception.ToString()).AppendLine();

        var msg = sb.ToString();
        if (IsColorEnabled)
        {

            msg = LogColors[logLevel]().Text(msg);
        }
        return msg;
    }

    public delegate string LogConverterDelegate(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        object? state,
        Exception? exception,
        string formattedStateException);


    private class LogDisableCounter
    {
        private volatile int counter;
        public void Increment() => Interlocked.Increment(ref counter);
        public void Decrement() => Interlocked.Decrement(ref counter);
        public bool IsLogEnabled => counter < 1;
    }

    private readonly LogDisableCounter logDisableCounter = new();
    private class LogDisableDisposable : IDisposable
    {
        private readonly LogDisableCounter counter;
        public LogDisableDisposable(LogDisableCounter counter)
        {
            this.counter = counter;
            this.counter.Increment();
        }
        public void Dispose() => counter.Decrement();
    }
    protected virtual IDisposable LogDisable() => new LogDisableDisposable(logDisableCounter);

    private volatile int logEnableForce;
    protected virtual void LogEnableForce() => Interlocked.Increment(ref logEnableForce);

    protected LogConverterDelegate LogConverter { get; set; }
    protected LogLevel LogLevel { get; set; }

    protected ILoggerProvider LoggerProvider { get; }
    protected readonly ILogger log;

    #endregion Logging

    #region TestDirectory / TestFile

    private TempDirectory? testDirectory;
    protected string TestDirectory
    {
        get
        {
            testDirectory ??= Util.CreateTempDirectory(LoggerProvider);
            return testDirectory.Path;
        }
    }

    private readonly Stack<TempFile> tempFiles = new();
    protected string CreateTestFile(bool createEmptyFile = false)
    {
        var f = Util.CreateTempFile(TestDirectory, createEmptyFile: createEmptyFile, loggerProvider: LoggerProvider);
        tempFiles.Push(f);
        return f.Path;
    }

    #endregion TestDirectory / TestFile

    #region Test Info

    public ITest Test => testOutputHelperWrapper.Test;

    public ITestCase? TestCase => Test.TestCase;

    public ITestMethod? TestMethod => TestCase?.TestMethod;

    public Type? TestClass => TestMethod?.TestClass?.Class?.ToRuntimeType();

    public string? TestClassName => TestClass?.NameFormatted();

    public string? TestClassNameFull => TestClass?.FullNameFormatted();

    public string? TestMethodName => TestMethod?.Method?.Name;

    public string? TestMethodNameWithArguments
    {
        get
        {
            static string ToStringArg(object? arg) => arg switch
            {
                null => "null",
                string argString => "\"" + argString + "\"",
                _ => arg.ToString() ?? string.Empty,
            };

            var mn = TestMethodName;
            if (mn == null) return null;
            var argsString = TestCase?.TestMethodArguments.OrEmpty().Select(ToStringArg).ToStringDelimited(", ");
            return mn + "(" + argsString + ")";
        }
    }

    #endregion Test Info

    #region Write

    private void WriteInternal(string message) => testOutputHelperWrapper.QueueTestOutput(message);

    public void WriteLine(string message) => WriteInternal(message.CheckNotNull() + Environment.NewLine);

    public void WriteLine(string format, params object[] args) => WriteInternal(string.Format(format.CheckNotNull(), args.CheckNotNull()) + Environment.NewLine);

    public void Write(string message) => WriteInternal(message.CheckNotNull());

    public void Write(string format, params object[] args) => WriteInternal(string.Format(format.CheckNotNull(), args.CheckNotNull()));

    #endregion Write

    #region Dispose

    public virtual void Dispose()
    {
        while (tempFiles.Count > 0)
        {
            var f = tempFiles.Pop();
            f.DisposeSafely(log);
        }

        var d = testDirectory;
        testDirectory = null;
        d?.Dispose();
    }

    #endregion Dispose

}
