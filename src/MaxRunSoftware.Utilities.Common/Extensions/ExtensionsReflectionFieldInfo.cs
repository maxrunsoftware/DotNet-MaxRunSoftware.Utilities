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

public static class ExtensionsReflectionFieldInfo
{
 
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
        if (!info.IsSettable()) throw new ArgumentException($"Field {ExtensionsReflectionMemberInfo.GetTypeNamePrefix(info)}{info.Name} is not settable", nameof(info));

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


}
