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

namespace MaxRunSoftware.Utilities.Database.Tests;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

public static class Constants
{
    public static readonly string MicrosoftSql_TestDatabase = "MRSTEST";
    public static readonly string MicrosoftSql_ConnectionString_Master = "Server=172.16.46.16;Database=master;User Id=sa;Password=testPass1!;";
    public static readonly string MicrosoftSql_ConnectionString_Test = $"Server=172.16.46.16;Database={MicrosoftSql_TestDatabase};User Id=sa;Password=testPass1!;";

    //public static readonly string MySql_ConnectionString_Test = $"Server=172.16.46.16;Database=mysql;Uid=root;Pwd=testPass1!;";
    public static readonly string MySql_ConnectionString_Test = "Server=172.16.46.16;Port=3306;User Id=root;Password=testPass1!;";
    //public static readonly string MySql_ConnectionString_Test = "Server=172.16.46.16;Port=3306;Database=testdb;Uid=root;Pwd=testPass1!;";
    //Server=myServerAddress;Port=1234;Database=myDataBase;Uid=myUsername;Pwd=myPassword;

}
