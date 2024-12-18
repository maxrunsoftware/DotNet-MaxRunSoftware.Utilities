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

using Xunit.Abstractions;
using Xunit.Sdk;

namespace MaxRunSoftware.Utilities.Testing;

public abstract class TestBaseBase : IDisposable
{
    private class LogCategoryTrimmer(string baseCategory)
    {
        private readonly ImmutableArray<string> baseCategoryPartsReversed = baseCategory.Split('.').Reverse().ToImmutableArray();

        private readonly Dictionary<string, string> categoryCodes = new();

        public string Trim(string category)
        {
            if (category.Length == 0) return category;

            if (categoryCodes.TryGetValue(category, out var categoryClean)) return categoryClean;

            var classNameParts = new Stack<string>(baseCategoryPartsReversed);
            var categoryParts = new Stack<string>(category.Split('.').Reverse());
            while (categoryParts.Count > 1 && classNameParts.Count > 0)
            {
                var classNamePart = classNameParts.Pop();
                if (categoryParts.Peek() != classNamePart) break;
                categoryParts.Pop();
            }

            categoryClean = categoryParts.ToStringDelimited(".");
            categoryCodes[category] = categoryClean;
            return categoryClean;
        }

        public LogCategoryTrimmer(Type baseCategory) : this(baseCategory.FullNameFormatted(false)) { }
    }
    
    private readonly LogCategoryTrimmer logMessageFormatCategoryTrimmer;
    
    protected sealed class LoggerProviderDelegating(LoggerDelegateIsEnabled isEnabled, LoggerDelegateEvent eventHandler) : ILoggerProvider, ILoggerFactory
    {
        private readonly LoggerDelegateIsEnabled isEnabled = isEnabled;
        private readonly LoggerDelegateEvent eventHandler = eventHandler;
        public ILogger CreateLogger(string categoryName) => new LoggerDelegating(categoryName, this);
        
        private sealed class LoggerDelegating(string categoryName, LoggerProviderDelegating provider) : LoggerBase(categoryName)
        {
            public override bool IsEnabled(LogLevel logLevel) => provider.isEnabled(CategoryName, logLevel);
            protected override void Log(LogEvent logEvent) => provider.eventHandler(logEvent);
        }
        
        public void Dispose() { }
        public void AddProvider(ILoggerProvider provider) => throw new NotImplementedException();

    }

    
    // ReSharper disable once IntroduceOptionalParameters.Global
    protected TestBaseBase(ITestOutputHelper? testOutputHelper) : this(testOutputHelper, null) { }
    protected TestBaseBase(ITestOutputHelper? testOutputHelper, IEnumerable<SkippedTest>? skippedTests)
    {
        testOutputHelperWrapper = new(testOutputHelper);
        LogLevel = LogLevel.Information;
        IsColorEnabled = true;
        logMessageFormatCategoryTrimmer = new(GetType());

        LoggerProvider = new LoggerProviderDelegating(IsLogEnabledFor, LogMessage);
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
        private class WrapperField<T>(FieldInfo info)
        {
            // public FieldInfo Info { get; } = info;
            private Func<object?, object?> Getter { get; } = info.CreateFieldGetter();
            public T Get(object instance) => (T)Getter(instance).CheckNotNull();
        }
        private readonly WrapperField<StringBuilder> buffer;
        public StringBuilder Buffer => buffer.Get(TestOutputHelper);

        private readonly WrapperField<IMessageBus> messageBus;
        public IMessageBus MessageBus => messageBus.Get(TestOutputHelper);

        private readonly WrapperField<ITest> test;
        public ITest Test => test.Get(TestOutputHelper);

        private readonly WrapperField<object> lockObject;
        public object LockObject => lockObject.Get(TestOutputHelper);

        private readonly MethodCaller guardInitialized;
        public void GuardInitialized() => guardInitialized.Invoke(TestOutputHelper, []);

        public TestOutputHelper TestOutputHelper { get; }

        public TestOutputHelperWrapper(ITestOutputHelper? helper)
        {
            TestOutputHelper = (TestOutputHelper?)helper ?? throw new ArgumentNullException(nameof(helper));

            buffer = GetField<StringBuilder>(nameof(buffer));
            messageBus = GetField<IMessageBus>(nameof(messageBus));
            test = GetField<ITest>(nameof(test));
            lockObject = GetField<object>(nameof(lockObject));

            guardInitialized = GetMethod(nameof(GuardInitialized));
        }


        private WrapperField<T> GetField<T>(string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = TestOutputHelper.GetType();
            var fieldType = typeof(T);
            var field = t.GetFields(flags).Where(o => o.Name.EqualsOrdinalIgnoreCase(name)).FirstOrDefault();
            if (field == null && fieldType != typeof(object))
                field = t.GetFields(flags).SingleOrDefault(o => o.FieldType.IsAssignableTo<T>());

            if (field != null) return new(field);

            throw new NullReferenceException($"Could not find field {t.Name}.{name} [{fieldType.NameFormatted()}]");
        }

        private MethodCaller GetMethod(string name)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var t = TestOutputHelper.GetType();
            var method = t.GetMethods(flags).SingleOrDefault(o => o.Name == name);

            if (method != null) return method.GetMethodCaller();

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
        logLevel.IsActiveFor(LogLevels.GetValueOrDefault(categoryName, LogLevel));

    //private readonly LogCategoryTrimmer logMessageFormatCategoryTrimmer;

    protected bool IsColorEnabled { get; set; }

    protected LogLevel LogLevel { get; set; }
    protected Dictionary<string, LogLevel> LogLevels { get; } = new(StringComparer.OrdinalIgnoreCase);

    protected virtual string LogMessageFormatColor(string text, LogLevel logLevel)
    {
        var (fg, bg) = Constant.LogLevel_ConsoleColor[logLevel];
        return text.FormatTerminal(fg, bg);
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
        var d = Util.CreateDisposable(() => logDisableCounter.Decrement());
        logDisableCounter.Increment();
        return d;
    }

    private volatile int logEnableForce;
    protected virtual void LogEnableForce() => Interlocked.Increment(ref logEnableForce);

    protected ILoggerFactory LoggerProvider { get; }
    protected readonly ILogger log;

    #endregion Logging

    #region TestDirectory / TestFile

    private TempDirectory? testDirectory;

    protected string TestDirectory
    {
        get
        {
            testDirectory ??= new(log: LoggerProvider.CreateLogger<TempDirectory>());
            return testDirectory.Path;
        }
    }

    private readonly Stack<TempFile> tempFiles = new();
    protected string CreateTestFile(bool createEmptyFile = false)
    {
        var f = new TempFile(path: TestDirectory, createEmptyFile: createEmptyFile, log: LoggerProvider.CreateLogger<TempFile>());
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
