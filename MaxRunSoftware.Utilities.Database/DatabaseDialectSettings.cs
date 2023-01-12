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

    public char? DialectEscapeLeft { get; set; }
    public char? DialectEscapeRight { get; set; }

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

        var el = DialectEscapeLeft;
        if (el != null && !objectToEscape.StartsWith(el.Value)) objectToEscape = el.Value + objectToEscape;

        var er = DialectEscapeRight;
        if (er != null && !objectToEscape.EndsWith(er.Value)) objectToEscape += er.Value;

        return objectToEscape;
    }

    public virtual string Unescape(string objectToUnescape)
    {
        var el = DialectEscapeLeft;
        if (el != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.StartsWith(el.Value))
            {
                objectToUnescape = objectToUnescape.RemoveLeft(1);
            }
        }

        var er = DialectEscapeRight;
        if (er != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.EndsWith(er.Value))
            {
                objectToUnescape = objectToUnescape.RemoveRight(1);
            }
        }

        return objectToUnescape;
    }

    public virtual string Escape(DatabaseSchemaDatabase database) => Escape(database.DatabaseName);
    public virtual string Escape(DatabaseSchemaSchema schema) => Escape(schema.Database.DatabaseName, schema.SchemaName);
    public virtual string Escape(DatabaseSchemaTable table) => Escape(table.Schema.Database.DatabaseName, table.Schema.SchemaName, table.TableName);

    public virtual string Escape(params string?[] objectsToEscape) => Escape((IReadOnlyList<string?>)objectsToEscape);

    public virtual string Escape(IReadOnlyList<string?> objectsToEscape)
    {
        var sb = new StringBuilder();
        foreach (var obj in objectsToEscape)
        {
            if (obj == null) continue;
            var objUnescaped = Unescape(obj);
            if (objUnescaped.TrimOrNull() == null) continue;
            var objEscaped = Escape(objUnescaped);
            if (sb.Length > 0) sb.Append('.');
            sb.Append(objEscaped);
        }
        return sb.ToString();
    }

    #endregion Escape

    #region IsExcluded

    public virtual bool IsExcluded(DatabaseSchemaObject obj) => obj switch
    {
        DatabaseSchemaTableColumn tableColumn => IsExcluded(tableColumn),
        DatabaseSchemaTable table => IsExcluded(table),
        DatabaseSchemaSchema schema => IsExcluded(schema),
        DatabaseSchemaDatabase database => IsExcluded(database),
        _ => throw new NotImplementedException()
    };

    public virtual bool IsExcluded(DatabaseSchemaDatabase database) => ExcludedDatabases.Contains(database);
    public virtual bool IsExcluded(DatabaseSchemaSchema schema) => IsExcluded(schema.Database) || ExcludedSchemas.Contains(schema) || (schema.SchemaName != null && ExcludedSchemasGlobal.Contains(schema.SchemaName));
    public virtual bool IsExcluded(DatabaseSchemaTable table) => IsExcluded(table.Schema) || ExcludedTables.Contains(table) || ExcludedTablesGlobal.Contains(table.TableName);
    public virtual bool IsExcluded(DatabaseSchemaTableColumn column) => IsExcluded(column.Table);

    #endregion IsExcluded





}
