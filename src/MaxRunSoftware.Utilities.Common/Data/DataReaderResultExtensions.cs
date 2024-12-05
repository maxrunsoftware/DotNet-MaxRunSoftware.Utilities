namespace MaxRunSoftware.Utilities.Common;

public static class DataReaderResultExtensions
{
    public static IEnumerable<DataReaderResult> ReadResults(this IDataReader reader)
    {
        var i = 0;
        do
        {
            var result = new DataReaderResult(reader, i);
            yield return result;
            i++;
        } while (reader.NextResult());
    }

    public static DataReaderResult? ReadResult(this IDataReader reader) => reader.ReadResults().FirstOrDefault();

    public static IEnumerable<DataReaderResult> ExecuteReaderResults(this IDbCommand command, CommandBehavior? behavior = null)
    {
        using var reader = behavior == null ? command.ExecuteReader() : command.ExecuteReader(behavior.Value);

        foreach (var result in reader.ReadResults())
        {
            yield return result;
        }
    }

    public static DataReaderResult? ExecuteReaderResult(this IDbCommand command, CommandBehavior? behavior = null)
    {
        using var reader = behavior == null ? command.ExecuteReader() : command.ExecuteReader(behavior.Value);

        return reader.ReadResult();
    }

    //public static SqlType GetSqlType(this SqlResultColumn sqlResultColumn, Sql sql) => sql.GetSqlDbType(sqlResultColumn.DataTypeName);
}
