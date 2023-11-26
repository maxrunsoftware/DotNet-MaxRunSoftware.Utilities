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

// ReSharper disable UseNameOfInsteadOfTypeOf

namespace MaxRunSoftware.Utilities.Common.Tests.Extensions;

public class ExtensionsCollectionTests : TestBase
{
    public ExtensionsCollectionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableFact]
    public void Permutate()
    {
        var array = new[] { "a", "b", "c" };
        var permutations = array.Permutate().ToArray();
        Assert.Equal(6, permutations.Length);
        Assert.Contains(new[] { "a", "b", "c" }, permutations);
        Assert.Contains(new[] { "a", "c", "b" }, permutations);
        Assert.Contains(new[] { "b", "a", "c" }, permutations);
        Assert.Contains(new[] { "b", "c", "a" }, permutations);
        Assert.Contains(new[] { "c", "a", "b" }, permutations);
        Assert.Contains(new[] { "c", "b", "a" }, permutations);

    }
}