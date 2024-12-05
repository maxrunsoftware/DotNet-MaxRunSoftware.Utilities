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

namespace MaxRunSoftware.Utilities.Common;

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
