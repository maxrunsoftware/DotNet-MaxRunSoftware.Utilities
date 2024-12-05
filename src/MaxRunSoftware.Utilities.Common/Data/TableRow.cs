namespace MaxRunSoftware.Utilities.Common;

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
