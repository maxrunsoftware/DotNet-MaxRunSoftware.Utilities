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

namespace MaxRunSoftware.Utilities.Common;

public static partial class Util
{
    public static T? Catch<T>(Func<T> func, IEnumerable<Type> exceptionTypes, out Exception? exception)
    {
        try
        {
            var v = func();
            exception = null;
            return v;
        }
        catch (Exception e)
        {
            var eType = e.GetType();
            foreach (var exceptionType in exceptionTypes)
            {
                if (eType.IsAssignableTo(exceptionType))
                {
                    exception = e;
                    return default;
                }
            }
            throw;
        }
    }
    
    public static T? Catch<T>(Func<T> func, IEnumerable<Type> exceptionTypes) =>
        Catch(func, exceptionTypes, out _);
    
    public static T? Catch<T>(Func<T> func, out Exception? exception) => 
        Catch(func, [typeof(Exception)], out exception);

    public static T? Catch<T>(Func<T> func) => 
        Catch(func, [typeof(Exception)], out _);
    
    
    public static T Catch<T>(Func<T> func, IEnumerable<Type> exceptionTypes, out Exception? exception, T defaultValue)
    {
        try
        {
            var v = func();
            exception = null;
            return v;
        }
        catch (Exception e)
        {
            var eType = e.GetType();
            foreach (var exceptionType in exceptionTypes)
            {
                if (eType.IsAssignableTo(exceptionType))
                {
                    exception = e;
                    return defaultValue;
                }
            }
            throw;
        }
    }
    
    public static T Catch<T>(Func<T> func, IEnumerable<Type> exceptionTypes, T defaultValue) =>
        Catch(func, exceptionTypes, out _, defaultValue);
    
    public static T Catch<T>(Func<T> func, out Exception? exception, T defaultValue) => 
        Catch(func, [typeof(Exception)], out exception, defaultValue);

    public static T Catch<T>(Func<T> func, T defaultValue) => 
        Catch(func, [typeof(Exception)], out _, defaultValue);

    
    public static T? CatchN<T>(Func<T> func, IEnumerable<Type> exceptionTypes, out Exception? exception) where T : struct
    {
        try
        {
            var v = func();
            exception = null;
            return v;
        }
        catch (Exception e)
        {
            var eType = e.GetType();
            foreach (var exceptionType in exceptionTypes)
            {
                if (eType.IsAssignableTo(exceptionType))
                {
                    exception = e;
                    return null;
                }
            }
            throw;
        }
    }
    
    public static T? CatchN<T>(Func<T> func, IEnumerable<Type> exceptionTypes) where T : struct =>
        CatchN(func, exceptionTypes, out _);
    
    public static T? CatchN<T>(Func<T> func, out Exception? exception) where T : struct => 
        CatchN(func, [typeof(Exception)], out exception);

    public static T? CatchN<T>(Func<T> func) where T : struct => 
        CatchN(func, [typeof(Exception)], out _);





    
}
