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

using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MaxRunSoftware.Utilities.Common;

public static class ExtensionsNumber
{
    private static readonly ConcurrentDictionary<Type, bool> isFloatingPoint = new();
    
    public static bool IsIFloatingPoint<T>(this T value) where T : INumber<T>
    {
        // https://learn.microsoft.com/en-us/dotnet/api/system.numerics.ifloatingpoint-1?view=net-8.0
        
        if (value is Decimal
            || value is Double
            || value is Half
            || value is NFloat
            || value is Single
           ) return true;
        
        return isFloatingPoint.GetOrAdd(typeof(T), static t => ImplementsInterfaceGeneric(t, typeof(IFloatingPoint<>)));
    }
    
    /// <summary>
    /// For checking whether the specified type implements a specific interface. Mainly useful for generic interfaces because
    /// you cannot "if (value is INumber&lt;T&gt;)"
    /// https://stackoverflow.com/a/76539302
    /// https://stackoverflow.com/a/76820581
    /// </summary>
    /// <param name="type"></param>
    /// <param name="otherType"></param>
    /// <returns></returns>
    private static bool ImplementsInterfaceGeneric(this Type type, Type otherType)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == otherType);
    }
    
    
    

}
