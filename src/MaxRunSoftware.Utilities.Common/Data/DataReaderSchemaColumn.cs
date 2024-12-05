namespace MaxRunSoftware.Utilities.Common;

public class DataReaderSchemaColumn(int index, string columnName, Type fieldType, string dataTypeName)
{
    public int Index { get; } = index;
    public string ColumnName { get; } = columnName;
    public Type FieldType { get; } = fieldType;
    public string DataTypeName { get; } = dataTypeName;

    public DataReaderSchemaColumn(IDataReader reader, int columnIndex) : this(
        columnIndex, 
        reader.GetName(columnIndex), 
        reader.GetFieldType(columnIndex), 
        reader.GetDataTypeName(columnIndex)
        ) { }

    public static List<DataReaderSchemaColumn> Create(IDataReader reader) =>
        Enumerable.Range(0, reader.FieldCount).Select(i => new DataReaderSchemaColumn(reader, i)).ToList();
}
