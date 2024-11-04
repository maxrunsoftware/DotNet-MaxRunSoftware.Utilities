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
using System.Text.RegularExpressions;

namespace MaxRunSoftware.Utilities.Common;



/// <summary>
/// Formatting of methods for debugging
/// https://github.com/kellyelton/System.Reflection.ExtensionMethods/tree/master/System.Reflection.ExtensionMethods
/// </summary>
public static class ExtensionsReflectionMethod
{
    private static readonly string IsAnonymous_MagicTag;
    private static readonly string IsInner_MagicTag;
    public static bool IsAnonymous(this MethodBase info) => info.Name.Contains(IsAnonymous_MagicTag);
    public static bool IsInner(this MethodBase info) => info.Name.Contains(IsInner_MagicTag);

    private static string GetNameMagicTag(this MethodInfo info)
    {
        // https://stackoverflow.com/a/56496797
        var match = new Regex(">([a-zA-Z]+)__").Match(info.Name);
        if (match.Success && match.Value is { } val && !match.NextMatch().Success) return val;
        throw new ArgumentException($"Cant find magic tag of {info}");
    }

    static ExtensionsReflectionMethod()
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
    
    public static string ToStringSignature(this MethodBase method, bool invokable) => ToStringSignature_Utils.ToStringSignature_Impl(method, invokable);

    private static class ToStringSignature_Utils
    {
        public static string ToStringSignature_Impl(MethodBase method, bool invokable)
        {
            var sb = new StringBuilder();

            // Add our method accessors if it's not invokable
            if (!invokable)
            {
                sb.Append(GetMethodAccessorSignature(method));
                sb.Append(' ');
            }

            // Add method name
            sb.Append(method.Name);

            // Add method generics
            if (method.IsGenericMethod) sb.Append(BuildGenericSignature(method.GetGenericArguments()));

            // Add method parameters
            sb.Append(GetMethodArgumentsSignature(method, invokable));

            return sb.ToString();
        }
        
        private static string GetMethodAccessorSignature(MethodBase method)
    {
        var signature = string.Empty;

        if (method.IsAssembly)
        {
            signature = "internal ";
            if (method.IsFamily) signature += "protected ";
        }
        else if (method.IsPublic) signature = "public ";
        else if (method.IsPrivate) signature = "private ";
        else if (method.IsFamily) signature = "protected ";

        if (method.IsStatic) signature += "static ";

        if (method is MethodInfo methodInfo) signature += GetSignature(methodInfo.ReturnType);

        return signature;
    }

    private static string GetMethodArgumentsSignature(MethodBase method, bool invokable)
    {
        var isExtensionMethod = method.IsDefined(typeof(ExtensionAttribute), false);
        var methodParameters = method.GetParameters().AsEnumerable();

        // If this signature is designed to be invoked and it's an extension method
        // Skip the first argument
        if (isExtensionMethod && invokable) methodParameters = methodParameters.Skip(1);

        var methodParameterSignatures = methodParameters.Select(param =>
        {
            var signature = string.Empty;
            if (param.ParameterType.IsByRef) signature = "ref ";
            else if (param.IsOut) signature = "out ";
            else if (isExtensionMethod && param.Position == 0) signature = "this ";
            if (!invokable) signature += GetSignature(param.ParameterType) + " ";
            signature += param.Name;
            return signature;
        });

        var methodParameterString = "(" + string.Join(", ", methodParameterSignatures) + ")";

        return methodParameterString;
    }

    /// <summary>Get a fully qualified signature for <paramref name="type" /></summary>
    /// <param name="type">Type. May be generic or <see cref="Nullable{T}" /></param>
    /// <returns>Fully qualified signature</returns>
    private static string GetSignature(Type type)
    {
        var isNullableType = type.IsNullable(out type);
        var signature = GetQualifiedTypeName(type);
        if (type.IsGeneric()) signature += BuildGenericSignature(type.GetGenericArguments());
        if (isNullableType) signature += "?";

        return signature;
    }

    /// <summary>
    /// Takes an <see cref="IEnumerable{T}" /> and creates a generic type signature (&lt;string,
    /// string&gt; for example)
    /// </summary>
    /// <param name="genericArgumentTypes"></param>
    /// <returns>Generic type signature like &lt;Type, ...&gt;</returns>
    private static string BuildGenericSignature(IEnumerable<Type> genericArgumentTypes) => "<" + genericArgumentTypes.Select(GetSignature).ToStringDelimited(", ") + ">";

