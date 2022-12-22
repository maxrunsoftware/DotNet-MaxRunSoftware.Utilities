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

namespace MaxRunSoftware.Utilities.Database;

public interface ISqlSchemaManager
{
    public string? GetCurrentDatabaseName();
    public string? GetCurrentSchemaName();

    public IEnumerable<SqlSchemaDatabase> GetDatabases();
    public IEnumerable<SqlSchemaSchema> GetSchemas(string? database = null);
    public IEnumerable<SqlSchemaTable> GetTables(string? database = null, string? schema = null);
    public IEnumerable<SqlSchemaTableColumn> GetTableColumns(string? database = null, string? schema = null, string? table = null);

    public bool GetTableExists(string? database, string? schema, string table);
    public bool DropTable(string? database, string? schema, string table);

}

public class SqlSchemaManager
{
    protected ISqlCommandProvider CommandProvider { get; }
    protected SqlDialectSettings DialectSettings { get; }

    public SqlSchemaManager(ISqlCommandProvider commandProvider, SqlDialectSettings dialectSettings)
    {
        CommandProvider = commandProvider;
        DialectSettings = dialectSettings;
    }


}
