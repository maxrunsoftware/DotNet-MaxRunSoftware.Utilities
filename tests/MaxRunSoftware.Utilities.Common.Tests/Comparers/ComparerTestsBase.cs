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

public abstract class ComparerTestsData<T, TComparer>
    where TComparer : ComparerBase<T> where T : class
{
    public abstract IEnumerable<TComparer> Comparers { get; }
    public abstract IEnumerable<(T, T)> Same { get; }
}

public abstract class ComparerTestsBase<T, TComparer, TData>(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
    where TComparer : ComparerBase<T> where T : class
    where TData : ComparerTestsData<T, TComparer>, new()
{
    [SkippableTheory]
    [MemberData(nameof(Same_Data))]
    public void Same_Equals(TComparer comparer, T x, T y) => Assert.True(comparer.Equals(x, y));

    [SkippableTheory]
    [MemberData(nameof(Same_Data))]
    public void Same_HashCode(TComparer comparer, T x, T y) => Assert.Equal(comparer.GetHashCode(x), comparer.GetHashCode(y));

    [SkippableTheory]
    [MemberData(nameof(Same_Data))]
    public void Same_Compare(TComparer comparer, T x, T y) => Assert.Equal(0, comparer.Compare(x, y));

    public static IEnumerable<object[]> Same_Data
    {
        get
        {
            var items = new List<object[]>();
            var data = new TData();

            foreach (var comparer in data.Comparers)
            {
                foreach (var xy in data.Same)
                {
                    items.Add([comparer, xy.Item1, xy.Item2,]);
                }
            }

            return items;
        }
    }

}
