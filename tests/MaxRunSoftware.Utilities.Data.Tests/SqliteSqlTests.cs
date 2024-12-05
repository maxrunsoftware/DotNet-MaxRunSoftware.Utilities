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

// ReSharper disable StringLiteralTypo
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable RedundantTypeDeclarationBody

namespace MaxRunSoftware.Utilities.Data.Tests;

public class SqliteSqlFixture : DatabaseFixture
{
    protected override void Setup() { }
    protected override void Teardown() { }
}

[CollectionDefinition(nameof(SqliteSql))]
public class SqliteSqlFixtureCollection : DatabaseFixtureCollection<SqliteSqlFixture> { }

[Collection(nameof(SqliteSql))]
public class SqliteSqlTests : DatabaseTests<SqliteSql>
{
    public SqliteSqlTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, DatabaseAppType.SqliteSql, TestConfig.SqliteSql_ConnectionString_Test)
    {
        using var cmd = sql.CreateCommand();
        cmd.CommandText =
            """
            CREATE TABLE contact (
            	contact_id INTEGER PRIMARY KEY
            ,	first_name TEXT NULL
            ,	last_name TEXT NOT NULL
            ,	email TEXT NOT NULL UNIQUE
            ,	phone TEXT NULL UNIQUE
            ,	height REAL NULL
            ,	birthdate TEXT NULL
            ,	zip INT NULL
            ,	picture BLOB NULL
            ) STRICT;
            """;
        cmd.ExecuteNonQuery();
    }

    [SkippableFact]
    public void GetServerProperties()
    {
        //var props = ((MicrosoftSql)sql).GetServerProperties();
        //Assert.NotNull(props);
        //WriteLine(nameof(GetServerProperties), Constant.NewLine + props.ToStringDetailed());
    }

    [SkippableFact]
    public void Insert_Contact()
    {
        LogLevel = LogLevel.Trace;
        sql.ExceptionShowFullSql = true;
        var table = sql.GetTables().First(o => o.TableName.EqualsOrdinalIgnoreCase("contact"));
        var pictureBytes = new Random().NextBytes(10000);
        sql.Insert(table, [
            ("first_name", "John"), 
            ("last_name", "Doe"), 
            ("email", "jdoe@aol.com"), 
            ("height", "123.456"), 
            ("zip", "12345"),
            ("picture", pictureBytes),
        ]);
        var result = table.GetRows(sql);
        Assert.Single(result.Rows);

        var v = result.Rows[0][GetColumnIndex("first_name", result)];
        Assert.Equal(typeof(string), v.GetType());
        Assert.Equal("John", v);
        
        v = result.Rows[0][GetColumnIndex("last_name", result)];
        Assert.Equal(typeof(string), v.GetType());
        Assert.Equal("Doe", v);
        
        v = result.Rows[0][GetColumnIndex("email", result)];
        Assert.Equal(typeof(string), v.GetType());
        Assert.Equal("jdoe@aol.com", v);
        
        v = result.Rows[0][GetColumnIndex("height", result)];
        Assert.Equal(typeof(double), v.GetType());
        Assert.Equal(double.Parse("123.456"), v);
        
        v = result.Rows[0][GetColumnIndex("zip", result)];
        Assert.Equal(typeof(long), v.GetType());
        Assert.Equal(long.Parse("12345"), v);
        
        v = result.Rows[0][GetColumnIndex("phone", result)];
        Assert.Null(v);
        
        v = result.Rows[0][GetColumnIndex("contact_id", result)];
        Assert.Equal(typeof(long), v.GetType());
        Assert.True(((long)v) >= 0L);

        v = result.Rows[0][GetColumnIndex("picture", result)];
        Assert.Equal(typeof(byte[]), v.GetType());
        Assert.Equal(pictureBytes, v);
        Assert.True(pictureBytes.IsEqual((byte[])v));
    }

    private int GetColumnIndex(string columnName, ITable table) => table.Columns.First(o => o.Name.EqualsOrdinalIgnoreCase(columnName)).Index;
}
