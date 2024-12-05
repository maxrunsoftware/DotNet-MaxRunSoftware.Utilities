namespace MaxRunSoftware.Utilities.Common;

public class DataReaderResultRow : TableRow
{
    public DataReaderResultRow(int index, object?[] data) : base(index, Parse(data, null)) { }

    internal DataReaderResultRow(int index, object?[] data, bool[] nullableColumnIndexes) : base(index, Parse(data, nullableColumnIndexes)) { }

    private static object?[] Parse(object?[] data, bool[]? nullableColumnIndexes)
    {
        if (nullableColumnIndexes == null)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == DBNull.Value) data[i] = null;
            }
        }
        else
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == null)
                {
                    nullableColumnIndexes[i] = true;
                }
                else if (data[i] == DBNull.Value)
                {
                    data[i] = null;
                    nullableColumnIndexes[i] = true;
                }
            }
        }

        return data;
    }
}
