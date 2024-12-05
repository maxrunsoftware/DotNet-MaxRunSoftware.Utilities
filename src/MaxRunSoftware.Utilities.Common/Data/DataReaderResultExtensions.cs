// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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
