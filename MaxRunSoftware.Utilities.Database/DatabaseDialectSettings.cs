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

public class DatabaseDialectSettings : ICloneable
{
    public string DefaultDataTypeString { get; set; } = "TEXT";
    public string DefaultDataTypeInteger { get; set; } = "INT";
    public string DefaultDataTypeDateTime { get; set; } = "DATETIME";

    public char? EscapeLeft { get; set; }
    public char? EscapeRight { get; set; }

    public ISet<string> ExcludedDatabases { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddDatabaseUserExcluded(params string[] values) => Add(ExcludedDatabases, values);

    public ISet<string> ExcludedSchemas { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddSchemaUserExcluded(params string[] values) => Add(ExcludedSchemas, values);

    public ISet<char> IdentifierCharactersValid { get; } = new HashSet<char>(Constant.CharComparer_OrdinalIgnoreCase);
    public DatabaseDialectSettings AddIdentifierCharactersValid(params char[] values) => Add(IdentifierCharactersValid, values);

    public ISet<string> ReservedWords { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public DatabaseDialectSettings AddReservedWords(params string[] values) => Add(ReservedWords, values.SelectMany(ReservedWordsParse).ToArray());

    protected static HashSet<string> ReservedWordsParse(string words) => words.SplitOnWhiteSpace().TrimOrNull().WhereNotNull().ToHashSet(StringComparer.OrdinalIgnoreCase);

    private DatabaseDialectSettings Add<T>(ICollection<T> collection, params T[] values)
    {
        foreach(var value in values) collection.Add(value);
        return this;
    }



    public DatabaseDialectSettings Copy() => Utils.CopyShallow(this);
    public object Clone() => Copy();
}


public static class SqlDialectSettingsExtensions
{
    #region Escape / Format

    public static bool NeedsEscaping(this DatabaseDialectSettings settings, string objectThatMightNeedEscaping)
    {
        if (settings.ReservedWords.Contains(objectThatMightNeedEscaping)) return true;

        if (!objectThatMightNeedEscaping.ContainsOnly(settings.IdentifierCharactersValid)) return true;

        return false;
    }

    public static string Escape(this DatabaseDialectSettings settings, string objectToEscape)
    {
        if (!settings.NeedsEscaping(objectToEscape)) return objectToEscape;

        var el = settings.EscapeLeft;
        if (el != null && !objectToEscape.StartsWith(el.Value)) objectToEscape = el.Value + objectToEscape;

        var er = settings.EscapeRight;
        if (er != null && !objectToEscape.EndsWith(er.Value)) objectToEscape += er.Value;

        return objectToEscape;
    }

    public static string Unescape(this DatabaseDialectSettings settings, string objectToUnescape)
    {
        var el = settings.EscapeLeft;
        if (el != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.StartsWith(el.Value))
            {
                objectToUnescape = objectToUnescape.RemoveLeft(1);
            }
        }

        var er = settings.EscapeRight;
        if (er != null)
        {
            while (!string.IsNullOrEmpty(objectToUnescape) && objectToUnescape.EndsWith(er.Value))
            {
                objectToUnescape = objectToUnescape.RemoveRight(1);
            }
        }

        return objectToUnescape;
    }

    #endregion Escape / Format

    public static  bool IsExcludedDatabase(this DatabaseDialectSettings settings, string databaseName) => settings.ExcludedDatabases.Contains(databaseName);
    public static  bool IsExcludedSchema(this DatabaseDialectSettings settings, string schemaName) => settings.ExcludedSchemas.Contains(schemaName);

    public static bool IsExcluded(this DatabaseDialectSettings settings, DatabaseSchemaDatabase database) => settings.IsExcludedDatabase(database.DatabaseName);
    public static  bool IsExcluded(this DatabaseDialectSettings settings, DatabaseSchemaSchema schema) => settings.IsExcluded(schema.Database) || settings.IsExcludedSchema(schema.SchemaName);
    public static bool IsExcluded(this DatabaseDialectSettings settings, DatabaseSchemaTable table) => settings.IsExcluded(table.Schema);
    public static  bool IsExcluded(this DatabaseDialectSettings settings, DatabaseSchemaTableColumn column) => settings.IsExcluded(column.Table);

}
