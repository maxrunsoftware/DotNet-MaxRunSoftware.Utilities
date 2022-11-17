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

namespace MaxRunSoftware.Utilities.Tests;

public class CommandLineArgumentParserTests : TestBase
{
    public CommandLineArgumentParserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Fact]
    public void ParseSimple()
    {
        var opts = new CommandLineArgumentsParserOptions('=', '-', false);
        var r = CommandLineArgumentsParser.Parse(opts, "-a=1", "b=2", "-c");
        Assert.Equal(3, r.Args.Count);
        Assert.Single(r.Options);
        Assert.Single(r.Arguments);
        Assert.Single(r.Flags);

        Assert.Equal("a", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1", r.Options.First().Value[0]);

        Assert.Equal("b", r.Arguments[0]);

        Assert.Equal("c", r.Flags[0]);

    }
}
