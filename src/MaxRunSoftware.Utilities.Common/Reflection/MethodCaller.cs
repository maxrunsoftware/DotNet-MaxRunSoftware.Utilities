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

using System.Linq.Expressions;

namespace MaxRunSoftware.Utilities.Common;

public sealed class MethodCaller
{
    public MethodInfo MethodInfo { get; }
    public IReadOnlyList<ParameterInfo> Parameters { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    public bool IsInstance => !IsStatic;
    public bool IsVoid { get; }
    public Delegate Delegate { get; }

    public object? Invoke(object? instance, params object?[] args)
    {
        if (IsVoid)
        {
            switch (args.Length)
            {
                // @formatter:off
                case 0:  ((Action<object?>)Delegate)(instance); return null;
                case 1:  ((Action<object?, object?>)Delegate)(instance, args[0]); return null;
                case 2:  ((Action<object?, object?, object?>)Delegate)(instance, args[0], args[1]); return null;
                case 3:  ((Action<object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2]); return null;
                case 4:  ((Action<object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3]); return null;
                case 5:  ((Action<object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4]); return null;
                case 6:  ((Action<object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5]); return null;
                case 7:  ((Action<object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6]); return null;
                case 8:  ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]); return null;
                case 9:  ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]); return null;
                case 10: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]); return null;
                case 11: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]); return null;
                case 12: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]); return null;
                case 13: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]); return null;
                case 14: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]); return null;
                case 15: ((Action<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]); return null;
                default: throw new NotImplementedException("Do not support VOID with " + args.Length + " arguments");
                // @formatter:on
            }
        }

        switch (args.Length)
        {
            // @formatter:off
            case 0:  return ((Func<object?, object?>)Delegate)(instance);
            case 1:  return ((Func<object?, object?, object?>)Delegate)(instance, args[0]);
            case 2:  return ((Func<object?, object?, object?, object?>)Delegate)(instance, args[0], args[1]);
            case 3:  return ((Func<object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2]);
            case 4:  return ((Func<object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3]);
            case 5:  return ((Func<object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4]);
            case 6:  return ((Func<object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5]);
            case 7:  return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            case 8:  return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            case 9:  return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
            case 10: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
            case 11: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
            case 12: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
            case 13: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
            case 14: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
            case 15: return ((Func<object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?, object?>)Delegate)(instance, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
            default: throw new NotImplementedException("Do not support OBJECT with " + args.Length + " arguments");
            // @formatter:on
        }
    }

    public MethodCaller(MethodInfo info)
    {
        MethodInfo = info.CheckNotNull(nameof(info));
        Name = info.Name;
        IsStatic = info.IsStatic;

        var returnType = info.ReturnType;
        IsVoid = returnType == typeof(void);

        /*
        if (returnType == typeof(void)) DefaultNullValue = null;
        else if (returnType.IsPrimitive || returnType.IsValueType || returnType.IsEnum) { DefaultNullValue = Activator.CreateInstance(returnType); }
        else { DefaultNullValue = null; }
        */

        var declaringType = info.DeclaringType;
        // Should not happen but if it does fail here rather then later trying to call it
        if (declaringType == null) throw new NullReferenceException("Could not determine class containing method " + Name);

        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var instanceUnary = IsStatic ? null : Expression.Convert(instanceParameter, declaringType);

        Parameters = info.GetParameters().ToList().AsReadOnly();
        var argumentsParameter = new List<ParameterExpression>(Parameters.Count);
        var argumentsUnary = new List<UnaryExpression>(Parameters.Count);

        for (var i = 0; i < Parameters.Count; i++)
        {
            var methodParameter = Parameters[i];
            var rawObject = Expression.Parameter(typeof(object), "arg" + (i + 1));
            argumentsParameter.Add(rawObject);

            var convertedObject = Expression.Convert(rawObject, methodParameter.ParameterType);
            argumentsUnary.Add(convertedObject);
        }

        Expression callExpression = Expression.Call(instanceUnary, info, argumentsUnary);
        if (!IsVoid) callExpression = Expression.TypeAs(callExpression, typeof(object));
        var lambda = Expression.Lambda(callExpression, instanceParameter.Yield().Concat(argumentsParameter).ToArray());
        Delegate = lambda.Compile();
    }

    public static IReadOnlyList<MethodCaller> GetMethodCallers(Type classType, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) => classType.GetMethods(flags).Select(o => new MethodCaller(o)).ToList();

    public static MethodCaller? GetMethodCaller(Type classType, string methodName, params Type[] argumentTypes)
    {
        foreach (var caller in GetMethodCallers(classType))
        {
            if (caller.Name != methodName) continue;
            if (caller.Parameters.Count != argumentTypes.Length) continue;

            var allMatch = true;
            for (var i = 0; i < argumentTypes.Length; i++)
            {
                if (caller.Parameters[i].ParameterType != argumentTypes[i]) allMatch = false;
                if (!allMatch) break;
            }

            if (!allMatch) continue;

            return caller;
        }

        return null;
    }

    // @formatter:off
    public static MethodCaller? GetMethodCaller<T1>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1));
    public static MethodCaller? GetMethodCaller<T1, T2>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2));
    public static MethodCaller? GetMethodCaller<T1, T2, T3>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15));
    public static MethodCaller? GetMethodCaller<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type classType, string methodName) => GetMethodCaller(classType, methodName, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16));
    // @formatter:on
}
