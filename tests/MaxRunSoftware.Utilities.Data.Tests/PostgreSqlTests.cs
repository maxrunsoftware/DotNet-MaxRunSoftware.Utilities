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

public class PostgreSqlFixture : DatabaseFixture
{
    protected override void Setup() { }
    protected override void Teardown() { }
}

[CollectionDefinition(nameof(PostgreSql))]
public class PostgreSqlFixtureCollection : DatabaseFixtureCollection<PostgreSqlFixture> { }

[Collection(nameof(PostgreSql))]
public class PostgreSqlTests(ITestOutputHelper testOutputHelper) : DatabaseTests<PostgreSql>(testOutputHelper, DatabaseAppType.PostgreSql, Constants.PostgreSql_ConnectionString_Test)
{
    [SkippableFact]
    public void GetServerProperties()
    {
        //var props = ((MicrosoftSql)sql).GetServerProperties();
        //Assert.NotNull(props);
        //WriteLine(nameof(GetServerProperties), Constant.NewLine + props.ToStringDetailed());
    }
}
