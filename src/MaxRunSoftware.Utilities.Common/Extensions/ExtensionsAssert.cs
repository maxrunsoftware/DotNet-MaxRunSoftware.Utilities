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

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
[Serializable]
public class DebugException : Exception
{
    public DebugException() { }
    // protected DebugException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public DebugException(string? message) : base(message) { }
    public DebugException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class AssertException : DebugException
{
    public AssertException() { }
    // protected AssertException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public AssertException(string? message) : base(message) { }
    public AssertException(string? message, Exception? innerException) : base(message, innerException) { }
}

public static class ExtensionsAssert
{
    private static volatile bool isEnabled = Constant.IsDebug;
    
    public static bool IsEnabled { get => isEnabled; set => isEnabled = value; }
    
    public static void AssertTrue(this bool obj)
    {
        if (!IsEnabled) return;
        if (!obj) throw new AssertException("Expected value to be true");
    }

    public static void AssertFalse(this bool obj)
    {
        if (!IsEnabled) return;
        if (obj) throw new AssertException("Expected value to be false");
    }

    public static void AssertNotNull(this object? obj)
    {
        if (!IsEnabled) return;
        if (obj == null) throw new AssertException("Expected value to not be null");
    }

    public static void AssertIsType(this object obj, bool isExactType, params Type[] types)
    {
        if (!IsEnabled) return;
        AssertNotNull(obj);
        if (obj == null) return; // don't throw NRE

        if (types == null || types.Length == 0) throw new AssertException("No types specified to match against");

        var objType = obj as Type ?? obj.GetType();
        foreach (var targetType in types)
        {
            if (targetType == null) continue;
            if (isExactType)
            {
                if (objType == targetType) return;
            }
            else
            {
                if (objType.IsAssignableTo(targetType)) return;
            }
        }

        throw new AssertException($"Object {obj} is not assignable to any of the types ({types.Length}) specified: " + types.Select(o => o.NameFormatted()).ToStringDelimited(" | "));
    }

    public static void AssertEmpty<T>(this T obj)
    {
        if (!IsEnabled) return;
        if (obj is string str && str.Length > 0) throw new AssertException("Expected collection to be empty");
        if (obj is ICollection collection && collection.Count > 0) throw new AssertException("Expected collection to be empty");
        if (obj is Array array)
        {
            foreach (var _ in array) throw new AssertException("Expected array to be empty");
            return;
        }

        if (obj is IEnumerable enumerable)
        {
            foreach (var _ in enumerable) throw new AssertException("Expected enumerable to be empty");
        }
    }
    
    public static void AssertIsEqual<T>(this T x, T y)
    {
        if (!IsEnabled) return;
        if (ReferenceEquals(x, y)) return;
        if (ReferenceEquals(x, null))
        {
            if (ReferenceEquals(y, null)) return; // both null
            throw new AssertException($"T:[{typeof(T).FullNameFormatted()}] {nameof(x)}:null != {nameof(y)}:{y}");
        }
        if (ReferenceEquals(y, null)) throw new AssertException($"T:[{typeof(T).FullNameFormatted()}] {nameof(x)}:{x} != {nameof(y)}:null");
        if (!x.Equals(y)) throw new AssertException($"T:[{typeof(T).FullNameFormatted()}] {nameof(x)}:{x} != {nameof(y)}:{y} ");
    }
}
