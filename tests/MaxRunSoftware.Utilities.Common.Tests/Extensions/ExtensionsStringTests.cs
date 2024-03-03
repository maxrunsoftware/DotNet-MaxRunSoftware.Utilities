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

// ReSharper disable UseNameOfInsteadOfTypeOf

// ReSharper disable PossibleNullReferenceException

namespace MaxRunSoftware.Utilities.Common.Tests.Extensions;

public class ExtensionsStringTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    [SkippableTheory]
    [InlineData("Hello World", ' ', StringSplitOptions.None)]
    [InlineData("Hello  World", ' ', StringSplitOptions.None)]
    public void SplitOnWhitespaceTests(string str, char c, StringSplitOptions options)
    {
        for (var i = 0; i < 20; i++)
        {
            log.LogInformation("i={Index}", i);
            var x = str.Split(c, i, options);
            var y = str.SplitOnWhiteSpace(i, options);
            Assert.Equal(x, y);
        }
    }
}
