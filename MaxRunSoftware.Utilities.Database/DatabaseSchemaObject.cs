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

public abstract class DatabaseSchemaObject { }


public class DatabaseSchemaDatabase : DatabaseSchemaObject, IEquatable<DatabaseSchemaDatabase>
{
    private readonly int hashCode;
    public string DatabaseName { get; }

    public DatabaseSchemaDatabase(string databaseName)
    {
        DatabaseName = databaseName;
        hashCode = Util.Hash(DatabaseName.ToUpperInvariant());
    }

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaDatabase);

    public bool Equals(DatabaseSchemaDatabase? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqualCaseInsensitive(DatabaseName, other.DatabaseName)) return false;
        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => DatabaseName;
}

public class DatabaseSchemaSchema : DatabaseSchemaObject, IEquatable<DatabaseSchemaSchema>
{
    private readonly int hashCode;
    public DatabaseSchemaDatabase Database { get; }
    public string SchemaName { get; }

    public DatabaseSchemaSchema(string databaseName, string schemaName) : this(new DatabaseSchemaDatabase(databaseName), schemaName) { }

    public DatabaseSchemaSchema(DatabaseSchemaDatabase database, string schemaName)
    {
        Database = database;
        SchemaName = schemaName;

        hashCode = Util.Hash(Database, SchemaName.ToUpperInvariant());
    }

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaSchema);

    public bool Equals(DatabaseSchemaSchema? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Database, other.Database)) return false;
        if (!Util.IsEqualCaseInsensitive(SchemaName, other.SchemaName)) return false;
        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => SchemaName;
}

public class DatabaseSchemaTable : DatabaseSchemaObject, IEquatable<DatabaseSchemaTable>
{
    private readonly int hashCode;
    public DatabaseSchemaSchema Schema { get; }
    public string TableName { get; }

    public DatabaseSchemaTable(string databaseName, string schemaName, string tableName) : this(new DatabaseSchemaSchema(databaseName, schemaName), tableName) { }

    public DatabaseSchemaTable(DatabaseSchemaSchema schema, string tableName)
    {
        Schema = schema;
        TableName = tableName;

        hashCode = Util.Hash(Schema, TableName.ToUpperInvariant());
    }

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTable);

    public bool Equals(DatabaseSchemaTable? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Schema, other.Schema)) return false;
        if (!Util.IsEqualCaseInsensitive(TableName, other.TableName)) return false;
        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => TableName;
}

public class DatabaseSchemaTableColumn : DatabaseSchemaObject, IEquatable<DatabaseSchemaTableColumn>
{
    private readonly int hashCode;
    public DatabaseSchemaTable Table { get; }

    public string ColumnName { get; }
    public string ColumnType { get; }

    public DbType ColumnDbType { get; }
    public bool IsNullable { get; }
    public int Ordinal { get; }
    public long? CharacterLengthMax { get; }
    public int? NumericPrecision { get; }
    public int? NumericScale { get; }
    public string? ColumnDefault { get; }

    public DatabaseSchemaTableColumn(
        DatabaseSchemaTable table,
        string columnName,
        string columnType,
        DbType columnDbType,
        bool isNullable,
        int ordinal,
        long? characterLengthMax = null,
        int? numericPrecision = null,
        int? numericScale = null,
        string? columnDefault = null
    )
    {
        Table = table;
        ColumnName = columnName.CheckNotNullTrimmed();
        ColumnType = columnType.CheckNotNullTrimmed();

        ColumnDbType = columnDbType;
        IsNullable = isNullable;
        Ordinal = ordinal;
        CharacterLengthMax = characterLengthMax;
        NumericPrecision = numericPrecision;
        NumericScale = numericScale;
        ColumnDefault = columnDefault;

        hashCode = Util.Hash(Table, ColumnName.ToUpperInvariant(), Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTableColumn);

    public bool Equals(DatabaseSchemaTableColumn? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Table, other.Table)) return false;
        if (!Util.IsEqualCaseInsensitive(ColumnName, other.ColumnName)) return false;
        if (Ordinal != other.Ordinal) return false;

        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => ColumnName;
}
