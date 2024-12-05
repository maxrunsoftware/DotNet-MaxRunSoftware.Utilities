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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace MaxRunSoftware.Utilities.Common.Tests.Data;

[SuppressMessage("Assertions", "xUnit2013:Do not use equality check to check for collection size.")]
public class JsonElementTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void ToJson()
    {
        var element = new JsonObject
        {
            Properties = new Dictionary<string, JsonElement>
            {
                ["A"] = new JsonValue("AA"),
                ["B"] = new JsonArray("BB", "BBB", "BBBB"),
            },
        };
        var json = element.ToJson(new JsonWriterOptions { Indented = false });
        Assert.Equal("{'A':'AA','B':['BB','BBB','BBBB']}".Replace('\'', '"'), json);
    }
}
