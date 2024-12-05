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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MaxRunSoftware.Utilities.Microsoft.Tests;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class OptionsAttributeTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    private const string TestOptions_SectionName = "MyApp:OptionsSection";
    
    [Options(TestOptions_SectionName)]
    public class TestOptions
    {
        public required string Host { get; set; }
        public uint Port { get; set; }
    }
    
    
    public class TestService(IOptions<TestOptions> os)
    {
        public string HostProp { get; } = os.Value.Host;
        public uint PortProp { get; } = os.Value.Port;
    }

    
    [SkippableFact]
    public void Can_Find_TestOptions()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
           [TestOptions_SectionName + ":Host"] = "foo",
           [TestOptions_SectionName + ":Port"] = "21",
        });
        var s = builder.Services;
        
        s.AddSingleton<IConfiguration>(builder.Configuration);
        s.AddSingleton<TestService>();
        s.AddOptionsAndBind(typeof(TestOptions).Assembly);
        
        var host = builder.Build();
        var ts = host.Services.GetRequiredService<TestService>();
        Assert.Equal("foo", ts.HostProp);
        Assert.Equal(21, (int)ts.PortProp);

    }

   
}
