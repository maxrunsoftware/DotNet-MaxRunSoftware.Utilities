namespace MaxRunSoftware.Utilities.Common;

public static class DataReaderSchemaExtensions
{
    public static List<DataReaderSchemaColumn> GetSchema(this IDataReader reader) => DataReaderSchemaColumn.Create(reader);
    public static List<DataReaderSchemaColumnExtended> GetSchemaExtended(this IDataReader reader) => DataReaderSchemaColumnExtended.Create(reader);
}
