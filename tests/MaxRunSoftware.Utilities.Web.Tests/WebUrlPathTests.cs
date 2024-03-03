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

using MaxRunSoftware.Utilities.Web.Server;

// ReSharper disable AssignNullToNotNullAttribute

namespace MaxRunSoftware.Utilities.Web.Tests;

public class WebUrlPathTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableTheory]
    [InlineData(true, "")]
    [InlineData(true, "    ")]
    [InlineData(true, "  \t  ")]
    [InlineData(true, "  /  ")]
    [InlineData(true, "  \t  /  ")]
    [InlineData(true, "/")]
    [InlineData(true, "//")]
    [InlineData(true, "///")]
    [InlineData(false, "aaa")]
    [InlineData(false, "aaa/")]
    [InlineData(false, "/aaa")]
    [InlineData(false, "/aaa/")]
    [InlineData(false, "//aaa")]
    [InlineData(false, "/aaa/bbb")]
    [InlineData(false, "/aaa/bbb/")]
    public void IsRoot(bool isRoot, string url)
    {
        var p = new WebUrlPath(url);

        Assert.Equal(isRoot, p.IsRoot);
    }

    [SkippableTheory]
    [InlineData(true, "", "")]
    [InlineData(true, "/", "")]
    [InlineData(true, "aaa", "/aaa/")]
    [InlineData(true, "aaa", "/")]
    [InlineData(true, "aaa/bbb", "/aaa/bbb/")]
    [InlineData(true, "aaa/bbb", "/aaa")]
    [InlineData(true, "aaa/bbb", "/")]
    [InlineData(false, "", "aaa")]
    [InlineData(false, "/", "aaa")]
    [InlineData(false, "aaa/bbb", "/aaa/bbb/ccc")]
    public void StartsWith(bool isMatch, string url, string startsWith)
    {
        var p = new WebUrlPath(url);
        Assert.Equal(isMatch, p.StartsWith(startsWith));
    }
}
