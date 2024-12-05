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

namespace MaxRunSoftware.Utilities.Common.Tests.Comparers;

public class AssemblyNameComparerTests(ITestOutputHelper testOutputHelper) : ComparerTestsBase<
    AssemblyName,
    AssemblyNameComparer,
    AssemblyNameComparerTests.Data
>(testOutputHelper)
{
    public class Data : ComparerTestsData<AssemblyName, AssemblyNameComparer>
    {
        public override IEnumerable<AssemblyNameComparer> Comparers => [AssemblyNameComparer.Default, new(),];

        public override IEnumerable<(AssemblyName, AssemblyName)> Same =>
        [
            (Get<int>(), Get<int>()),
            (Get<Percent>(), Get<Percent>()),
            (Get<ITestOutputHelper>(), Get<ITestOutputHelper>()),
        ];

        private static AssemblyName Get<T>() => typeof(T).Assembly.GetName();
    }
}
