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
    public Delegate Delegate { get; }
    
    public object? Invoke(object? instance, object?[] args)
    {
        if (MethodInfo.ReturnType == typeof(void))
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
    
    
    public MethodCaller(MethodInfo methodInfo)
    {
        var m = MethodInfo = methodInfo;
        
        /*
        if (returnType == typeof(void)) DefaultNullValue = null;
        else if (returnType.IsPrimitive || returnType.IsValueType || returnType.IsEnum) { DefaultNullValue = Activator.CreateInstance(returnType); }
        else { DefaultNullValue = null; }
        */
        
        
        // https://stackoverflow.com/a/2850366
        // Should not happen but if it does fail here rather than later trying to call it
        var declaringType = m.DeclaringType ?? throw new NullReferenceException("Could not determine class containing method " + m.Name);
        
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var instanceUnary = m.IsStatic ? null : Expression.Convert(instanceParameter, declaringType);
        
        var parameters = m.GetParameters();
        var argumentsParameter = new List<ParameterExpression>(parameters.Length);
        var argumentsUnary = new List<UnaryExpression>(parameters.Length);
        
        for (var i = 0; i < parameters.Length; i++)
        {
            var methodParameter = parameters[i];
            var rawObject = Expression.Parameter(typeof(object), $"arg{i+1}");
            argumentsParameter.Add(rawObject);
            
            var convertedObject = Expression.Convert(rawObject, methodParameter.ParameterType);
            argumentsUnary.Add(convertedObject);
        }
        
        
        
        Expression callExpression = Expression.Call(instanceUnary, m, argumentsUnary);
        
        if (m.ReturnType != typeof(void)) callExpression = Expression.TypeAs(callExpression, typeof(object));
        var ps = instanceParameter.Yield().Concat(argumentsParameter).ToArray();
        var lambda = Expression.Lambda(callExpression, ps);
        Delegate = lambda.Compile();
    }
}


public static class MethodCallerExtensions
{
    public static MethodCaller GetMethodCaller(this MethodInfo info, Type[]? genericTypeArguments = null) => new(
        genericTypeArguments != null && genericTypeArguments.Length > 0
            ? info.MakeGenericMethod(genericTypeArguments)
            : info
    );

    public static object? InvokeMethodCaller(
        this MethodInfo info,
        object? instance,
        object?[]? args = null,
        Type[]? genericTypeArguments = null
    ) =>
        GetMethodCaller(info, genericTypeArguments).Invoke(instance, args ?? []);
    
    public static object? GetMethodCallerValue(this MethodInfo info, object? instance) => InvokeMethodCaller(info, instance);

}
