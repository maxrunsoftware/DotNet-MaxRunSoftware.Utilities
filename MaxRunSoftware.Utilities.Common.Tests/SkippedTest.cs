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

namespace MaxRunSoftware.Utilities.Common.Tests;

[PublicAPI]
public class SkippedTest
{
    public Type Clazz { get; }
    public string? Method { get; }
    public string Message { get; }

    public SkippedTest(Type clazz, string? method, string message)
    {
        Clazz = clazz;
        Method = method;
        Message = message;
    }

    public SkippedTest(Type clazz, string message) : this(clazz, null, message) { }

    public bool IsMatch(Type? testClazz, string? testMethod)
    {
        if (testClazz == null) return false;
        if (Clazz != testClazz) return false;
        if (Method == null) return true;
        if (string.Equals(Method, testMethod, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    public void CheckSkip(Type? testClazz, string? testMethod) => Skip.If(IsMatch(testClazz, testMethod), Message);

    public static SkippedTest Create<T>(string methodName, string message) => new(typeof(T), methodName, message);
    public static SkippedTest Create<T>(string message) => new(typeof(T), null, message);
}
