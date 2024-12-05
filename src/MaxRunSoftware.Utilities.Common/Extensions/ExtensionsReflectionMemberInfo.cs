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

using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsReflectionMemberInfo
{
    /*
    #region BindingFlags

    public static bool IsIgnoreCase(this BindingFlags flags) => (flags & BindingFlags.IgnoreCase) != 0;
    public static bool IsDeclaredOnly(this BindingFlags flags) => (flags & BindingFlags.DeclaredOnly) != 0;
    public static bool IsInstance(this BindingFlags flags) => (flags & BindingFlags.Instance) != 0;
    public static bool IsStatic(this BindingFlags flags) => (flags & BindingFlags.Static) != 0;
    public static bool IsPublic(this BindingFlags flags) => (flags & BindingFlags.Public) != 0;
    public static bool IsNonPublic(this BindingFlags flags) => (flags & BindingFlags.NonPublic) != 0;
    public static bool IsFlattenHierarch(this BindingFlags flags) => (flags & BindingFlags.FlattenHierarchy) != 0;

    public static bool IsInvokeMethod(this BindingFlags flags) => (flags & BindingFlags.InvokeMethod) != 0;
    public static bool IsCreateInstance(this BindingFlags flags) => (flags & BindingFlags.CreateInstance) != 0;
    public static bool IsGetField(this BindingFlags flags) => (flags & BindingFlags.GetField) != 0;
    public static bool IsSetField(this BindingFlags flags) => (flags & BindingFlags.SetField) != 0;
    public static bool IsGetProperty(this BindingFlags flags) => (flags & BindingFlags.GetProperty) != 0;
    public static bool IsSetProperty(this BindingFlags flags) => (flags & BindingFlags.SetProperty) != 0;

    public static bool IsPutDispProperty(this BindingFlags flags) => (flags & BindingFlags.PutDispProperty) != 0;
    public static bool IsPutRefDispProperty(this BindingFlags flags) => (flags & BindingFlags.PutRefDispProperty) != 0;

    public static bool IsExactBinding(this BindingFlags flags) => (flags & BindingFlags.ExactBinding) != 0;
    public static bool IsSuppressChangeType(this BindingFlags flags) => (flags & BindingFlags.SuppressChangeType) != 0;

    public static bool IsOptionalParamBinding(this BindingFlags flags) => (flags & BindingFlags.OptionalParamBinding) != 0;

    public static bool IsIgnoreReturn(this BindingFlags flags) => flags.HasFlag() (flags & BindingFlags.IgnoreReturn) != 0;
    public static bool IsDoNotWrapExceptions(this BindingFlags flags) => (flags & BindingFlags.DoNotWrapExceptions) != 0;

    #endregion BindingFlags
    */
    
    internal static string GetTypeNamePrefix(MemberInfo info)
    {
        var type = info.ReflectedType;
        if (type == null) type = info.DeclaringType;
        if (type == null) return string.Empty;
        return type.FullNameFormatted() + ".";
    }
    
    public static Type[]? GetGenericArguments_Safe(this MethodBase methodBase)
    {
        try
        {
            return methodBase.GetGenericArguments();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }
    
    public static int? MetadataToken_Safe(this MemberInfo info)
    {
        try
        {
            // https://learn.microsoft.com/en-us/dotnet/api/System.Reflection.MemberInfo.MetadataToken?view=net-8.0
            return info.MetadataToken;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }
    
    public static int? MetadataToken_Safe(this Module info)
    {
        try
        {
            return info.MetadataToken;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }
    
    public static Module? Module_Safe(this MemberInfo info)
    {
        try
        {
            // https://learn.microsoft.com/en-us/dotnet/api/System.Reflection.MemberInfo.Module?view=net-8.0
            return info.Module;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }

    public static bool? HasSameMetadataDefinitionAs_Safe(this MemberInfo x, MemberInfo y)
    {
        try
        {
            // https://github.com/dotnet/runtime/issues/16288
            return x.HasSameMetadataDefinitionAs(y);
        }
        catch (NotImplementedException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (Exception e)
        {
            if (e is NotImplementedException) return null;
            if (e is not InvalidOperationException) return null;
            throw;
        }
    }



    /*
    public static bool EqualsDeep(
        this MemberInfo x, 
        MemberInfo y, 
        bool checkMetadataDefinition = false, 
        bool checkMetadataToken = false, 
        bool checkModule = false,
        bool checkAttributes = false
        )
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return true;

        if (x.Name != y.Name) return false;
        if (x.MemberType != y.MemberType) return false;
        if (x.DeclaringType != y.DeclaringType) return false;
        if (x.IsCollectible != y.IsCollectible) return false;

        if (checkMetadataDefinition)
        {
            var b = x.HasSameMetadataDefinitionAs_Safe(y);
            if (b != null) return b.Value;
        }

        if (checkMetadataToken)
        {
            var xt = x.MetadataToken_Safe();
            var yt = y.MetadataToken_Safe();

            if (xt == null && yt != null) return false;
            if (xt != null && yt == null) return false;
            if (xt != null && yt != null && xt.Value != yt.Value) return false;
        }

        if (checkModule)
        {
            var xm = x.Module_Safe();
            var ym = y.Module_Safe();

            if (xm == null && ym != null) return false;
            if (xm != null && ym == null) return false;
            if (xm != null && ym != null && !xm.Equals(ym)) return false;
        }

        if (checkAttributes)
        {
            var xAttrs = Attribute.GetCustomAttributes(x);
            var yAttrs = Attribute.GetCustomAttributes(y);
            if (xAttrs.Length != yAttrs.Length) return false;

            var xAttrTypeCounts = xAttrs.Select(o => o.GetType()).GroupBy(o => o).ToDictionary(o => o.Key, o => o.Count());
            var yAttrTypeCounts = yAttrs.Select(o => o.GetType()).GroupBy(o => o).ToDictionary(o => o.Key, o => o.Count());
            if (xAttrTypeCounts.Count != yAttrTypeCounts.Count) return false;

            foreach (var (xAttrType, xAttrTypeCount) in xAttrTypeCounts)
            {
                if (!yAttrTypeCounts.TryGetValue(xAttrType, out var yAttrTypeCount)) return false;
                if (xAttrTypeCount != yAttrTypeCount) return false;
            }
        }

        
        
        return true;
    }
    */
}
