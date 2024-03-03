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

namespace MaxRunSoftware.Utilities.Data;

internal static class Utils
{
    public static T CopyShallow<T>(T obj) where T : class, new()
    {
        var copy = Activator.CreateInstance(obj.GetType()).CheckNotNull();
        foreach (var property in obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(o => o.CanRead)
        )
        {
            CopyShallow(property, obj, copy);
        }

        return (T)copy;
    }

    private static void CopyShallow(PropertyInfo property, object source, object target)
    {
        void W(string msg) => Console.WriteLine($"{property.DeclaringType?.Name}.{property.Name}  {msg}");

        // TODO: handle this better for the collections
        var isCollection = false;
        if (property.PropertyType.IsGenericType)
        {
            var gtd = property.PropertyType.GetGenericTypeDefinition();
            W($"GetGenericTypeDefinition={gtd.Name}");
            if (gtd.IsAssignableTo(typeof(ICollection<>)))
            {
                isCollection = true;
            }
            else
            {
                foreach (var iface in gtd.GetInterfaces())
                {
                    gtd = iface.GetGenericTypeDefinition();
                    if (gtd.IsAssignableTo(typeof(ICollection<>)))
                    {
                        isCollection = true;
                        break;
                    }
                }
            }
        }

        var isPrimitive = !isCollection;

        W($"isCollection={isCollection}  isPrimitive={isPrimitive}");
        if (!isCollection && !isPrimitive) return; // skip non-collection non-primitives

        var value = property.GetValue(source);
        if (!isCollection)
        {
            property.SetValue(target, value);
            return;
        }

        Tuple<MethodInfo, bool>? GetMethodAdd(object obj)
        {
            var t = obj.GetType();
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.Name != nameof(ICollection<object>.Add)) continue;
                W($"Found method {m.Name}");
                var ps = m.GetParameters();
                if (ps.Length == 1) return Tuple.Create(m, false);
                if (ps.Length == 2) return Tuple.Create(m, true);
            }

            return null;
        }

        if (value == null) return;
        var targetCollection = property.GetValue(target);
        if (targetCollection == null) throw new NullReferenceException($"Property {property.DeclaringType?.Name}.{property.Name} cannot be null");
        var addMethod = GetMethodAdd(targetCollection);
        if (addMethod == null) throw new NullReferenceException($"Property {property.DeclaringType?.Name}.{property.Name} has no 'Add' method");
        foreach (var item in (IEnumerable)value)
        {
            if (addMethod.Item2)
            {
                var key = item.GetType().GetProperty(nameof(KeyValuePair<object, object>.Key))!.GetValue(item);
                var val = item.GetType().GetProperty(nameof(KeyValuePair<object, object>.Value))!.GetValue(item);
                addMethod.Item1.Invoke(targetCollection, new[] { key, val });
            }
            else
            {
                addMethod.Item1.Invoke(targetCollection, new[] { item });
            }
        }
    }
}
