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

namespace MaxRunSoftware.Utilities.Common;

public abstract class ReadOnlyListBase<T>(IReadOnlyList<T> list) : IReadOnlyList<T>
{
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();

    public int Count => list.Count;

    public T this[int index] => list[index];
}
