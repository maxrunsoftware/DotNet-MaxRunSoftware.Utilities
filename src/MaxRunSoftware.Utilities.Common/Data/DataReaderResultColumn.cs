namespace MaxRunSoftware.Utilities.Common;

public class DataReaderResultColumn(DataReaderSchemaColumn schemaColumn, DataReaderResultColumnCollection columnCollection) : ITableColumn
{
    private DataReaderSchemaColumn SchemaColumn { get; } = schemaColumn.CheckNotNull(nameof(schemaColumn));

    public DataReaderResultColumnCollection ColumnCollection { get; } = columnCollection;

    public int Index => SchemaColumn.Index;
    public string Name => SchemaColumn.ColumnName;
    public Type Type => SchemaColumn.FieldType;
    public string DataTypeName => SchemaColumn.DataTypeName;

    public bool IsNullable => ColumnCollection.nullableColumns[Index];
}
