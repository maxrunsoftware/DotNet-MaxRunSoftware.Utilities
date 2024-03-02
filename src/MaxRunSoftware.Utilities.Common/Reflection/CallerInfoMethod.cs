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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MaxRunSoftware.Utilities.Common;

public class CallerInfoMethod
{
    public CallerInfoMethod(
        IReadOnlyList<object?> args,
        string? callerMemberName,
        IReadOnlyList<string?> callerArgNames,
        string? callerFilePath,
        int? callerLineNumber
    )
    {
        Debug.Assert(args.Count == callerArgNames.Count);
        Args = args.Select((t, i) => (callerArgNames[i], t)).ToImmutableArray();
        MemberName = callerMemberName;
        FilePath = callerFilePath;
        LineNumber = callerLineNumber;
    }

    private CallerInfoMethod(CallerInfoMethod info, int offsetLineNumber)
    {
        Args = info.Args;
        MemberName = info.MemberName;
        FilePath = info.FilePath;
        LineNumber = info.LineNumber + offsetLineNumber;
    }

    public ImmutableArray<(string? Name, object? Value)> Args { get; }
    public string? MemberName { get; }
    public string? FilePath { get; }
    public int? LineNumber { get; }

    public CallerInfoMethod OffsetLineNumber(int offset) => new(this, offset);

    // @formatter:off
    // ReSharper disable once ConvertToStaticClass
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class ArgumentSeparator { private ArgumentSeparator() { } }
    // @formatter:on

    // ReSharper disable once UnusedMember.Local
    private static string[] CodeBuild(int numMethods)
    {
        const string code = @"
                public CallerInfoMethod(
                    object? arg_,
                    ArgumentSeparator? separator = null,
                    [CallerMemberName] string? callerMemberName = null,
                    [CallerArgumentExpression(""arg_"")] string? callerArgName_ = null,
                    [CallerFilePath] string? callerFilePath = null,
                    [CallerLineNumber] int? callerLineNumber = null
                ) : this(
                    new object?[] {
                        arg_,
                    },
                    callerMemberName,
                    new string?[] {
                        callerArgName_,
                    },
                    callerFilePath,
                    callerLineNumber
                ) {}
            ";

        static string[] ParseLine(string codePart, int argCount) =>
            Enumerable.Range(1, codePart.Contains('_') ? argCount : 1)
                .Select(i => codePart.Replace("_", i.ToString()))
                .ToArray();

        string CreateLine(int numArguments) =>
            string.Join(" ",
                code.Split(new[] { "\r\n", "\n", "\r", Environment.NewLine }, StringSplitOptions.None)
                    .Select(o => o.Trim())
                    .Where(o => o.Length > 0)
                    .SelectMany(o => ParseLine(o, numArguments))
                    .ToArray()
            );

        var linesCode = Enumerable.Range(0, numMethods + 1)
            .Select(CreateLine)
            .ToArray();

        var stringsPrefixSuffix = new[]
        {
            new[] { "#region Constructors", "#endregion Constructors" },
            new[] { "// @formatter:off", "// @formatter:on" },
            new[] { "// ReSharper disable UnusedParameter.Local", "// ReSharper restore UnusedParameter.Local" },
            new[] { "// ReSharper disable RedundantExplicitArrayCreation", "// ReSharper restore RedundantExplicitArrayCreation" },
        };
        return stringsPrefixSuffix.Select(o => o[0])
            .Concat(linesCode)
            .Concat(stringsPrefixSuffix.Reverse().Select(o => o[1]))
            .ToArray();
    }


    #region Constructors

    // @formatter:off
    // ReSharper disable UnusedParameter.Local
    // ReSharper disable RedundantExplicitArrayCreation
    public CallerInfoMethod( ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { }, callerMemberName, new string?[] { }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1 }, callerMemberName, new string?[] { callerArgName1 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2 }, callerMemberName, new string?[] { callerArgName1, callerArgName2 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, object? arg12, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerArgumentExpression("arg12")] string? callerArgName12 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11, callerArgName12 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, object? arg12, object? arg13, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerArgumentExpression("arg12")] string? callerArgName12 = null, [CallerArgumentExpression("arg13")] string? callerArgName13 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11, callerArgName12, callerArgName13 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, object? arg12, object? arg13, object? arg14, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerArgumentExpression("arg12")] string? callerArgName12 = null, [CallerArgumentExpression("arg13")] string? callerArgName13 = null, [CallerArgumentExpression("arg14")] string? callerArgName14 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11, callerArgName12, callerArgName13, callerArgName14 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, object? arg12, object? arg13, object? arg14, object? arg15, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerArgumentExpression("arg12")] string? callerArgName12 = null, [CallerArgumentExpression("arg13")] string? callerArgName13 = null, [CallerArgumentExpression("arg14")] string? callerArgName14 = null, [CallerArgumentExpression("arg15")] string? callerArgName15 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11, callerArgName12, callerArgName13, callerArgName14, callerArgName15 }, callerFilePath, callerLineNumber ) {}
    public CallerInfoMethod( object? arg1, object? arg2, object? arg3, object? arg4, object? arg5, object? arg6, object? arg7, object? arg8, object? arg9, object? arg10, object? arg11, object? arg12, object? arg13, object? arg14, object? arg15, object? arg16, ArgumentSeparator? separator = null, [CallerMemberName] string? callerMemberName = null, [CallerArgumentExpression("arg1")] string? callerArgName1 = null, [CallerArgumentExpression("arg2")] string? callerArgName2 = null, [CallerArgumentExpression("arg3")] string? callerArgName3 = null, [CallerArgumentExpression("arg4")] string? callerArgName4 = null, [CallerArgumentExpression("arg5")] string? callerArgName5 = null, [CallerArgumentExpression("arg6")] string? callerArgName6 = null, [CallerArgumentExpression("arg7")] string? callerArgName7 = null, [CallerArgumentExpression("arg8")] string? callerArgName8 = null, [CallerArgumentExpression("arg9")] string? callerArgName9 = null, [CallerArgumentExpression("arg10")] string? callerArgName10 = null, [CallerArgumentExpression("arg11")] string? callerArgName11 = null, [CallerArgumentExpression("arg12")] string? callerArgName12 = null, [CallerArgumentExpression("arg13")] string? callerArgName13 = null, [CallerArgumentExpression("arg14")] string? callerArgName14 = null, [CallerArgumentExpression("arg15")] string? callerArgName15 = null, [CallerArgumentExpression("arg16")] string? callerArgName16 = null, [CallerFilePath] string? callerFilePath = null, [CallerLineNumber] int? callerLineNumber = null ) : this( new object?[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16 }, callerMemberName, new string?[] { callerArgName1, callerArgName2, callerArgName3, callerArgName4, callerArgName5, callerArgName6, callerArgName7, callerArgName8, callerArgName9, callerArgName10, callerArgName11, callerArgName12, callerArgName13, callerArgName14, callerArgName15, callerArgName16 }, callerFilePath, callerLineNumber ) {}
    // ReSharper restore RedundantExplicitArrayCreation
    // ReSharper restore UnusedParameter.Local
    // @formatter:on

    #endregion Constructors
}
