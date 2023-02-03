using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;

namespace MaxRunSoftware.Utilities.Data;

public class TableCsvStreaming : ITable
{
    private readonly Func<StreamReader> func;
    private readonly CsvConfiguration config;
    private bool isColumnsLoaded;
    private IReadOnlyList<IColumn>? columns;

    public TableCsvStreaming(Func<StreamReader> func, CsvConfiguration config)
    {
        this.func = func;
        this.config = config;
    }

    public IReadOnlyList<IColumn>? Columns
    {
        get
        {
            if (isColumnsLoaded) return columns;
            if (config.HasHeaderRecord)
            {
                using var r = func();
                using var csv = new CsvReader(r, config);
                if (csv.Read()) columns = csv.ReadColumns();
            }

            isColumnsLoaded = true;
            return columns;
        }
    }

    private class RowCollection : IReadOnlyList<IRow>
    {
        private class RowCollectionEnumerator : IEnumerator<IRow>
        {
            private readonly TableCsvStreaming table;
            private int index;

            private StreamReader? reader;
            private CsvReader? csv;

            public RowCollectionEnumerator(TableCsvStreaming table)
            {
                this.table = table;
                index = -1;
                Current = default!;
            }

            public bool MoveNext()
            {
                if (reader == null) reader = table.func();
                if (csv == null) csv = new(reader, table.config);
                index++;
                if (!csv.Read())
                {
                    Dispose();
                    return false;
                }

                if (csv.Configuration!.HasHeaderRecord)
                {
                    if (!table.isColumnsLoaded)
                    {
                        table.columns = csv.ReadColumns();
                        table.isColumnsLoaded = true;
                    }
                    if (!csv.Read())
                    {
                        Dispose();
                        return false;
                    }
                }

                //Avoids going beyond the end of the collection.
                if (++curIndex >= _collection.Count)
                {
                    return false;
                }
                else
                {
                    // Set current box to next item in collection.
                    curBox = _collection[curIndex];
                }
                return true;
            }
            public void Reset() => throw new NotImplementedException();
            public IRow Current { get; }

            object IEnumerator.Current => Current;

            public void Dispose() => throw new NotImplementedException();
        }
        private readonly TableCsvStreaming table;
        public RowCollection(TableCsvStreaming table) => this.table = table;
        public IEnumerator<IRow> GetEnumerator()
        {

        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count { get; }
        public IRow this[int index] => throw new NotImplementedException();
    }

    public IReadOnlyList<IRow> Rows { get; }
}
public class Csv : IReadOnlyList<string[]>
{
    private readonly TextReader textReader;
    private readonly CsvReader csvReader;
    private Csv(TextReader textReader, CsvConfiguration config)
    {
        this.textReader = textReader;
        csvReader = new(textReader, config);

    }


    private static CsvConfiguration CreateDefaultConfig() => new(CultureInfo.InvariantCulture)
    {
        BufferSize = (int)Bytes.Mega.Value * 1,
        DetectColumnCountChanges = false,
        IgnoreBlankLines = true,
    };



    public static ITable CreateFromString(string data, CsvConfiguration config)
    {
        IReadOnlyList<IColumn>? columns = null;
        var rows = new List<IRow>();

        using (var r = new StringReader(data))
        {
            using (var csv = new CsvReader(r, config))
            {
                if (!csv.Read()) return new TableMemory(columns, rows);

                if (config.HasHeaderRecord)
                {
                    columns = csv.ReadColumns();
                    if (!csv.Read()) return new TableMemory(columns, rows);
                }

                var rowIndex = 0;
                do
                {
                    var row = csv.ReadRow(rowIndex);
                    rows.Add(row);
                    rowIndex++;
                } while (csv.Read());
                return new TableMemory(columns, rows);
            }
        }


    }

    public static ITable CreateFromFile(string file, Encoding encoding, CsvConfiguration config, long memoryThreshold)
    {
        file.CheckFileExists();

        var fileLength = memoryThreshold == long.MaxValue ? 0 : Util.FileGetLength(file);
        if (fileLength <= memoryThreshold)
        {
            return CreateFromString(Util.FileRead(file, encoding), config);
        }

    }
    public IEnumerator<string[]> GetEnumerator() => throw new NotImplementedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count { get; }
    public string[] this[int index] => throw new NotImplementedException();
}

internal static class CsvHelperExtensions
{
    public static IReadOnlyList<IColumn>? ReadColumns(this CsvReader csv)
    {
        csv.ReadHeader();
        var header = csv.HeaderRecord;
        if (header == null) return null;

        var array = new IColumn[header.Length];

        for (var i = 0; i < array.Length; i++)
        {
            array[i] = new ColumnMemory(i, header[i]);
        }

        return array;
    }

    public static IRow ReadRow(this CsvReader csv, int index)
    {
        var dataArray = new object?[csv.ColumnCount];
        for (var i = 0; i < dataArray.Length; i++)
        {
            dataArray[i] = csv.GetField(i);
        }

        return new RowMemory(index, dataArray);
    }
}
