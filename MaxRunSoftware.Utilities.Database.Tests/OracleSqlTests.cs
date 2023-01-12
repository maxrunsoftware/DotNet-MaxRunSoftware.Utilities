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

using MaxRunSoftware.Utilities.Common;

namespace MaxRunSoftware.Utilities.Database.Tests;

// ReSharper disable StringLiteralTypo

/// <summary>
/// https://xunit.net/docs/shared-context
/// </summary>
public class OracleSqlFixture : DatabaseFixture
{
    protected override void Setup() { }
    protected override void Teardown() { }
}

[CollectionDefinition(nameof(OracleSql))]
public class OracleSqlFixtureCollection : DatabaseFixtureCollection<OracleSqlFixture> { }

[Collection(nameof(Oracle))]
public class OracleSqlTests : DatabaseTests<OracleSql>
{
    public OracleSqlTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, DatabaseAppType.OracleSql, Constants.OracleSql_ConnectionString_Test) { }

    [Fact]
    public void GetServerProperties()
    {
        //var props = ((MicrosoftSql)sql).GetServerProperties();
        //Assert.NotNull(props);
        //WriteLine(nameof(GetServerProperties), Constant.NewLine + props.ToStringDetailed());
    }
}
