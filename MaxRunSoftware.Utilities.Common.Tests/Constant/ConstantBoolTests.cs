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

namespace MaxRunSoftware.Utilities.Common.Tests;

public class ConstantBoolTests : TestBase
{
    public ConstantBoolTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public void TestTrue()
    {
        var vals = new[]
        {
            "TRUE", "true", "True",
            "T", "t",
            "1",
            "Y", "y",
            "YES", "yes", "yEs"
        };
        foreach (var val in vals)
        {
            Assert.Contains(val, Constant.Bool_True);
        }
    }

    [Fact]
    public void TestFalse()
    {
        var vals = new[]
        {
            "FALSE", "false", "False",
            "F", "f",
            "0",
            "N", "n",
            "NO", "no", "nO"
        };
        foreach (var val in vals)
        {
            Assert.Contains(val, Constant.Bool_False);
        }
    }

}
