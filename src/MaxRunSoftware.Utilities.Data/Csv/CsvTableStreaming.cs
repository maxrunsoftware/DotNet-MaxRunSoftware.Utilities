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

using CsvHelper;
using CsvHelper.Configuration;

namespace MaxRunSoftware.Utilities.Data;

public class CsvTableStreaming : ITable
{
    private readonly Func<StreamReader> func;
    private readonly CsvConfiguration config;
    private bool isColumnsLoaded;
    private IReadOnlyList<ITableColumn>? columns;

    public CsvTableStreaming(Func<StreamReader> func, CsvConfiguration config)
    {
        this.func = func;
        this.config = config;
        Rows = new RowCollection(this);
    }

    private StreamReader CreateStreamReader() => func();

    public IReadOnlyList<ITableColumn>? Columns
    {
        get
        {
            if (isColumnsLoaded) return columns;
            if (config.HasHeaderRecord)
            {
                using var r = CreateStreamReader();
                using var csv = new CsvReader(r, config);
                if (csv.Read()) columns = csv.ReadColumns();
            }

            isColumnsLoaded = true;
            return columns;
        }
    }

    public IReadOnlyList<ITableRow> Rows { get; }

    private class RowCollection : IReadOnlyList<ITableRow>
    {
        private class RowCollectionEnumerator : IEnumerator<ITableRow>
        {
            private readonly CsvTableStreaming table;
            private int index;

            private StreamReader? reader;
            private CsvReader? csv;

            public RowCollectionEnumerator(CsvTableStreaming table)
            {
                this.table = table;
                index = -1;
                Current = default!;
            }

            public bool MoveNext()
            {
                reader ??= table.CreateStreamReader();
                csv ??= new(reader, table.config);

                index++;
                if (!csv.Read())
                {
                    DisposeStreams();
                    return false;
                }

                if (index == 0 && table.config.HasHeaderRecord)
                {
                    if (!table.isColumnsLoaded)
                    {
                        table.columns = csv.ReadColumns();
                        table.isColumnsLoaded = true;
                    }

                    if (!csv.Read())
                    {
                        DisposeStreams();
                        return false;
                    }
                }

                Current = csv.ReadRow(index);
                return true;
            }
            public void Reset() => DisposeStreams();
            public ITableRow Current { get; private set; }

            object IEnumerator.Current => Current;

            private void DisposeStreams()
            {
                index = -1;
                Current = default!;
                var c = csv;
                csv = null;
                c?.Dispose();

                var r = reader;
                reader = null;
                r?.Dispose();
            }
            public void Dispose() => DisposeStreams();
        }

        private readonly CsvTableStreaming table;
        public RowCollection(CsvTableStreaming table) => this.table = table;
        public IEnumerator<ITableRow> GetEnumerator() => new RowCollectionEnumerator(table);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private int? count;

        public int Count
        {
            get
            {
                if (count == null)
                {
                    var c = 0;
                    using var e = GetEnumerator();
                    checked
                    {
                        while (e.MoveNext())
                        {
                            c++;
                        }
                    }

                    count = c;
                }

                return count.Value;
            }
        }

        public ITableRow this[int index]
        {
            get
            {
                index.CheckMin(0);
                if (count != null) index.CheckMax(count.Value - 1);
                var c = -1;
                foreach (var row in this)
                {
                    c++;
                    if (c == index) return row;
                }

                index.CheckMax(c); // should throw exception

                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is greater then max index {c}");
            }
        }
    }
}
