namespace MaxRunSoftware.Utilities.Common;

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
