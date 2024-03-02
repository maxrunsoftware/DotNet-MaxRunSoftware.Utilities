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

using System.Runtime.CompilerServices;

namespace MaxRunSoftware.Utilities.Common;

public sealed class CallerInfo : ImmutableObjectBase<CallerInfo>
{
    public string? FilePath { get; }
    public int? LineNumber { get; }
    public string? MemberName { get; }
    public string? ArgumentExpression { get; }

    // , [CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null, [CallerMemberName] string? memberName = null)
    // , [CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null, [CallerMemberName] string? memberName = null, [CallerArgumentExpression("condition")] string? callerArgumentExpression = null)
    public CallerInfo(string? callerFilePath, int? callerLineNumber, string? callerMemberName, string? callerArgumentExpression = null)
    {
        // https://blog.jetbrains.com/dotnet/2021/11/04/caller-argument-expressions-in-csharp-10/
        // https://weblogs.asp.net/dixin/csharp-10-new-feature-callerargumentexpression-argument-check-and-more

        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/caller-information
        FilePath = callerFilePath.TrimOrNull();
        LineNumber = callerLineNumber is null or int.MaxValue or int.MinValue ? null : callerLineNumber.Value;
        MemberName = callerMemberName.TrimOrNull();
        ArgumentExpression = callerArgumentExpression.TrimOrNull();
    }

    protected override int GetHashCode_Build() => Util.Hash(FilePath, LineNumber, MemberName, ArgumentExpression);
    protected override string ToString_Build() => $"{nameof(CallerInfo)}({nameof(MemberName)}={MemberName}, {nameof(FilePath)}={FilePath}, {nameof(LineNumber)}={LineNumber}, {nameof(ArgumentExpression)}={ArgumentExpression})";
    protected override bool Equals_Internal(CallerInfo other) =>
        FilePath.IsEqual(other.FilePath) &&
        LineNumber.IsEqual(other.LineNumber) &&
        MemberName.IsEqual(other.MemberName) &&
        ArgumentExpression.IsEqual(other.ArgumentExpression);

    protected override int CompareTo_Internal(CallerInfo other)
    {
        int c;
        if (0 != (c = FilePath.Compare(other.FilePath))) return c;
        if (0 != (c = LineNumber.Compare(other.LineNumber))) return c;
        if (0 != (c = MemberName.Compare(other.MemberName))) return c;
        if (0 != (c = ArgumentExpression.Compare(other.ArgumentExpression))) return c;
        return 0;
    }

    public static CallerInfo Get([CallerFilePath] string? filePath = null, [CallerLineNumber] int? lineNumber = null, [CallerMemberName] string? memberName = null) =>
        new(filePath, lineNumber, memberName);

    public static string? GetName([CallerMemberName] string? memberName = null) => memberName;
}
