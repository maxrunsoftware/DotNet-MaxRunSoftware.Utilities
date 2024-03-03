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

using Xunit.Sdk;

namespace MaxRunSoftware.Utilities.Common.Tests;

#nullable enable

[PublicAPI]
public abstract class TestBase : IDisposable
{
    // ReSharper disable once IntroduceOptionalParameters.Global
    protected TestBase(ITestOutputHelper? testOutputHelper) : this(testOutputHelper, null) { }
    protected TestBase(ITestOutputHelper? testOutputHelper, IEnumerable<SkippedTest>? skippedTests)
    {
        testOutputHelperWrapper = new(testOutputHelper);
        LogLevel = LogLevel.Information;
        IsColorEnabled = true;
        logMessageFormatCategoryTrimmer = new(GetType());

        LoggerProvider = Common.LoggerProvider.Create(IsLogEnabledFor, LogMessage);
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

        public TestOutputHelperWrapper(ITestOutputHelper? helper)
        {
            if (helper == null) throw new ArgumentNullException(nameof(helper));
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

        private void WriteInternal(string message) => QueueTestOutput(message);

        public void WriteLineEscaped(string message) => TestOutputHelper.WriteLine(message);

        public void WriteLineEscaped(string format, params object[] args) => TestOutputHelper.WriteLine(format, args);

        public void WriteLine(string message) => WriteInternal(message.CheckNotNull() + Environment.NewLine);

        public void WriteLine(string format, params object[] args) => WriteInternal(string.Format(format.CheckNotNull(), args.CheckNotNull()) + Environment.NewLine);

        public void Write(string message) => WriteInternal(message.CheckNotNull());

        public void Write(string format, params object[] args) => WriteInternal(string.Format(format.CheckNotNull(), args.CheckNotNull()));
    }

    protected readonly TestOutputHelperWrapper testOutputHelperWrapper;

    #endregion TestOutputHelperWrapper

    #region Logging

    protected virtual bool IsLogEnabledFor(string categoryName, LogLevel logLevel) =>
        logLevel.IsActiveFor(LogLevels.TryGetValue(categoryName, out var logLevelCustom) ? logLevelCustom : LogLevel);

    private readonly LogCategoryTrimmer logMessageFormatCategoryTrimmer;

    protected bool IsColorEnabled { get; set; }

    protected LogLevel LogLevel { get; set; }
    protected Dictionary<string, LogLevel> LogLevels { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected virtual string LogMessageFormatColor(string text, LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace: return text.FormatTerminal(TerminalColors.Blue, null);
            case LogLevel.Debug: return text.FormatTerminal(TerminalColors.BrightBlue, null);
            case LogLevel.Information: return text; //.FormatTerminal4(TerminalColor.BrightWhite, null);
            case LogLevel.Warning: return text.FormatTerminal(TerminalColors.BrightYellow, null);
            case LogLevel.Error: return text.FormatTerminal(TerminalColors.BrightRed, null);
            case LogLevel.Critical: return text.FormatTerminal(null, TerminalColors.Red); //.FormatTerminal4(white, TerminalColor.BrightRed);
            case LogLevel.None: return text.FormatTerminal(TerminalColors.Magenta, null);
            default: throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    protected virtual void LogMessage(LogEvent logEvent)
    {
        if (logEnableForce > 0 || logDisableCounter.IsLogEnabled)
        {
            testOutputHelperWrapper.WriteLine(LogMessageFormat(logEvent));
            //testOutputHelperWrapper.WriteLineEscaped(LogMessageFormat(logEvent));
        }
    }


    protected virtual string LogMessageFormat(LogEvent logEvent)
    {
        var isColorEnabled = IsColorEnabled;
        //if (isColorEnabled) TerminalColor.EnableOnWindows();

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString(DateTimeToStringFormat.HH_MM_SS_FFF));

        var logLevelString = logEvent.LogLevel switch
        {
            LogLevel.None => "[None] ",
            LogLevel.Trace => "[Trace]",
            LogLevel.Debug => "[Debug]",
            LogLevel.Information => "[Info] ",
            LogLevel.Warning => "[Warn] ",
            LogLevel.Error => "[Error]",
            LogLevel.Critical => "[CRITICAL]",
            _ => throw new NotImplementedException($"{logEvent.LogLevel}"),
        };
        sb.Append($" {logLevelString}");

        sb.Append($" {logMessageFormatCategoryTrimmer.Trim(logEvent.CategoryName)}");

        if (!string.IsNullOrWhiteSpace(logEvent.EventId.Name) || logEvent.EventId.Id > 0) sb.Append($" ({logEvent.EventId.Name}:{logEvent.EventId.Id})");

        sb.Append($"  {logEvent.Text}");

        if (logEvent.Exception != null) sb.AppendLine().AppendLine(logEvent.Exception.ToString()).AppendLine();

        var msg = sb.ToString();
        if (isColorEnabled)
        {
            msg = LogMessageFormatColor(msg, logEvent.LogLevel);
        }

        return msg;
    }

    private class LogDisableCounter
    {
        private volatile int counter;
        public void Increment() => Interlocked.Increment(ref counter);
        public void Decrement() => Interlocked.Decrement(ref counter);
        public bool IsLogEnabled => counter < 1;
    }

    private readonly LogDisableCounter logDisableCounter = new();

    protected virtual IDisposable LogDisable()
    {
        var d = DisposableFunc.Create(() => logDisableCounter.Decrement());
        logDisableCounter.Increment();
        return d;
    }

    private volatile int logEnableForce;
    protected virtual void LogEnableForce() => Interlocked.Increment(ref logEnableForce);

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
        var f = Util.CreateTempFile(TestDirectory, createEmptyFile, LoggerProvider);
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

    public void WriteLine(string message) => testOutputHelperWrapper.WriteLineEscaped(message.CheckNotNull());

    public void WriteLine(string format, params object[] args) => testOutputHelperWrapper.WriteLineEscaped(string.Format(format.CheckNotNull(), args.CheckNotNull()));

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
