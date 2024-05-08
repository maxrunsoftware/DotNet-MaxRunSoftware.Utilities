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

namespace MaxRunSoftware.Utilities.Common.Tests.Reflection;

public static class MethodCallerTestsUtil
{
    public static void Log(List<string> calls, Type caller, object[] args, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
    {
        var sb = new StringBuilder();
        sb.Append(caller.NameFormatted());
        sb.Append(".");
        sb.Append(memberName);
        sb.Append("(");
        var ps = new List<string>();
        args ??= [];
        for (var i = 0; i < args.Length; i++)
        {
            var a = args[i];
            var sbb = new StringBuilder();
            sbb.Append('[');
            sbb.Append(i);
            if (a != null)
            {
                sbb.Append(':');
                sbb.Append(a.GetType().NameFormatted());
            }
            sbb.Append($"] {a}");
            ps.Add(sbb.ToString());
        }
        sb.Append(ps.ToStringDelimited(", "));
        sb.Append(")");
        calls.Add(sb.ToString());
    }
}

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException
public class MethodCallerTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    
    
    
    public class MethodCallerTests_ClassA
    {
        public List<string> C { get; } = new();
        
        public void Action_0() => MethodCallerTestsUtil.Log(C, GetType(), []);
        
        public void Action_1(string a) => MethodCallerTestsUtil.Log(C, GetType(), [a,]);
    }
    

    
    [SkippableFact]
    public void Can_Invoke_Instance_Method_Action0()
    {
        var o1 = new MethodCallerTests_ClassA();
        Assert.Empty(o1.C);
        o1.Action_0();
        Assert.Single(o1.C);
        
        var o2 = new MethodCallerTests_ClassA();
        Assert.Empty(o2.C);
        var mc = GetMC(o2.GetType(), nameof(MethodCallerTests_ClassA.Action_0));
        Assert.NotNull(mc);
        
        mc.Invoke(o2, []);
        Assert.Single(o2.C);
        
        Assert.Equal(o1.C[0], o2.C[0]);
    }
    
