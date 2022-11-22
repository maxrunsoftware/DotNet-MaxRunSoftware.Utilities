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

// ReSharper disable StringLiteralTypo
namespace MaxRunSoftware.Utilities.Tests.Collections;

public class SortingTests : TestBase
{
    public SortingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }


    [Fact]
    public void Sort_Correctly()
    {
        var list = "AazZCcbByYXx".ToCharArray().Select(o => o.ToString()).ToList();
        list.Sort(StringComparerOrdinalThenOrdinalIgnoreCase.INSTANCE);
        var items = list.ToStringDelimited("");
        Assert.Equal("ABCXYZabcxyz", items);


        list = "AazZCcbByYXx".ToCharArray().Select(o => o.ToString()).ToList();
        list.Sort(Constant.StringComparer_OrdinalIgnoreCase_Ordinal);
        items = list.ToStringDelimited("");
        Assert.Equal("AaBbCcXxYyZz", items);



    }
}
