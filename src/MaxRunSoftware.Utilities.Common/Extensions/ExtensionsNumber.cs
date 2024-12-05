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
    
    private static bool ImplementsInterfaceGeneric(this Type type, Type otherType) => Constant.ImplementsInterfaceGeneric(type, otherType);


    public static TimeSpan GetMedian(this IEnumerable<TimeSpan> items) {
        // https://stackoverflow.com/a/8328226
        var sortedNumbers = items.Select(o => o.Ticks).ToArray();
        Array.Sort(sortedNumbers);

        //get the median
        var size = sortedNumbers.Length;
        var mid = size / 2;
        long median;
        if (size == 1)
        {
            median = sortedNumbers[0];
        }
        else if (size % 2 == 0)
        {
            median = sortedNumbers[mid];
            median += sortedNumbers[mid - 1];
            median /= 2;
        }
        else
        {
            median = sortedNumbers[mid];
        }
        return TimeSpan.FromTicks(median);
    }
    
    public static T GetMedian<T>(this IEnumerable<T> items) where T : INumberBase<T>, IDivisionOperators<T, T, T> {
        
        // https://stackoverflow.com/a/8328226
        var sortedNumbers = items.ToArray();
        Array.Sort(sortedNumbers);

        //get the median
        var size = sortedNumbers.Length;
        var mid = size / 2;
        T median;
        if (size == 1)
        {
            median = sortedNumbers[0];
        }
        else if (size % 2 == 0)
        {
            median = sortedNumbers[mid];
            median += sortedNumbers[mid - 1];
            median /= (T.One + T.One);
        }
        else
        {
            median = sortedNumbers[mid];
        }
        return median;
    }
    

}
