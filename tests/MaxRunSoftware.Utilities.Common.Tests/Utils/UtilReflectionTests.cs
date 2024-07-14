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

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

public class UtilReflectionTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void CreateInstance()
    {
        var vRandom = Util.CreateInstanceFactory(typeof(Random))();
        Assert.NotNull(vRandom);

        var vInt = Util.CreateInstanceFactory(typeof(int))();
        Assert.NotNull(vInt);

        var vDateTime = Util.CreateInstanceFactory(typeof(DateTime))();
        Assert.NotNull(vDateTime);
    }

    public static IEnumerable<object[]> GetCount_Data
    {
        get
        {
            var list = new List<object[]>();
            void A(int? expected, object oo) => list.Add([oo, expected,]);

            A(null, null);
            A(0, new int[] { });
            A(4, new[] {"a", "bb", null, "dddd"});
            A(4, new List<string>{ "aaa", "bbb", null, "ddd" });
            A(3, new Dictionary<string, string> { ["A"]="AAA", ["B"]="BBB", ["C"]="CCC" });
            A(4, "abcd");
            A(null, 'a');
            
            return list;
        }
    }
    
    [SkippableTheory]
    [MemberData(nameof(GetCount_Data))]
    public void GetCount(object enumerable, int? expected) => Assert.Equal(expected, Util.GetCount(enumerable));
}
