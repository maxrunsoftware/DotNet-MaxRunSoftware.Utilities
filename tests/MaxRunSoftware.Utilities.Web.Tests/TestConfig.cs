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

namespace MaxRunSoftware.Utilities.Web.Tests;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo
public static class TestConfig
{
    public static readonly ImmutableArray<SkippedTest> IGNORED_TESTS = [
        SkippedTest.Create<GenHTTPServerSiteAuthTests>("these start a web server for demo"),
    ];

    public static readonly string DEFAULT_HOST = "localhost";
    public static readonly ushort DEFAULT_PORT = 32913;

    public static readonly string DEFAULT_USERNAME = "test";
    public static readonly string DEFAULT_PASSWORD = "testPass1!";

    public static readonly string URL_BASE = $"http://{DEFAULT_HOST}:{DEFAULT_PORT}";
    
    public static readonly FrozenDictionary<string, ImmutableArray<byte>> RESOURCES = RESOURCES_Build();
    
    private static FrozenDictionary<string, ImmutableArray<byte>> RESOURCES_Build()
    {
        var resourceAsm = typeof(TestConfig).Assembly;
        var resourceDir = "Resources";
        var resourceExts = "png jpg jpeg js css".Split(' ').TrimOrNull().WhereNotNull().Select(o => "." + o).ToArray();
        var d = new Dictionary<string, ImmutableArray<byte>>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var resourceName in resourceAsm.GetEmbeddedResourceNames())
        {
            if (!resourceName.StartsWith(resourceDir)) continue;
            
            var resourceParts = resourceName.Split(new[] { '/', '\\', }).TrimOrNull().WhereNotNull().ToArray();
            if (resourceParts.Length < 2) continue;
            if (resourceParts[0].EqualsOrdinal(resourceDir)) continue;
            
            var filename = resourceParts.Last();
            if (!filename.EndsWithAny(StringComparison.OrdinalIgnoreCase, resourceExts)) continue;
            
            d.Add(filename, resourceAsm.GetEmbeddedResource(resourceName).ToImmutableArray());
        }
        
        
        return d.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }
}
