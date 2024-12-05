namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Access Modifiers
/// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/accessibility-levels
/// https://learn.microsoft.com/en-us/dotnet/api/system.reflection.typeattributes?view=net-8.0
/// https://learn.microsoft.com/en-us/dotnet/api/system.reflection.fieldinfo.isfamily?view=net-8.0
/// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/private-protected
/// </summary>
public enum AccessibilityLevel
{
    /// <summary>
    /// Could not determine access type.
    /// </summary>
    Unknown,	
    
    /// <summary>
    /// Public<br/>Access is not restricted.
    /// </summary>
    Public,	
    
    /// <summary>
    /// Family<br/>Access is limited to the containing class or types derived from the containing class.
    /// </summary>
    Protected,	

    /// <summary>
    /// Assembly<br/>Access is limited to the current assembly.
    /// </summary>
    Internal,

    /// <summary>
    /// FamORAssem<br/>Access is limited to the current assembly or types derived from the containing class.
    /// </summary>
    ProtectedInternal,	

    /// <summary>
    /// Private<br/>Access is limited to the containing type.
    /// </summary>
    Private,	

    /// <summary>
    /// FamANDAssem<br/>Access is limited to the containing class or types derived from the containing class within the current assembly.
    /// </summary>
    PrivateProtected,	
}
