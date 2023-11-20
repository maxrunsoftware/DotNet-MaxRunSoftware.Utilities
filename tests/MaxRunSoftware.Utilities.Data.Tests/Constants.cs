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

namespace MaxRunSoftware.Utilities.Data.Tests;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

public static class Constants
{
    public static readonly ImmutableArray<SkippedTest> IGNORED_TESTS = new []
    {
        SkippedTest.Create<OracleSqlTests>(nameof(OracleSqlTests.GetTableColumns), "takes too long"),
        SkippedTest.Create<OracleSqlTests>(nameof(OracleSqlTests.GetTableExists), "takes too long"),
    }.ToImmutableArray();


    private static readonly string DEFAULT_SERVER = "172.16.46.16";
    private static readonly string DEFAULT_PASSWORD = "testPass1!";

    public static readonly string MicrosoftSql_TestDatabase = "MRSTEST";
    public static readonly string MicrosoftSql_ConnectionString_Master = $"Server={DEFAULT_SERVER};Database=master;User Id=sa;Password={DEFAULT_PASSWORD};";
    public static readonly string MicrosoftSql_ConnectionString_Test = $"Server={DEFAULT_SERVER};Database={MicrosoftSql_TestDatabase};User Id=sa;Password={DEFAULT_PASSWORD};";

    public static readonly string MySql_ConnectionString_Test = $"Server={DEFAULT_SERVER};Port=3306;User Id=root;Password={DEFAULT_PASSWORD};";

    public static readonly string PostgreSql_ConnectionString_Test = $"Host={DEFAULT_SERVER};Username=postgres;Password={DEFAULT_PASSWORD};";

    public static readonly string OracleSql_ConnectionString_Test = $"Data Source={DEFAULT_SERVER}:1521/XE;User Id=SYSTEM;Password={DEFAULT_PASSWORD};";
}
