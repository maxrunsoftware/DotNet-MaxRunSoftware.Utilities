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

using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace MaxRunSoftware.Utilities.Database;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

/// <summary>
/// https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-strings
/// </summary>
public enum SqlConnectionType
{
    /// <summary>
    /// Server=172.16.46.3;Database=NorthWind;User Id=testuser;Password=testpass;TrustServerCertificate=True;
    /// https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqlconnection.connectionstring?view=dotnet-plat-ext-7.0
    /// </summary>
    MicrosoftSql,

    /// <summary>
    /// Data Source=172.16.46.9:1521/orcl;User Id=testuser;Password=testpass;
    /// https://docs.oracle.com/database/121/ODPNT/featConnecting.htm#ODPNT164
    /// </summary>
    OracleSql,

    /// <summary>
    /// Server=172.16.46.3;Database=NorthWind;User Id=testuser;Password=testpass;
    /// https://dev.mysql.com/doc/connector-net/en/connector-net-8-0-connection-options.html#connector-net-8-0-connection-options-classic-xprotocol
    /// </summary>
    MySql,

    /// <summary>
    /// Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase;
    /// https://www.npgsql.org/doc/connection-string-parameters.html
    /// </summary>
    PostgreSql,
}

public static class SqlConnectionTypeExtensions
{
    public static IDbConnection CreateConnection(this SqlConnectionType connectionType, string connectionString) => connectionType switch
    {
        SqlConnectionType.MicrosoftSql => new SqlConnection(connectionString),
        SqlConnectionType.OracleSql => new OracleConnection(connectionString),
        SqlConnectionType.MySql => new MySqlConnection(connectionString),
        SqlConnectionType.PostgreSql => new NpgsqlConnection(connectionString),
        _ => throw new NotImplementedException(connectionType.ToString())
    };

    public static IDbConnection OpenConnection(this SqlConnectionType connectionType, string connectionString)
    {
        var connection = CreateConnection(connectionType, connectionString);
        connection.Open();
        return connection;
    }
}
