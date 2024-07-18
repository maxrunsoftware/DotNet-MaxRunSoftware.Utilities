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

namespace MaxRunSoftware.Utilities.Common.Tests.Types;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class FieldSlimTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    public class TestClassA
    {
        public int f_public_int = 1;
        public readonly int f_public_readonly_int = 2;
        public static int f_public_static_int = 3;
        public static readonly int f_public_static_readonly_int = 4;
        public const int f_public_const_int = 5;
    }

    private static (T, FieldSlim) GetFieldAndInstance<T>(string name) where T : new()
    {
        var obj = new T();
        
        var ms = obj.GetType().GetFieldSlims(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var m = ms.First(o => o.Name == name);
        return (obj, m);
    }
    
    private static FieldSlim GetField<T>(string name) where T : new()
    {
        var (_, f) = GetFieldAndInstance<T>(name);
        return f;
    }
    
    [SkippableFact]
    public void Can_Get_And_Set_Field()
    {
        var (obj, m) = GetFieldAndInstance<TestClassA>(nameof(TestClassA.f_public_int));
        obj.f_public_int = 42;
        
        var v = m.GetValue(obj);
        Assert.Equal(typeof(int), v.GetType());
        var vv = Convert.ToInt32(v);
        Assert.Equal(42, vv);
        
        m.SetValue(obj, 43);
        Assert.Equal(43, obj.f_public_int);
    }
    
    [SkippableFact]
    public void Can_Get_And_Set_Field_Static()
    {
        var m = GetField<TestClassA>(nameof(TestClassA.f_public_static_int));
        TestClassA.f_public_static_int = 42;
        
        var v = m.GetValue(null);
        Assert.Equal(typeof(int), v.GetType());
        var vv = Convert.ToInt32(v);
        Assert.Equal(42, vv);
        
        m.SetValue(null, 43);
        Assert.Equal(43, TestClassA.f_public_static_int);
    }
    
    [SkippableTheory]
    [InlineData(nameof(TestClassA.f_public_int), false)]
    [InlineData(nameof(TestClassA.f_public_readonly_int), true)]
    [InlineData(nameof(TestClassA.f_public_static_int), false)]
    [InlineData(nameof(TestClassA.f_public_static_readonly_int), true)]
    [InlineData(nameof(TestClassA.f_public_const_int), false)]
    public void IsReadonly(string name, bool expected) => Assert.Equal(expected, GetField<TestClassA>(name).IsReadonly);

    [SkippableTheory]
    [InlineData(nameof(TestClassA.f_public_int), false)]
    [InlineData(nameof(TestClassA.f_public_readonly_int), false)]
    [InlineData(nameof(TestClassA.f_public_static_int), true)]
    [InlineData(nameof(TestClassA.f_public_static_readonly_int), true)]
    [InlineData(nameof(TestClassA.f_public_const_int), true)]
    public void IsStatic(string name, bool expected) => Assert.Equal(expected, GetField<TestClassA>(name).IsStatic);

    [SkippableTheory]
    [InlineData(nameof(TestClassA.f_public_int), false)]
    [InlineData(nameof(TestClassA.f_public_readonly_int), false)]
    [InlineData(nameof(TestClassA.f_public_static_int), false)]
    [InlineData(nameof(TestClassA.f_public_static_readonly_int), false)]
    [InlineData(nameof(TestClassA.f_public_const_int), true)]
    public void IsConstant(string name, bool expected) => Assert.Equal(expected, GetField<TestClassA>(name).IsConstant);
}
