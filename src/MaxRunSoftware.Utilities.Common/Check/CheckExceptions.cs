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

public class CheckNotNullException : ArgumentNullException
{
    public CallerInfo CallerInfo { get; }
    public CheckType Type { get; }
    public Type? Parent { get; }

    private CheckNotNullException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent) : base(name, message)
    {
        Type = type;
        CallerInfo = callerInfo;
        Parent = parent;
    }

    internal static CheckNotNullException Create(CheckType? type, string? name, Type? parent, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            "cannot be null",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent);
    }
}

public abstract class CheckOutOfRangeException : ArgumentOutOfRangeException
{
    public CallerInfo CallerInfo { get; }
    public CheckType Type { get; }
    public Type? Parent { get; }
    public object Value { get; }

    private protected CheckOutOfRangeException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, object value) : base(name, 0, message)
    {
        Type = type;
        CallerInfo = callerInfo;
        Parent = parent;
        Value = value;
    }
}

public class CheckNotEmptyException : CheckOutOfRangeException
{
    private CheckNotEmptyException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent) : base(type, callerInfo, name, message, parent, 0) { }

    internal static CheckNotEmptyException Create(CheckType? type, string? name, Type? parent, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            "cannot be empty",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent);
    }
}

public class CheckMinException : CheckOutOfRangeException
{
    public object Min { get; }

    private CheckMinException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, object value, object min) : base(type, callerInfo, name, message, parent, value) => Min = min;

    internal static CheckMinException Create(CheckType? type, string? name, Type? parent, object value, object min, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"must be greater than or equal to {min} but was {value}",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent, value, min);
    }
}

public class CheckMaxException : CheckOutOfRangeException
{
    public object Max { get; }

    private CheckMaxException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, object value, object max) : base(type, callerInfo, name, message, parent, value) => Max = max;

    internal static CheckMaxException Create(CheckType? type, string? name, Type? parent, object value, object max, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"must be less than or equal to {max} but was {value}",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent, value, max);
    }
}

public class CheckFileExistsException : FileNotFoundException
{
    public string Name { get; }
    public CallerInfo CallerInfo { get; }
    public CheckType Type { get; }
    public Type? Parent { get; }
    public string Path { get; }

    private CheckFileExistsException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, string path) : base(message, path)
    {
        Name = name;
        Type = type;
        CallerInfo = callerInfo;
        Parent = parent;
        Path = path;
    }

    internal static CheckFileExistsException Create(CheckType? type, string? name, Type? parent, string path, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"file not found {path}",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent, path);
    }
}

public class CheckDirectoryExistsException : DirectoryNotFoundException
{
    public string Name { get; }
    public CallerInfo CallerInfo { get; }
    public CheckType Type { get; }
    public Type? Parent { get; }
    public string Path { get; }

    private CheckDirectoryExistsException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, string path) : base(message)
    {
        Name = name;
        Type = type;
        CallerInfo = callerInfo;
        Parent = parent;
        Path = path;
    }

    internal static CheckDirectoryExistsException Create(CheckType? type, string? name, Type? parent, string path, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"directory not found {path}",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent, path);
    }
}

public abstract class CheckTypeException : ArgumentException
{
    public CallerInfo CallerInfo { get; }
    public CheckType Type { get; }
    public Type? Parent { get; }
    public Type Value { get; }

    private protected CheckTypeException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, Type value) : base(message, name)
    {
        Type = type;
        CallerInfo = callerInfo;
        Parent = parent;
        Value = value;
    }
}

public class CheckIsEnumException : CheckTypeException
{
    private protected CheckIsEnumException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, Type value) : base(type, callerInfo, name, message, parent, value) { }

    internal static CheckTypeException Create(CheckType? type, string? name, Type? parent, Type value, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"{value.NameFormatted()} is not an enum",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new CheckIsEnumException(d.Type, d.CallerInfo, d.Name, d.Message, parent, value);
    }
}

public class CheckIsAssignableToException : CheckTypeException
{
    public Type TargetType { get; }
    private protected CheckIsAssignableToException(CheckType type, CallerInfo callerInfo, string name, string message, Type? parent, Type value, Type targetType) : base(type, callerInfo, name, message, parent, value) => TargetType = targetType;

    internal static CheckIsAssignableToException Create(CheckType? type, string? name, Type? parent, Type value, Type targetType, string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression)
    {
        var d = CheckUtil.CreateExceptionDetail(
            type, name, parent,
            $"{value.NameFormatted()} is not assignable to {targetType.NameFormatted()}",
            callerFilePath, callerLineNumber, callerMemberName, callerArgumentExpression);

        return new(d.Type, d.CallerInfo, d.Name, d.Message, parent, value, targetType);
    }
}
