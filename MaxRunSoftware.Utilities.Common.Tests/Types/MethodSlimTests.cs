// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common.Tests.Types;

public class MethodSlimTests : TestBase
{
    public MethodSlimTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    public class TestClass
    {
        public static string TestMethodStaticA(int a)
        {
            return (a + a).ToString();
        }
    }

    private static MethodInfo GetMethod(string name)
    {
        return typeof(TestClass).GetMethods().First(o => o.Name == name);
    }
    [SkippableFact]
    public void TestMethodStaticA_Can_Create()
    {
        var mi = GetMethod(nameof(TestClass.TestMethodStaticA));
        var ms = new MethodSlim(mi);
        output.WriteLine($"[{ms.Name}] {ms.NameFull}");
    }

    [SkippableFact]
    public void TestMethodStaticA_Can_InvokeStatic()
    {
        var mi = GetMethod(nameof(TestClass.TestMethodStaticA));
        var ms = new MethodSlim(mi);
        var x = 2;
        var o = ms.InvokeStatic(x);
        Assert.NotNull(o!);
        Assert.IsType<string>(o);
        var y = (string)o!;
        output.WriteLine($"[{ms.Name}]: {y}");
    }


}
