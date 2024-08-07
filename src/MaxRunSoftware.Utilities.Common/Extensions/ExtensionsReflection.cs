﻿// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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
    
    public static string? GetFileVersion(this Assembly assembly) => FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

    public static string? GetVersion(this Assembly assembly) => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;

    private static readonly string IsAnonymous_MagicTag;
    private static readonly string IsInner_MagicTag;
    public static bool IsAnonymous(this MethodInfo info) => info.Name.Contains(IsAnonymous_MagicTag);
    public static bool IsInner(this MethodInfo info) => info.Name.Contains(IsInner_MagicTag);

    private static string GetNameMagicTag(this MethodInfo info)
    {
        // https://stackoverflow.com/a/56496797
        var match = new Regex(">([a-zA-Z]+)__").Match(info.Name);
        if (match.Success && match.Value is { } val && !match.NextMatch().Success) return val;
        throw new ArgumentException($"Cant find magic tag of {info}");
    }

    static ExtensionsReflection()
    {
        // ReSharper disable EmptyStatement
        // ReSharper disable MoveLocalFunctionAfterJumpStatement

        // https://stackoverflow.com/a/56496797
        void Inner() { }
        ;
        
        var inner = Inner;
        IsInner_MagicTag = GetNameMagicTag(inner.Method);

        var anonymous = () => { };
        IsAnonymous_MagicTag = GetNameMagicTag(anonymous.Method);
        
        // ReSharper restore MoveLocalFunctionAfterJumpStatement
        // ReSharper restore EmptyStatement
    }


    public static bool IsSystemAssembly(this Assembly assembly)
    {
        assembly.CheckNotNull(nameof(assembly));

        var namePrefixes = new[]
        {
            "CommonLanguageRuntimeLibrary",
            "System.",
            "System,",
            "mscorlib,",
            "netstandard,",
            "Microsoft.CSharp",
            "Microsoft.VisualStudio",
        };

        foreach (var name in AssemblyNames(assembly))
        {
            var s = name?.ToString().TrimOrNull();
            if (s != null && s.StartsWithAny(StringComparison.OrdinalIgnoreCase, namePrefixes)) return true;
        }

        return false;

        static IEnumerable<object?> AssemblyNames(Assembly a)
        {
            foreach (var module in a.Modules) yield return module.ScopeName;
            yield return a.FullName;
            yield return a;
            var an = a.GetName();
            yield return an;
            yield return an.FullName;
            yield return an.Name;
        }
    }

    #region PropertyInfo

    public static bool IsNullable(this PropertyInfo info)
    {
        var type = info.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null) return true;

        if (type.IsValueType) return false;

        // https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-7/#libraries-reflection-apis-for-nullability-information
        // https://stackoverflow.com/a/68757807


        var ctx = new NullabilityInfoContext();
        var nullabilityInfo = ctx.Create(info);
        if (nullabilityInfo.WriteState == NullabilityState.Nullable) return true;
        if (nullabilityInfo.WriteState == NullabilityState.NotNull) return false;
        if (nullabilityInfo.ReadState == NullabilityState.Nullable) return true;
        if (nullabilityInfo.ReadState == NullabilityState.NotNull) return false;
        return true;
    }

    public static bool IsNullableGenericParameter(this PropertyInfo info, int genericParameterIndex = 0)
    {
        // https://stackoverflow.com/a/71549957

        var ctx = new NullabilityInfoContext();
        var nullabilityInfo = ctx.Create(info);
        var gps = nullabilityInfo.GenericTypeArguments;
        if (genericParameterIndex >= gps.Length) throw new ArgumentOutOfRangeException(nameof(genericParameterIndex), $"Could not retrieve generic parameter index {genericParameterIndex} because there are {gps.Length} generic parameters");
        var myModelNullabilityInfo = gps[genericParameterIndex];

        if (myModelNullabilityInfo.WriteState == NullabilityState.Nullable) return true;
        if (myModelNullabilityInfo.WriteState == NullabilityState.NotNull) return false;
        if (myModelNullabilityInfo.ReadState == NullabilityState.Nullable) return true;
        if (myModelNullabilityInfo.ReadState == NullabilityState.NotNull) return false;

        return true;
    }


    public static bool IsGettable(this PropertyInfo info) => info.CanRead && info.GetMethod != null;

    public static bool IsSettable(this PropertyInfo info) => info.CanWrite && info.SetMethod != null;

    /// <summary>
    /// Determines if this property is marked as init-only.
    /// </summary>
    /// <param name="info">The property.</param>
    /// <returns>True if the property is init-only, false otherwise.</returns>
    public static bool IsInitOnly(this PropertyInfo info)
    {
        // https://alistairevans.co.uk/2020/11/01/detecting-init-only-properties-with-reflection-in-c-9/
        if (!info.CanWrite) return false;

        var setMethod = info.SetMethod;
        if (setMethod == null) return false;

        // Get the modifiers applied to the return parameter.
        var setMethodReturnParameterModifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();

        // Init-only properties are marked with the IsExternalInit type.
        return setMethodReturnParameterModifiers.Contains(typeof(IsExternalInit));
    }


    public static Func<object?, object?> CreatePropertyGetter(this PropertyInfo info)
    {
        var exceptionMsg = $"Property {GetTypeNamePrefix(info)}{info.Name} is not gettable";
        if (!info.CanRead) throw new ArgumentException(exceptionMsg, nameof(info));

        // https://stackoverflow.com/questions/16436323/reading-properties-of-an-object-with-expression-trees
        var mi = info.GetMethod;
        if (mi == null) throw new ArgumentException(exceptionMsg, nameof(info));

        //IsStatic = mi.IsStatic;
        var instance = Expression.Parameter(typeof(object), "instance");
        var callExpr = mi.IsStatic // Is this a static property
            ? Expression.Call(null, mi)
            : Expression.Call(Expression.Convert(instance, info.DeclaringType ?? throw new NullReferenceException()), mi);

        var unaryExpression = Expression.TypeAs(callExpr, typeof(object));
        var action = Expression.Lambda<Func<object?, object?>>(unaryExpression, instance).Compile();
        return action;
    }

    public static Action<object?, object?> CreatePropertySetter(this PropertyInfo info)
    {
        var exceptionMsg = $"Property {GetTypeNamePrefix(info)}{info.Name} is not settable";
        if (!info.CanWrite) throw new ArgumentException(exceptionMsg, nameof(info));

        // https://stackoverflow.com/questions/16436323/reading-properties-of-an-object-with-expression-trees
        var methodInfo = info.SetMethod;
        if (methodInfo == null) throw new ArgumentException(exceptionMsg, nameof(info));

        //IsStatic = mi.IsStatic;
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var valueConverted = Expression.Convert(value, info.PropertyType);
        var callExpr = methodInfo.IsStatic // Is this a static property
            ? Expression.Call(null, methodInfo, valueConverted)
            : Expression.Call(Expression.Convert(instance, info.DeclaringType ?? throw new NullReferenceException()), methodInfo, valueConverted);
        var action = Expression.Lambda<Action<object?, object?>>(callExpr, instance, value).Compile();

        return action;
    }

    #endregion PropertyInfo

    private static string GetTypeNamePrefix(MemberInfo info)
    {
        var type = info.ReflectedType;
        if (type == null) type = info.DeclaringType;
        if (type == null) return string.Empty;
        return type.FullNameFormatted() + ".";
    }

    #region FieldInfo

    public static bool IsSettable(this FieldInfo info) => !info.IsLiteral && !info.IsInitOnly;

    public static Func<object?, object?> CreateFieldGetter(this FieldInfo info)
    {
        // https://stackoverflow.com/a/321686
        var instance = Expression.Parameter(typeof(object), "instance");
        var fieldExpr = info.IsStatic ? Expression.Field(null, info) : Expression.Field(Expression.Convert(instance, info.DeclaringType ?? throw new NullReferenceException()), info);
        var unaryExpression = Expression.TypeAs(fieldExpr, typeof(object));
        var action = Expression.Lambda<Func<object?, object?>>(unaryExpression, instance).Compile();
        return action;
    }


    public static Action<object?, object?> CreateFieldSetter(this FieldInfo info)
    {
        // Can no longer write to 'readonly' field
        // https://stackoverflow.com/questions/934930/can-i-change-a-private-readonly-field-in-c-sharp-using-reflection#comment116393125_934942
        if (!info.IsSettable()) throw new ArgumentException($"Field {GetTypeNamePrefix(info)}{info.Name} is not settable", nameof(info));

        // https://stackoverflow.com/a/321686
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");
        var valueConverted = Expression.Convert(value, info.FieldType);
        var fieldExpr = info.IsStatic ? Expression.Field(null, info) : Expression.Field(Expression.Convert(instance, info.DeclaringType ?? throw new NullReferenceException()), info);
        var assignExp = Expression.Assign(fieldExpr, valueConverted);

        var action = Expression.Lambda<Action<object?, object?>>(assignExp, instance, value).Compile();
        return action;
    }

    #endregion FieldInfo

    #region ParameterInfo

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsOut(this ParameterInfo info) => info.ParameterType.IsByRef && !info.IsOut;

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsIn(this ParameterInfo info) => info.ParameterType.IsByRef && info.IsIn;

    /// <summary>
    /// https://stackoverflow.com/a/38110036
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsRef(this ParameterInfo info) => info.ParameterType.IsByRef && !info.IsOut;

    #endregion ParameterInfo

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
    
    public static int? MetadataTokenNullable(this MemberInfo info)
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
    }
    
    public static Module? ModuleNullable(this MemberInfo info)
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

    public static bool? HasSameMetadataDefinitionAsNullable(this MemberInfo x, MemberInfo y)
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
            var b = x.HasSameMetadataDefinitionAsNullable(y);
            if (b != null) return b.Value;
        }

        if (checkMetadataToken)
        {
            var xt = x.MetadataTokenNullable();
            var yt = y.MetadataTokenNullable();

            if (xt == null && yt != null) return false;
            if (xt != null && yt == null) return false;
            if (xt != null && yt != null && xt.Value != yt.Value) return false;
        }

        if (checkModule)
        {
            var xm = x.ModuleNullable();
            var ym = y.ModuleNullable();

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
}
