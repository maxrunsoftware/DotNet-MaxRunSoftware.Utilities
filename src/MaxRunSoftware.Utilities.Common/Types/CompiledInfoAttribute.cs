using System.Runtime.CompilerServices;

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Allows maintaining order of properties.
/// https://stackoverflow.com/a/17998371
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class CompiledInfoAttribute(
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string filePath = "",
    [CallerLineNumber] int lineNumber = 0)
    : Attribute
{
    public string? MemberName { get; } = memberName;
    public string? FilePath { get; } = filePath;
    public int? LineNumber { get; } = lineNumber;
}
