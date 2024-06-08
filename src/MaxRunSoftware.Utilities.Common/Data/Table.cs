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

namespace MaxRunSoftware.Utilities.Common;

public interface ITable
{
    public IReadOnlyList<ITableColumn>? Columns { get; }
    public IReadOnlyList<ITableRow> Rows { get; }
}

public class Table(IReadOnlyList<ITableColumn>? columns, IReadOnlyList<ITableRow> rows) : ITable
{
    public IReadOnlyList<ITableColumn>? Columns { get; } = columns;
    public IReadOnlyList<ITableRow> Rows { get; } = rows;
}

public interface ITableColumn
{
    public int Index { get; }
    public string? Name { get; }
}

public class TableColumn(int index, string? name) : ITableColumn
{
    public int Index { get; } = index;
    public string? Name { get; } = name;
}

public interface ITableRow : IReadOnlyList<object?>
{
    public int Index { get; }
}

public class TableRow(int index, IReadOnlyList<object?> data) : ITableRow
{
    public IEnumerator<object?> GetEnumerator() => data.GetEnumerator();
    public int Count => data.Count;
    public object? this[int index] => data[index];
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    public int Index { get; } = index;
}
