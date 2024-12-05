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

public partial class DataReaderResultRowCollection : IReadOnlyList<DataReaderResultRow>
{
    public DataReaderResult Result { get; }

    private readonly List<DataReaderResultRow> rows;

    public DataReaderResultRow this[int index] => rows[index];

    public DataReaderResultRowCollection(IDataReader reader, DataReaderResult result)
    {
        Result = result;
        var nullableColumnIndexes = Result.Columns.nullableColumns;
        rows = reader.GetValuesAll().Select((valueRow, i) => new DataReaderResultRow(i, valueRow, nullableColumnIndexes)).ToList();
    }

    public IEnumerator<DataReaderResultRow> GetEnumerator() => rows.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => rows.Count;
}

public partial class DataReaderResultRowCollection : IReadOnlyList<ITableRow>
{
    IEnumerator<ITableRow> IEnumerable<ITableRow>.GetEnumerator() => GetEnumerator();
    ITableRow IReadOnlyList<ITableRow>.this[int index] => this[index];

}
