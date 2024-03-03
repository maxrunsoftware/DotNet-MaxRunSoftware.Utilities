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

namespace MaxRunSoftware.Utilities.Data.Tests;

#nullable enable

/// <summary>
/// https://xunit.net/docs/shared-context
/// </summary>
public abstract class DatabaseFixture : IDisposable
{
    protected DatabaseFixture() =>
        // ReSharper disable once VirtualMemberCallInConstructor
        Setup();

    protected abstract void Setup();
    protected abstract void Teardown();

    public void Dispose() => Teardown();

    protected int ExecuteNonQuery(DatabaseAppType appType, string connectionString, string sql, int timeoutSeconds = 60 * 5)
    {
        using var c = appType.CreateConnection(connectionString);
        c.Open();
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

public abstract class DatabaseTests<T> : TestBase where T : Sql
{
    protected readonly T sql;
    protected DatabaseAppType DatabaseAppType { get; }
    protected DatabaseTests(ITestOutputHelper testOutputHelper, DatabaseAppType databaseAppType, string connectionString) : base(testOutputHelper)
    {
        sql = (T)databaseAppType.CreateSql(connectionString, LoggerProvider);
        DatabaseAppType = sql.DatabaseAppType;
    }

    [SkippableFact]
    public void GetCurrentDatabase()
    {
        var o = sql.CurrentDatabaseName;
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetCurrentSchema()
    {
        var o = sql.CurrentSchemaName;
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetDatabases()
    {
        var o = sql.GetDatabases().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetSchemas()
    {
        var o = sql.GetSchemas().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetTables()
    {
        var o = sql.GetTables().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetTableColumns()
    {
        var o = sql.GetTableColumns().OrderBy(o => o).ToArray();
        Assert.NotNull(o);
        Assert.NotEmpty(o);
        WriteLine(new(), o);
    }

    [SkippableFact]
    public void GetTableExists()
    {
        var tables = sql.GetTables().OrderBy(o => o).ToArray();

        Assert.NotNull(tables);
        Assert.NotEmpty(tables);
        var skipAmount = Math.Max(1, tables.Length / 100);
        for (var i = 0; i < tables.Length; i += skipAmount)
        {
            var table = tables[i];
            var result = sql.GetTableExists(table);
            WriteLine(new(), "[" + Util.FormatRunningCount(i, tables.Length) + $"] {table} -> {result}");
            Assert.True(result);
        }
    }

    protected void WriteLine(CallerInfoMethod info, object? outData) => log.LogInformationMethod(info, $"[{DatabaseAppType}] {ToStringParse(outData)}");

    protected void WriteLine<TItem>(CallerInfoMethod info, TItem?[] outData)
    {
        for (var i = 0; i < outData.Length; i++)
        {
            WriteLine(info, "[" + Util.FormatRunningCount(i, outData.Length) + "] " + ToStringParse(outData[i]));
        }
    }

    protected static string? ToStringParse(object? o)
    {
        if (o is DatabaseSchemaObject databaseSchemaObject) return databaseSchemaObject.ToString();
        return o.ToStringGuessFormat();
    }

    public override void Dispose()
    {
        sql.Dispose();
        base.Dispose();
    }
}
