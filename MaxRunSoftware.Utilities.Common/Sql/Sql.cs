// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using Microsoft.Extensions.Logging;

namespace MaxRunSoftware.Utilities.Common;

public class SqlExecuteBuilder
{
    public IDbConnection Connection { get; }
    public IDbCommand Command { get; }
    public IDictionary<string, IDataParameter> Parameters { get; }

    public SqlExecuteBuilder(IDbConnection connection, IDbCommand command)
    {
        Connection = connection;
        Command = command;
        Parameters = new DictionaryIndexed<string, IDataParameter>(StringComparer.Ordinal);
    }

    public IDataParameter AddParameter(string name, object? value)
    {
        if (value == DBNull.Value) value = null;
        if (value == null) return AddParameter(name, value, DbType.String);
        if (Constant.Type_DbType.TryGetValue(value.GetType(), out var dbType)) return AddParameter(name, value, dbType);
        return AddParameter(name, value, DbType.String);
    }

    public IDataParameter AddParameter(string name, DbType type)
    {
        if (Parameters.TryGetValue(name, out var parameterExisting)) throw new Exception($"Cannot add parameter '{name}' because it was already added with value '{parameterExisting.Value}'");
        var parameter = Command.AddParameter(parameterName: name, dbType: type);
        Parameters.Add(name, parameter);
        return parameter;
    }

    public IDataParameter AddParameter(string name, object? value, DbType type)
    {
        if (Parameters.TryGetValue(name, out var parameterExisting)) throw new Exception($"Cannot add parameter '{name}' with value '{value}' because it was already added with value '{parameterExisting.Value}'");
        var parameter = Command.AddParameter(parameterName: name, value: value, dbType: type);
        Parameters.Add(name, parameter);
        return parameter;
    }

}

public static class SqlExtensions2
{
    private static IEnumerable<SqlResult> Execute(this IDbCommand command, Action<SqlExecuteBuilder> builder)
    {
        var conn = command.Connection.CheckNotNull();
        var seb = new SqlExecuteBuilder(connection: conn, command: command);
        builder(seb);
        using var reader = command.ExecuteReader();
        return reader.ReadSqlResults();
    }
    public static IEnumerable<SqlResult> Execute(this IDbConnection connection, Action<SqlExecuteBuilder> builder)
    {
        using var command = connection.CreateCommand();
        return Execute(command: command, builder: builder);
    }

    public static IEnumerable<SqlResult> Execute(this IDbConnection connection, string sql, Action<SqlExecuteBuilder> builder)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        return Execute(command: command, builder: builder);
    }
}
