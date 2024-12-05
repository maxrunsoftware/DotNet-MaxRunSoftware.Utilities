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
