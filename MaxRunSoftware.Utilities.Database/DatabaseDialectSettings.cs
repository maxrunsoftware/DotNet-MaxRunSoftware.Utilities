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

namespace MaxRunSoftware.Utilities.Database;

public class DatabaseDialectSettings : ICloneable
{
    public string DefaultDataTypeString { get; set; } = "TEXT";
    public string DefaultDataTypeInteger { get; set; } = "INT";
    public string DefaultDataTypeDateTime { get; set; } = "DATETIME";

    public char? EscapeLeft { get; set; }
    public char? EscapeRight { get; set; }

    public ISet<DatabaseSchemaDatabase> ExcludedDatabases { get; } = new HashSet<DatabaseSchemaDatabase>();
    public DatabaseDialectSettings AddExcludedDatabase(params string[] values) => Add(ExcludedDatabases, values.Select(o => new DatabaseSchemaDatabase(o)));

    public ISet<DatabaseSchemaSchema> ExcludedSchemas { get; } = new HashSet<DatabaseSchemaSchema>();
    public DatabaseDialectSettings AddExcludedSchema(params (string DatabaseName, string SchemaName)[] values) => Add(ExcludedSchemas, values.Select(o => new DatabaseSchemaSchema(o.DatabaseName, o.SchemaName)));

    public ISet<string> ExcludedSchemasGlobal { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddExcludedSchemaGlobal(params string[] values) => Add(ExcludedSchemasGlobal, values);


    public ISet<DatabaseSchemaTable> ExcludedTables { get; } = new HashSet<DatabaseSchemaTable>();
    public DatabaseDialectSettings AddExcludedSchema(params (string DatabaseName, string SchemaName, string TableName)[] values) => Add(ExcludedTables, values.Select(o => new DatabaseSchemaTable(o.DatabaseName, o.SchemaName, o.TableName)));

    public ISet<string> ExcludedTablesGlobal { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddExcludedTablesGlobal(params string[] values) => Add(ExcludedTablesGlobal, values);


    public ISet<char> IdentifierCharactersValid { get; } = new HashSet<char>(Constant.CharComparer_OrdinalIgnoreCase);
    public DatabaseDialectSettings AddIdentifierCharactersValid(params char[] values) => Add(IdentifierCharactersValid, values);

    public ISet<string> ReservedWords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddReservedWords(params string[] values) => Add(ReservedWords, values.SelectMany(ReservedWordsParse).ToArray());

    public virtual string GenerateParameterName(int parameterIndex) => "@p" + parameterIndex;

    protected static HashSet<string> ReservedWordsParse(string words) => words.SplitOnWhiteSpace().TrimOrNull().WhereNotNull().ToHashSet(StringComparer.OrdinalIgnoreCase);

    private DatabaseDialectSettings Add<T>(ICollection<T> collection, IEnumerable<T> values)
    {
        foreach (var value in values) collection.Add(value);
        return this;
    }

    public virtual DatabaseDialectSettings Copy() => Utils.CopyShallow(this);
    public object Clone() => Copy();

    #region Escape

    public virtual bool NeedsEscaping(string objectThatMightNeedEscaping)
    {
        if (ReservedWords.Contains(objectThatMightNeedEscaping)) return true;

        if (!objectThatMightNeedEscaping.ContainsOnly(IdentifierCharactersValid)) return true;

        return false;
    }

    public virtual string Escape(string objectToEscape)
    {
        if (!NeedsEscaping(objectToEscape)) return objectToEscape;

        var el = EscapeLeft;
        if (el != null && !objectToEscape.StartsWith(el.Value)) objectToEscape = el.Value + objectToEscape;

        var er = EscapeRight;
        if (er != null && !objectToEscape.EndsWith(er.Value)) objectToEscape += er.Value;

        return objectToEscape;
    }

    public virtual string Unescape(string objectToUnescape)
    {
        var el = EscapeLeft;
        if (el != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.StartsWith(el.Value))
            {
                objectToUnescape = objectToUnescape.RemoveLeft(1);
            }
        }

        var er = EscapeRight;
        if (er != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.EndsWith(er.Value))
            {
                objectToUnescape = objectToUnescape.RemoveRight(1);
            }
        }

        return objectToUnescape;
    }

    #endregion Escape

    #region IsExcluded

    public virtual bool IsExcluded(DatabaseSchemaDatabase database) => ExcludedDatabases.Contains(database);
    public virtual bool IsExcluded(DatabaseSchemaSchema schema) => IsExcluded(schema.Database) || ExcludedSchemas.Contains(schema) || ExcludedSchemasGlobal.Contains(schema.Name);
    public virtual bool IsExcluded(DatabaseSchemaTable table) => IsExcluded(table.Schema) || ExcludedTables.Contains(table) || ExcludedTablesGlobal.Contains(table.Name);
    public virtual bool IsExcluded(DatabaseSchemaTableColumn column) => IsExcluded(column.Table);

    #endregion IsExcluded
}

public static class DatabaseDialectSettingsExtensions
{
    public static bool IsExcluded(this DatabaseDialectSettings settings, DatabaseSchemaObject obj) => obj switch
    {
        DatabaseSchemaTableColumn tableColumn => settings.IsExcluded(tableColumn),
        DatabaseSchemaTable table => settings.IsExcluded(table),
        DatabaseSchemaSchema schema => settings.IsExcluded(schema),
        DatabaseSchemaDatabase database => settings.IsExcluded(database),
        _ => throw new NotImplementedException()
    };
}
