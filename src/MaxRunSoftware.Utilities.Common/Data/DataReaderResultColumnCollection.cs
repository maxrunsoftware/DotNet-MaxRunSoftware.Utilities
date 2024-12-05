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

public partial class DataReaderResultColumnCollection : IReadOnlyList<DataReaderResultColumn>
{
    private readonly List<DataReaderResultColumn> columns;
    private readonly Dictionary<string, List<DataReaderResultColumn>> columnsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<DataReaderResultColumn>> columnsByNameCaseSensitive = new(StringComparer.Ordinal);

    internal readonly bool[] nullableColumns;

    public DataReaderResult Result { get; }
    public IReadOnlyList<string> Names { get; }
    public DataReaderResultColumn this[int index] => columns[index];

    public DataReaderResultColumn this[string name]
    {
        get
        {
            var list = columnsByName[name]; // Should throw exception on missing case-insensitive match
            if (list.Count == 1) return list[0];

            // We have multiple case-insensitive columns with that name so let's try case-sensitive
            if (columnsByNameCaseSensitive.TryGetValue(name, out var listCaseSensitive))
            {
                if (listCaseSensitive.Count == 1) return listCaseSensitive[0];
                if (listCaseSensitive.Count > 1) throw new ArgumentOutOfRangeException(nameof(name), $"Multiple columns ({listCaseSensitive.Count}) with name '{name}'");
            }

            // We have multiple case-insensitive columns with that name but 0 columns matching that name case-sensitive
            throw new ArgumentOutOfRangeException(nameof(name), $"Multiple columns ({list.Count}) with name '{name}'");
        }
    }

    public DataReaderResultColumnCollection(IDataReader reader, DataReaderResult result)
    {
        Result = result;
        columns = reader.GetSchema().Select(o => new DataReaderResultColumn(o, this)).OrderBy(o => o.Index).ToList();
        nullableColumns = new bool[columns.Count];
        Array.Fill(nullableColumns, false); // TODO: Not sure if needed

        foreach (var column in columns)
        {
            columnsByName.AddToList(column.Name, column);
            columnsByNameCaseSensitive.AddToList(column.Name, column);
        }

        Names = columns.Select(o => o.Name).ToList().AsReadOnly();
    }


    public bool Contains(int index) => index >= 0 && index < Count;
    public bool Contains(string name) => columnsByName.ContainsKey(name);
    public bool Contains(DataReaderResultColumn column) => columns.Contains(column);

    public int Count => columns.Count;

    public IEnumerator<DataReaderResultColumn> GetEnumerator() => columns.GetEnumerator();

}

public partial class DataReaderResultColumnCollection : IReadOnlyList<ITableColumn>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    ITableColumn IReadOnlyList<ITableColumn>.this[int index] => this[index];
    IEnumerator<ITableColumn> IEnumerable<ITableColumn>.GetEnumerator() => GetEnumerator();


}
