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

namespace MaxRunSoftware.Utilities.Common;

public class LogEvent
{
    public string CategoryName { get; }
    public LogLevel LogLevel { get; }
    public EventId EventId { get; }
    public Type StateType { get; }
    public object? StateObject { get; }
    public Exception? Exception { get; }
    public LoggerFormatterDelegate Formatter { get; }
    public IReadOnlyList<Tuple<string?, object?>> State { get; }
    public string Text { get; }

    // ReSharper disable once InconsistentNaming
    private static readonly LogStateParser logStateParser = new();

    public LogEvent(
        string categoryName,
        LogLevel logLevel,
        EventId eventId,
        Type stateType,
        object? stateObject,
        Exception? exception,
        LoggerFormatterDelegate formatter,
        string text
    )
    {
        CategoryName = categoryName;
        LogLevel = logLevel;
        EventId = eventId;
        StateType = stateType;
        StateObject = stateObject;
        Exception = exception;
        Formatter = formatter;
        Text = text;
        State = logStateParser.Parse(stateObject);
    }











}
