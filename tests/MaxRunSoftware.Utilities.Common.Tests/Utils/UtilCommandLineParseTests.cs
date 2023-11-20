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

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

#nullable enable

public class CommandLineArgumentParserTests : TestBase
{
    public CommandLineArgumentParserTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    private CommandLineParseResult Parse(char optId, char optDelim, bool caseSensitive, params string?[]? args) =>
        Util.CommandLineParse(
            optionIdentifier: optId,
            optionDelimiter: optDelim,
            optionsCaseSensitive: caseSensitive,
            flagsCaseSensitive: caseSensitive,
            args
            );

    private CommandLineParseResult Parse(params string?[]? args) => Parse('-', '=', false, args);

    [SkippableFact]
    public void Parse_Option()
    {
        var r = Parse("-a=1");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1", r.Options.First().Value[0]);
    }

    [SkippableFact]
    public void Parse_Option_Multiple_Identifier()
    {
        var r = Parse("----a=1");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1", r.Options.First().Value[0]);
    }

    [SkippableFact]
    public void Parse_Option_Multiple_Values()
    {
        var r = Parse("-a=42", " - a = 6 ");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        var kvp = r.Options.First();
        Assert.Equal("a", kvp.Key);
        Assert.Equal(2, kvp.Value.Count);
        Assert.Equal("42", kvp.Value[0]);
        Assert.Equal("6", kvp.Value[1]);
    }

    [SkippableFact]
    public void Parse_Option_Whitespace_And_Multiple_In_Identifier()
    {
        var r = Parse(" - - a=1");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1", r.Options.First().Value[0]);
    }

    [SkippableFact]
    public void Parse_Option_No_Identifier()
    {
        var r = Parse("  a=1");
        Assert.Empty(r.Options);
    }

    [SkippableFact]
    public void Parse_OptionFlag_No_Name_Should_Be_Argument()
    {
        var r = Parse("-=1");
        Assert.Empty(r.Options);
        Assert.Empty(r.Flags);
        Assert.Single(r.Arguments);
        Assert.Equal("-=1", r.Arguments.First());
    }

    [SkippableFact]
    public void Parse_OptionFlag_No_Name_With_Spaces_Should_Be_Argument()
    {
        var r = Parse(" - = 1 ");
        Assert.Empty(r.Options);
        Assert.Empty(r.Flags);
        Assert.Single(r.Arguments);
        Assert.Equal("- = 1", r.Arguments.First());
    }

    [SkippableFact]
    public void Parse_Option_Name_Keep_Whitespace_In_Middle()
    {
        var r = Parse(" - - a b c = 1 ");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a b c", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1", r.Options.First().Value[0]);
    }

    [SkippableFact]
    public void Parse_Option_Value_Keep_Whitespace_In_Middle()
    {
        var r = Parse(" - - a b c = 1 2 3");
        Assert.Single(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a b c", r.Options.First().Key);
        Assert.Single(r.Options.First().Value);
        Assert.Equal("1 2 3", r.Options.First().Value[0]);
    }


    [SkippableFact]
    public void Parse_Flag()
    {
        var r = Parse("-a");
        Assert.Empty(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Single(r.Flags);
        Assert.Equal("a", r.Flags.First());
    }

    [SkippableFact]
    public void Parse_Flag_With_Delimiter()
    {
        var r = Parse("- a =  ");
        Assert.Empty(r.Options);
        Assert.Empty(r.Arguments);
        Assert.Single(r.Flags);
        Assert.Equal("a", r.Flags.First());
    }

    [SkippableFact]
    public void Parse_Flag_With_Multiple_Delimiter_Should_Not_Be_Flag()
    {
        var r = Parse("-a==");
        Assert.Empty(r.Flags);
    }

    [SkippableFact]
    public void Parse_Flag_No_Identifier()
    {
        var r = Parse("a=");
        Assert.Empty(r.Flags);
    }

    [SkippableFact]
    public void Parse_Argument()
    {
        var r = Parse(" a b c = 1 2 3  ");
        Assert.Empty(r.Options);
        Assert.Single(r.Arguments);
        Assert.Empty(r.Flags);
        Assert.Equal("a b c = 1 2 3", r.Arguments.First());
    }


}
