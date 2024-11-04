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

public static class AccessibilityLevelExtensions
{
    private static volatile bool throwExceptionOnUnknown;

    public static bool ThrowExceptionOnUnknown
    {
        get => throwExceptionOnUnknown;
        set => throwExceptionOnUnknown = value;
    }

    private static AccessibilityLevel GetAccessibilityLevel_Internal(
        MemberInfo info,
        bool isPublic,
        bool isFamily,
        bool isAssembly,
        bool isFamORAssem,
        bool isPrivate,
        bool isFamANDAssem
    )
    {
        if (isPublic) return AccessibilityLevel.Public;
        if (isFamANDAssem) return AccessibilityLevel.PrivateProtected;
        if (isFamORAssem) return AccessibilityLevel.ProtectedInternal;
        if (isPrivate) return AccessibilityLevel.Private;
        if (isFamily) return AccessibilityLevel.Protected;
        if (isAssembly) return AccessibilityLevel.Internal;

        if (ThrowExceptionOnUnknown)
            throw new NotImplementedException(string.Format(
                "Could not determine {0} of [{1}] '{2}' -> {3}",
                nameof(AccessibilityLevel),
                info.GetType().FullNameFormatted(),
                info.Name,
                info
            ));

        return AccessibilityLevel.Unknown;

    }
    
    public static AccessibilityLevel GetAccessibilityLevel(this Type info) =>
        info.IsNested 
            ? GetAccessibilityLevel_Internal(info, info.IsPublic, info.IsNestedFamily, info.IsNestedAssembly, info.IsNestedFamORAssem, info.IsNestedPrivate, info.IsNestedFamANDAssem) 
            : GetAccessibilityLevel_Internal(info, info.IsPublic, false, info.IsNotPublic, false, false, false);

    public static AccessibilityLevel GetAccessibilityLevel(this MethodBase info) => 
        GetAccessibilityLevel_Internal(info, info.IsPublic, info.IsFamily, info.IsAssembly, info.IsFamilyOrAssembly, info.IsPrivate, info.IsFamilyAndAssembly);

    public static AccessibilityLevel GetAccessibilityLevel(this FieldInfo info) => 
        GetAccessibilityLevel_Internal(info, info.IsPublic, info.IsFamily, info.IsAssembly, info.IsFamilyOrAssembly, info.IsPrivate, info.IsFamilyAndAssembly);
    
    public static (AccessibilityLevel? Getter, AccessibilityLevel? Setter) GetAccessibilityLevel(this PropertyInfo info)
    {
        var getterMethod = info.CanRead ? info.GetGetMethod(true) : null;
        AccessibilityLevel? getter = getterMethod == null ? null : GetAccessibilityLevel(getterMethod);
        
        var setterMethod = info.CanWrite ? info.GetGetMethod(true) : null;
        AccessibilityLevel? setter = setterMethod == null ? null : GetAccessibilityLevel(setterMethod);

        return (getter, setter);
    }
    
    public static (AccessibilityLevel? Add, AccessibilityLevel? Remove, AccessibilityLevel? Raise) GetAccessibilityLevel(this EventInfo info)
    {
        var addMethod = info.AddMethod;
        AccessibilityLevel? add = addMethod == null ? null : GetAccessibilityLevel(addMethod);
        
        var removeMethod = info.RemoveMethod;
        AccessibilityLevel? remove = removeMethod == null ? null : GetAccessibilityLevel(removeMethod);

        var raiseMethod = info.RaiseMethod;
        AccessibilityLevel? raise = raiseMethod == null ? null : GetAccessibilityLevel(raiseMethod);
        
        return (add, remove, raise);
    }
    
}
