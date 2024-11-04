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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsReflection
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
    


    
    

    #region IComparable

    public static bool IsComparable(this Type type)
    {
        if (type.IsAssignableTo(typeof(IComparable))) return true;

        foreach (var iface in type.GetInterfaces())
        {
            if (iface.GetGenericArguments().Length != 1) continue;
            var ifaceUntyped = iface.GetGenericTypeDefinition();
            if (ifaceUntyped == typeof(IComparable<>)) return true;
        }

        return false;
    }

    public static MethodBase? GetCompareToMethod(this Type type, Type targetType)
    {
        var comparableInterfaces = new HashSet<Type>();
        foreach (var iface in type.GetInterfaces())
        {
            if (iface == typeof(IComparable))
            {
                comparableInterfaces.Add(iface);
                continue;
            }

            if (iface.GetGenericArguments().Length != 1) continue;

            var ifaceUntyped = iface.GetGenericTypeDefinition();
            if (ifaceUntyped == typeof(IComparable<>)) comparableInterfaces.Add(iface);
        }

        var compareToMethods = new HashSet<MethodBase>();
        foreach (var comparableInterface in comparableInterfaces)
        {
            var map = type.GetInterfaceMap(comparableInterface);
            foreach (var targetMethod in map.TargetMethods) compareToMethods.Add(targetMethod);
        }

        if (compareToMethods.Count == 0) return null;

        return Type.DefaultBinder.SelectMethod(BindingFlags.Default, compareToMethods.ToArray(), new[] { targetType }, null);
    }

    #endregion IComparable

    #region EmbeddedResource

    public static string[] GetEmbeddedResourceNames(this Assembly assembly) => assembly.GetManifestResourceNames();

    public static byte[] GetEmbeddedResource(this Assembly assembly, string filename)
    {
        var resourceNames = assembly.GetEmbeddedResourceNames();
        var resourceName = resourceNames.Single(o => o.EndsWith(filename, StringComparison.OrdinalIgnoreCase));
        if (resourceName == null) throw new($"Could not find resource file: {filename}");
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new($"Could not open stream for resource: {resourceName}  {filename}");
        return stream.ReadAll();
    }

    public static string GetEmbeddedResource(this Assembly assembly, string filename, Encoding encoding) =>
        encoding.GetString(assembly.GetEmbeddedResource(filename));

    #endregion EmbeddedResource
    
    #region Attribute
    
    public static IEnumerable<(Type, Attribute)> GetTypesWithAttribute(this Assembly assembly, Type attributeType, bool inherited = true, bool exactType = false)
    {
        attributeType.CheckIsAssignableTo(typeof(Attribute));
        foreach (var type in assembly.GetTypes())
        {
            var attrs = GetAttributes(type, attributeType, inherited: inherited, exactType: exactType);
            foreach (var attr in attrs)
            {
                yield return (type, attr);
            }
            
        }
    }
    
    
    public static Attribute[] GetAttributes(this ICustomAttributeProvider info, Type attributeType, bool inherited = true, bool exactType = false)
    {
        attributeType.CheckIsAssignableTo(typeof(Attribute));
        
        var attrs = info.GetCustomAttributes(inherited);
        if (attrs.Length == 0) return [];
        
        var list = new List<Attribute>(attrs.Length);
        foreach (var o in attrs)
        {
            if (o is not Attribute attr) continue;
            if (exactType)
            {
                if (attributeType == attr.GetType()) list.Add(attr);
            }
            else
            {
                if (attr.GetType().IsAssignableTo(attributeType)) list.Add(attr);
            }
        }
        
        if (list.IsEmpty()) return [];
        return list.ToArray();
        
    }
    
    public static Attribute? GetAttribute(this ICustomAttributeProvider info, Type attributeType, bool inherited = true, bool? exactType = null)
    {
        attributeType.CheckIsAssignableTo(typeof(Attribute));
        
        if (exactType == null)
        {
            return GetAttribute(info, attributeType, inherited: inherited, exactType: true) 
                   ?? GetAttribute(info, attributeType, inherited: inherited, exactType: false);
        }
        
        return GetAttributes(info, attributeType, inherited: inherited, exactType: exactType.Value).FirstOrDefault();
    }
    
    public static IEnumerable<(Type, TAttribute)> GetTypesWithAttribute<TAttribute>(this Assembly assembly, bool inherited = true, bool exactType = false) where TAttribute : Attribute =>
        GetTypesWithAttribute(assembly, typeof(TAttribute), inherited: inherited, exactType: exactType).Select(o => (o.Item1, (TAttribute)o.Item2));

    public static TAttribute[] GetAttributes<TAttribute>(this ICustomAttributeProvider info, bool inherited = true, bool exactType = false) where TAttribute : Attribute => 
        GetAttributes(info, typeof(TAttribute), inherited: inherited, exactType: exactType)
            .Select(o => (TAttribute)o)
            .ToArray();
    
    public static TAttribute? GetAttribute<TAttribute>(this ICustomAttributeProvider info, bool inherited = true, bool? exactType = null) where TAttribute : Attribute =>
        (TAttribute?)GetAttribute(info, typeof(TAttribute), inherited: inherited, exactType: exactType);
    
    #endregion Attribute

    
}
