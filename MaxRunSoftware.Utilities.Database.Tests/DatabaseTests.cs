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

using MaxRunSoftware.Utilities.Common;

namespace MaxRunSoftware.Utilities.Database.Tests;


/// <summary>
/// https://xunit.net/docs/shared-context
/// </summary>
public abstract class DatabaseFixture : IDisposable
{
    protected DatabaseFixture()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Setup();
    }

    protected abstract void Setup();
    protected abstract void Teardown();

    public void Dispose()
    {
        Teardown();
    }

    protected int ExecuteNonQuery(DatabaseAppType appType, string connectionString, string sql, int timeoutSeconds = 60 * 5)
    {
        using var c = appType.OpenConnection(connectionString);
        using var cmd = c.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = timeoutSeconds;
        cmd.CommandText = sql;
        return cmd.ExecuteNonQuery();
    }
}

public abstract class DatabaseFixtureCollection<T> : ICollectionFixture<T> where T : DatabaseFixture
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

public abstract class DatabaseTests<T> : TestBase, IDisposable where T : Sql
{
    protected readonly T sql;
    protected DatabaseTests(ITestOutputHelper testOutputHelper, DatabaseAppType databaseAppType, string connectionString) : base(testOutputHelper)
    {
        sql = (T)databaseAppType.OpenSql(connectionString);
    }

    [Fact]
    public void GetCurrentDatabase()
    {
        var o = sql.CurrentDatabaseName;
        WriteLine(nameof(sql.CurrentDatabaseName), o);
    }

    [Fact]
    public void GetCurrentSchema()
    {
        var o = sql.CurrentSchemaName;
        WriteLine(nameof(sql.CurrentSchemaName), o);
    }

    [Fact]
    public void GetDatabases()
    {
        var o = sql.GetDatabases().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(nameof(sql.GetDatabases), o);
    }

    [Fact]
    public void GetSchemas()
    {
        var o = sql.GetSchemas().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(nameof(sql.GetSchemas), o);
    }

    [Fact]
    public void GetTables()
    {
        var o = sql.GetTables().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(nameof(sql.GetTables), o);
    }

    [Fact]
    public void GetTableColumns()
    {
        var o = sql.GetTableColumns().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(nameof(sql.GetTableColumns), o);
    }

    [Fact]
    public void GetTableExists()
    {
        var tables = sql.GetTables().OrderBy(o => o).ToArray();

        Assert.NotNull(tables);
        Assert.NotEmpty(tables);
        var skipAmount = Math.Max(1, tables.Length / 100);
        for (var i = 0; i < tables.Length; i+= skipAmount)
        {
            var table = tables[i];
            var result = sql.GetTableExists(table);
            WriteLine(nameof(sql.GetTableExists), "[" + Util.FormatRunningCount(i, tables.Length) + $"] {table} -> {result}");
            Assert.True(result);
        }
    }

    protected void WriteLine(string methodName, object? outData)
    {
        output.WriteLine($"[{sql.DatabaseAppType}] {methodName}: {ToStringParse(outData)}");
    }

    protected void WriteLine<TItem>(string methodName, TItem?[] outData)
    {
        for (var i = 0; i < outData.Length; i++)
        {
            WriteLine(methodName, "[" + Util.FormatRunningCount(i, outData.Length) + "] " + ToStringParse(outData[i]));
        }
    }

    protected static string? ToStringParse(object? o)
    {
        if (o is DatabaseSchemaObject databaseSchemaObject) return databaseSchemaObject.ToString();
        return o.ToStringGuessFormat();
    }

    public void Dispose() => sql.Dispose();
}
