// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

using System.Globalization;
using System.Text.RegularExpressions;
using MaxRunSoftware.Utilities.Common;

namespace MaxRunSoftware.Utilities.Database.Tests;

/// <summary>
/// https://xunit.net/docs/shared-context
/// </summary>
public class MicrosoftSqlFixture : IDisposable
{
    public MicrosoftSqlFixture()
    {
        ExecuteNonQueryMicrosoftSql(SqlDatabaseDrop);
        ExecuteNonQueryMicrosoftSql(SqlDatabaseCreate);
    }

    public void Dispose()
    {
        //ExecuteNonQueryMicrosoftSql(SqlDatabaseDrop);
    }

    private string SqlDatabaseDrop => @"
        IF EXISTS (SELECT * FROM master.sys.databases WHERE NAME=N'Northwind')
        BEGIN
            ALTER DATABASE Northwind SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE IF EXISTS Northwind;
        END
        ";

    private string SqlDatabaseCreate => GetResource("MicrosoftSql.Northwind.sql");

    public int ExecuteNonQueryMicrosoftSql(string sql)
    {
        // https://stackoverflow.com/a/52443620

        var batchTerminator = "GO";

        // Handle backslash utility statement (see http://technet.microsoft.com/en-us/library/dd207007.aspx)
        sql = Regex.Replace(sql, @"\\(\r\n|\r|\n)", string.Empty);

        // Handle batch splitting utility statement (see http://technet.microsoft.com/en-us/library/ms188037.aspx)
        var batches = Regex.Split(
            sql,
            string.Format(CultureInfo.InvariantCulture, @"^\s*({0}[ \t]+[0-9]+|{0})(?:\s+|$)", batchTerminator),
            RegexOptions.IgnoreCase | RegexOptions.Multiline
        );

        var resultCount = 0;
        for (var i = 0; i < batches.Length; ++i)
        {
            // Skip batches that merely contain the batch terminator
            if (batches[i].StartsWith(batchTerminator, StringComparison.OrdinalIgnoreCase)) continue;
            if (i == (batches.Length - 1) && string.IsNullOrWhiteSpace(batches[i])) continue;

            // Include batch terminator if the next element is a batch terminator
            if (batches.Length > (i + 1) && batches[i + 1].StartsWith(batchTerminator, StringComparison.OrdinalIgnoreCase))
            {
                var repeatCount = 1;

                // Handle count parameter on the batch splitting utility statement
                if (!string.Equals(batches[i + 1], batchTerminator, StringComparison.OrdinalIgnoreCase))
                {
                    repeatCount = int.Parse(Regex.Match(batches[i + 1], @"([0-9]+)").Value, CultureInfo.InvariantCulture);
                }

                for (var j = 0; j < repeatCount; ++j)
                {
                    resultCount += ExecuteNonQuery(batches[i]);
                }
            }
            else
            {
                resultCount += ExecuteNonQuery(batches[i]);
            }
        }

        return resultCount;
    }

    private int ExecuteNonQuery(string sql)
    {
        using var c = OpenConnection();
        using var cmd = c.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 60 * 1;
        cmd.CommandText = sql;
        return cmd.ExecuteNonQuery();
    }

    private IDbConnection OpenConnection() => DatabaseAppType.MicrosoftSql.OpenConnection(Constants.ConnectionString_MicrosoftSql);

    private string GetResource(string filename)
    {
        var assembly = GetType().Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames.Single(o => o.EndsWith(filename, StringComparison.OrdinalIgnoreCase));
        if (resourceName == null) throw new Exception($"Could not find resource file: {filename}");
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new Exception($"Could not open stream for resource: {resourceName}  {filename}");
        using var reader = new StreamReader(stream, Constant.Encoding_UTF8);
        return reader.ReadToEnd();
    }
}

[CollectionDefinition(nameof(MicrosoftSql))]
public class MicrosoftSqlFixtureCollection : ICollectionFixture<MicrosoftSqlFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

[Collection(nameof(MicrosoftSql))]
public class MicrosoftSqlTests : DatabaseTests
{
    public MicrosoftSqlTests(ITestOutputHelper testOutputHelper) :
        base(testOutputHelper, new MicrosoftSql(Constants.ConnectionString_MicrosoftSql))
    { }
}
