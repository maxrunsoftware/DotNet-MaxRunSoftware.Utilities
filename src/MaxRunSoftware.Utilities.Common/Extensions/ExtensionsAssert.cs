// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

using System.Runtime.Serialization;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
[Serializable]
public class DebugException : Exception
{
    public DebugException() { }
    protected DebugException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public DebugException(string? message) : base(message) { }
    public DebugException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class AssertException : DebugException
{
    public AssertException() { }
    protected AssertException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public AssertException(string? message) : base(message) { }
    public AssertException(string? message, Exception? innerException) : base(message, innerException) { }
}

public static class ExtensionsAssert
{
    public static void AssertTrue(this bool obj)
    {
#if DEBUG
        if (!obj) throw new AssertException("Expected value to be true");
#endif
    }

    public static void AssertFalse(this bool obj)
    {
#if DEBUG
        if (obj) throw new AssertException("Expected value to be false");
#endif
    }

    public static void AssertNotNull(this object? obj)
    {
#if DEBUG
        if (obj == null) throw new AssertException("Expected value to not be null");
#endif
    }

    public static void AssertIsType(this object obj, bool isExactType, params Type[] types)
    {
#if DEBUG
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
#endif
    }

    public static void AssertEmpty<T>(this T obj)
    {
#if DEBUG
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
#endif
    }
}
