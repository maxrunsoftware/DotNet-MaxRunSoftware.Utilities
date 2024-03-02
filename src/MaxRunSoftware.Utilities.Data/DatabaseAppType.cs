// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Data;

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming
/// <summary>
/// https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-strings
/// </summary>
public enum DatabaseAppType
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

public static class DatabaseAppTypeExtensions
{
    public static ImmutableDictionary<DatabaseAppType, Func<string, IDbConnection>> ConnectionFactories { get; } = new Dictionary<DatabaseAppType, Func<string, IDbConnection>>
    {
        [DatabaseAppType.MicrosoftSql] = MicrosoftSql.CreateConnection,
        [DatabaseAppType.OracleSql] = OracleSql.CreateConnection,
        [DatabaseAppType.MySql] = MySql.CreateConnection,
        [DatabaseAppType.PostgreSql] = PostgreSql.CreateConnection,
    }.ToImmutableDictionary();

    public static ImmutableDictionary<DatabaseAppType, Func<IDbConnection, ILoggerProvider, Sql>> SqlFactories { get; } = new Dictionary<DatabaseAppType, Func<IDbConnection, ILoggerProvider, Sql>>
    {
        [DatabaseAppType.MicrosoftSql] = (connection, loggerProvider) => new MicrosoftSql(connection, loggerProvider),
        [DatabaseAppType.OracleSql] = (connection, loggerProvider) => new OracleSql(connection, loggerProvider),
        [DatabaseAppType.MySql] = (connection, loggerProvider) => new MySql(connection, loggerProvider),
        [DatabaseAppType.PostgreSql] = (connection, loggerProvider) => new PostgreSql(connection, loggerProvider),
    }.ToImmutableDictionary();

    public static IDbConnection CreateConnection(this DatabaseAppType connectionType, string connectionString) =>
        ConnectionFactories[connectionType](connectionString);

    public static Sql CreateSql(this DatabaseAppType connectionType, string connectionString, ILoggerProvider loggerProvider) =>
        SqlFactories[connectionType](CreateConnection(connectionType, connectionString), loggerProvider);
}