    private MethodCaller GetMC(Type clazz, string name, Type[] argTypes = null, Type[] genericArgs = null)
    {
        //argTypes ??= [];
        var genericTypeArgs = genericArgs ?? [];
        
        var objectMethodsToSkip = typeof(object).GetMethodSlims(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).ToHashSet(); 
        var msAll = clazz.GetMethodSlims(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(o => !objectMethodsToSkip.Contains(o)).ToImmutableArray();
        log.LogInformationMethod(new(clazz, name, argTypes), "Found {Count} methods on class [{Class}]: {Methods}", msAll.Length, clazz.NameFormatted(), msAll.Select(o => o.Name).OrderByOrdinalIgnoreCaseThenOrdinal().ToStringDelimited(", "));
        
        var ms = new List<MethodSlim>();
        for (int i = 0; i < msAll.Length; i++)
        {
            log.LogInformation("");
            var m = msAll[i];
            log.LogInformation("  [{Index}] {Method}", i.ToStringRunningCount(msAll.Length), m.Name);
            
            var checkName = name.EqualsOrdinal(m.Name);
            log.LogInformation("    " + nameof(checkName) + ": {Name1} {Equals} {Name2}", name, checkName ? "==" : "!=", m.Name);
            if (!checkName)
            {
                log.LogInformation("      failed");
                continue;
            }
            
            if (argTypes != null)
            {
                var checkArgsLength = argTypes.Length == m.Parameters.Length;
                log.LogInformation("    " + nameof(checkArgsLength) + ": {ArgsLength1} {Equals} {ArgsLength2}", argTypes.Length, checkArgsLength ? "==" : "!=", m.Parameters.Length);
                if (!checkArgsLength)
                {
                    log.LogInformation("      failed");
                    continue;
                }
                
                var parameterTypes = m.Parameters.Select(oo => (Type)oo.Parameter.Type).ToArray();
                var checkArgsTypes = argTypes.SequenceEqual(parameterTypes);
                log.LogInformation("    " + nameof(checkArgsTypes) + ": {ArgsTypes1} {Equals} {ArgsTypes2}",
                    "(" + argTypes.Select(o => o.NameFormatted()).ToStringDelimited(", ") + ")",
                    checkArgsTypes ? "==" : "!=",
                    "(" + parameterTypes.Select(o => o.NameFormatted()).ToStringDelimited(", ") + ")"
                );
                if (!checkArgsTypes)
                {
                    log.LogInformation("      failed");
                    continue;
                }
            }
            
            if (m.GenericArguments.Length > 0)
            {
                var checkGenericArgsLength = genericTypeArgs.Length == m.GenericArguments.Length;
                log.LogInformation("    " + nameof(checkGenericArgsLength) + ": {GenericTypeArgsLength1} {Equals} {GenericTypeArgsLength2}", genericTypeArgs.Length, checkGenericArgsLength ? "==" : "!=", m.GenericArguments.Length);
                if (!checkGenericArgsLength)
                {
                    log.LogInformation("      failed");
                    continue;
                }
            }
            
            log.LogInformation("    MATCH");
            ms.Add(m);
        }
        
        if (ms.Count < 1) throw new MissingMethodException($"Could not find method {clazz.NameFormatted()}.{name}");
        if (ms.Count > 1) throw new MissingMethodException($"Found multiple ({ms.Count}) methods {clazz.NameFormatted()}.{name}");
        return ms.First().GetMethodCaller(genericTypeArguments: genericArgs);
    }
    
    [SkippableFact]
    public void Can_Invoke_Instance_Method_Action1()
    {
        var o1 = new MethodCallerTests_ClassA();
        Assert.Empty(o1.C);
        o1.Action_1("foo");
        Assert.Single(o1.C);
        
        var o2 = new MethodCallerTests_ClassA();
        Assert.Empty(o2.C);
        var mc = GetMC(o2.GetType(), nameof(MethodCallerTests_ClassA.Action_1));
        Assert.NotNull(mc);
        mc.Invoke(o2, ["foo", ]);
        Assert.Single(o2.C);
        
        Assert.Equal(o1.C[0], o2.C[0]);
    }
    
    
    
    [SkippableFact]
    public void Can_Invoke_Extension_Method_Generic()
    {
        // ReSharper disable once InvokeAsExtensionMethod
        var o1 = MethodCallerTests_Class_Static.AddOptions<Uri>("foo");
        Assert.NotNull(o1);
        Assert.Equal(typeof(Uri), o1.GType);
        Assert.Equal("foo", o1.Text);
        Assert.Equal(typeof(MethodCallerTests_Class_Static_Builder<Uri>), o1.GetType());
        
        var mc = GetMC(typeof(MethodCallerTests_Class_Static), nameof(MethodCallerTests_Class_Static.AddOptions), [typeof(string),], [typeof(Uri),]);
        Assert.NotNull(mc);
        var oo2 = mc.Invoke(null, ["foo",]);
        Assert.NotNull(oo2);
        var o2 = (MethodCallerTests_Class_Static_Builder<Uri>)oo2;
        Assert.Equal(typeof(Uri), o2.GType);
        Assert.Equal("foo", o2.Text);
        Assert.Equal(typeof(MethodCallerTests_Class_Static_Builder<Uri>), o2.GetType());

    }
    
    [SkippableFact]
    public void Can_Invoke_Extension_Method_Generic_Call_ReturnValue()
    {
        // ReSharper disable once InvokeAsExtensionMethod
        var o1 = MethodCallerTests_Class_Static.AddOptions<Uri>("foo");
        var o1Msg = o1.DoSomething("bar");
        Assert.Equal("bar", o1Msg);
        Assert.Equal(typeof(MethodCallerTests_Class_Static_Builder<Uri>), o1.GetType());
        
        var mc1 = GetMC(typeof(MethodCallerTests_Class_Static), nameof(MethodCallerTests_Class_Static.AddOptions), [typeof(string),], [typeof(Uri),]);
        var o2 = (MethodCallerTests_Class_Static_Builder<Uri>)mc1.Invoke(null, ["foo",]);
        Assert.Equal(o1.GetType(), o2.GetType());
        var mcmcList = mc1.MethodInfo.ReturnType.GetMethodSlims(BindingFlags.Public | BindingFlags.Instance)
            .Where(o => o.Name.EqualsOrdinal(nameof(MethodCallerTests_Class_Static_Builder<object>.DoSomething)))
            .Where(o => o.Parameters.Select(oo => oo.Parameter.Type.Type).SequenceEqual([typeof(string),]))
            .ToList();
        
        Assert.Single(mcmcList);
        var mc2 = mcmcList.First().GetMethodCaller();
        var cObject = o2.GetType().GetPropertySlim(nameof(MethodCallerTests_Class_Static_Builder<object>.C)).GetValue(o2);
        Assert.NotNull(cObject);
        
        Assert.Empty((ICollection<string>)cObject);
        var o2Msg = mc2.Invoke(o2, ["bar", ]);
        Assert.Equal("bar", o2Msg);
        Assert.Single((ICollection<string>)cObject);
        
        
        
        
        
        
    }


   
}


public static class MethodCallerTests_Class_Static
{
    public static MethodCallerTests_Class_Static_Builder<TOptions> AddOptions<TOptions>(this string text) where TOptions : class
    {
        return new() { Text=text };
    }
    
    
}

public class MethodCallerTests_Class_Static_Builder<TOptions> where TOptions : class
{
    public List<string> C { get; } = new();
    
    public required string Text { get; init; }
    public Type GType => typeof(TOptions);
    
    public string DoSomething(string someMsg)
    {
        MethodCallerTestsUtil.Log(C, GetType(), [someMsg, ]);
        return someMsg;
    }
    
    public string DoSomething(object someMsg)
    {
        MethodCallerTestsUtil.Log(C, GetType(), [someMsg, ]);
        return "someMsg is object: " + someMsg;
    }
    
}
