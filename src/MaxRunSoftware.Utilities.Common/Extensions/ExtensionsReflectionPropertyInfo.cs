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

public static class ExtensionsReflectionPropertyInfo
{
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


    public static bool IsGettable(this PropertyInfo info, bool includeNonPublic = false) => info.CanRead && info.GetGetMethod(includeNonPublic) != null;

    public static bool IsSettable(this PropertyInfo info, bool includeNonPublic = false) => info.CanWrite && info.GetSetMethod(includeNonPublic) != null;

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
        var exceptionMsg = $"Property {ExtensionsReflectionMemberInfo.GetTypeNamePrefix(info)}{info.Name} is not gettable";
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
        var exceptionMsg = $"Property {ExtensionsReflectionMemberInfo.GetTypeNamePrefix(info)}{info.Name} is not settable";
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

   
}