    /// <summary>
    /// Gets the fully qualified type name of <paramref name="type" />. This will use any
    /// keywords in place of types where possible (string instead of System.String for example)
    /// </summary>
    /// <param name="type"></param>
    /// <returns>The fully qualified name for <paramref name="type" /></returns>
    private static string GetQualifiedTypeName(Type type)
    {
        if (Constant.Type_PrimitiveAlias.TryGetValue(type, out var aliasString)) return aliasString;
        var signature = type.FullName.TrimOrNull() ?? type.Name;
        if (type.IsGeneric()) signature = signature.Substring(0, signature.IndexOf('`'));
        return signature;
    }
    }
    
    public static string ToStringSignature2(this MethodBase method)
    {
        var sb = new StringBuilder();
        var declaringType = method.DeclaringType;
        if (declaringType != null) sb.Append(declaringType.FullNameFormatted().Trim());
        if (sb.Length > 0) sb.Append('.');
        sb.Append(method.Name);
        var genericArguments = method.GetGenericArguments();
        if (genericArguments.Length > 0)
        {
            sb.Append('<');
            sb.Append(genericArguments.Select(o => o.Name).ToStringDelimited(", "));
            sb.Append('>');
        }

        var Parameters = method.GetParameters();
        sb.Append('(');
        if (Parameters.Length > 0)
        {
            var sb2 = new StringBuilder();
            foreach (var p in Parameters)
            {
                if (sb2.Length > 0) sb2.Append(", ");
                if (p.IsIn()) sb2.Append("in ");
                if (p.IsOut()) sb2.Append("out ");
                if (p.IsRef()) sb2.Append("ref ");
                sb2.Append(p.ParameterType.NameFormatted());
                if (p.Name != null) sb2.Append(" " + p.Name);
            }

            sb.Append(sb2);
        }

        sb.Append(')');
        return sb.ToString();
    }

    public static bool IsPropertyMethod(this MethodInfo info) => IsPropertyMethod(info, true, true);
    public static bool IsPropertyMethodGet(this MethodInfo info) => IsPropertyMethod(info, true, false);
    public static bool IsPropertyMethodSet(this MethodInfo info) => IsPropertyMethod(info, false, true);

    private static bool IsPropertyMethod(MethodInfo info, bool isCheckGet, bool isCheckSet)
    {
        (isCheckGet || isCheckSet).AssertTrue();

        // https://stackoverflow.com/a/73908
        if (!info.IsSpecialName) return false;
        if ((info.Attributes & MethodAttributes.HideBySig) == 0) return false;


        var result = true;
        if (isCheckGet && isCheckSet)
        {
            if (!info.Name.StartsWithAny("get_", "set_")) result = false;
        }
        else if (isCheckGet)
        {
            if (!info.Name.StartsWith("get_")) result = false;
        }
        else if (isCheckSet)
        {
            if (!info.Name.StartsWith("set_")) result = false;
        }

        if (!result) return false;

        // https://stackoverflow.com/a/40128143
        var reflectedType = info.ReflectedType;
        if (reflectedType != null)
        {
            var methodHashCode = info.GetHashCode();
            foreach (var p in reflectedType.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (isCheckGet)
                {
                    var m = p.GetGetMethod(true);
                    if (m != null)
                    {
                        if (m.GetHashCode() == methodHashCode) return true;
                    }
                }

                if (isCheckSet)
                {
                    var m = p.GetSetMethod(true);
                    if (m != null)
                    {
                        if (m.GetHashCode() == methodHashCode) return true;
                    }
                }
            }

            return false;
        }

        return true;
    }
    
    
    
    public static bool IsOperatorImplicit(this MethodInfo info) => IsOperator(info, "op_Implicit");
    
    public static bool IsOperatorExplicit(this MethodInfo info) => IsOperator(info, "op_Explicit");

    private static bool IsOperator(MethodInfo info, string name)
    {
        if (!info.IsStatic) return false;
        var ps = info.GetParameters();
        if (ps.Length != 1) return false;
        return StringComparer.Ordinal.Equals(info.Name, name);
    }
}
