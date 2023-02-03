// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common;

public class TableMemory : ITable
{
    public TableMemory(IReadOnlyList<IColumn>? columns, IReadOnlyList<IRow> rows)
    {
        Columns = columns;
        Rows = rows;
    }

    public IReadOnlyList<IColumn>? Columns { get; }
    public IReadOnlyList<IRow> Rows { get; }
}

public class ColumnMemory : IColumn
{
    public ColumnMemory(int index, string? name)
    {
        Index = index;
        Name = name;
    }

    public int Index { get; }
    public string? Name { get; }
}

public class RowMemory : IRow
{
    private readonly IReadOnlyList<object?> data;
    public RowMemory(int index, IReadOnlyList<object?> data)
    {
        Index = index;
        this.data = data;
    }
    public IEnumerator<object?> GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => data.Count;
    public object? this[int index] => data[index];

    public int Index { get; }
}
