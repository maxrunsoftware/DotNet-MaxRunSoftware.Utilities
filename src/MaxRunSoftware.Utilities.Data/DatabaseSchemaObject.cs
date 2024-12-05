// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

// ReSharper disable RedundantTypeDeclarationBody

namespace MaxRunSoftware.Utilities.Data;

public abstract class DatabaseSchemaObject { }

public class DatabaseSchemaDatabase(string databaseName) : DatabaseSchemaObject, IEquatable<DatabaseSchemaDatabase>, IComparable<DatabaseSchemaDatabase>
{
    //public DatabaseSchemaDatabase(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0])) { }

    public string DatabaseName { get; } = databaseName;

    #region Override

    public override string ToString() => DatabaseName;

    public override int GetHashCode() => Util.Hash(StringComparer.OrdinalIgnoreCase.GetHashCode(DatabaseName));

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaDatabase);

    public bool Equals(DatabaseSchemaDatabase? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!StringComparer.OrdinalIgnoreCase.Equals(DatabaseName, other.DatabaseName)) return false;
        return true;
    }

    public int CompareTo(DatabaseSchemaDatabase? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(other, this)) return 0;

        int c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(DatabaseName, other.DatabaseName))) return c;
        return c;
    }

    #endregion Override
}

public class DatabaseSchemaSchema(DatabaseSchemaDatabase database, string? schemaName) : DatabaseSchemaObject, IEquatable<DatabaseSchemaSchema>, IComparable<DatabaseSchemaSchema>
{
    //public DatabaseSchemaSchema(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0]), row[1].CheckNotNull(columns[1])) { }
    public DatabaseSchemaSchema(string databaseName, string? schemaName) : this(new DatabaseSchemaDatabase(databaseName), schemaName) { }

    public DatabaseSchemaDatabase Database { get; } = database;
    public string? SchemaName { get; } = schemaName;

    #region Override

    public override string ToString() => Database.DatabaseName + (SchemaName == null ? string.Empty : "." + SchemaName);

    // ReSharper disable once RedundantCast
    public override int GetHashCode() => Util.Hash(Database, SchemaName == null ? (int?)null : StringComparer.OrdinalIgnoreCase.GetHashCode(SchemaName));

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaSchema);

    public bool Equals(DatabaseSchemaSchema? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Database, other.Database)) return false;
        if (!StringComparer.OrdinalIgnoreCase.Equals(SchemaName, other.SchemaName)) return false;
        return true;
    }

    public int CompareTo(DatabaseSchemaSchema? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(other, this)) return 0;

        int c;
        if (0 != (c = Database.CompareTo(other.Database))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(SchemaName, other.SchemaName))) return c;
        return c;
    }

    #endregion Override
}

public class DatabaseSchemaTable(DatabaseSchemaSchema schema, string tableName) : DatabaseSchemaObject, IEquatable<DatabaseSchemaTable>, IComparable<DatabaseSchemaTable>
{
    //public DatabaseSchemaTable(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0]), row[1].CheckNotNull(columns[1]), row[2].CheckNotNull(columns[2])) { }
    public DatabaseSchemaTable(string databaseName, string? schemaName, string tableName) : this(new(databaseName, schemaName), tableName) { }

    public DatabaseSchemaSchema Schema { get; } = schema;
    public string TableName { get; } = tableName;

    #region Override

    public override string ToString() => Schema + "." + TableName;

    // ReSharper disable once RedundantCast
    public override int GetHashCode() => Util.Hash(Schema, StringComparer.OrdinalIgnoreCase.GetHashCode(TableName));

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTable);

    public bool Equals(DatabaseSchemaTable? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Schema, other.Schema)) return false;
        if (!StringComparer.OrdinalIgnoreCase.Equals(TableName, other.TableName)) return false;
        return true;
    }

    public int CompareTo(DatabaseSchemaTable? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(other, this)) return 0;

        int c;
        if (0 != (c = Schema.CompareTo(other.Schema))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(TableName, other.TableName))) return c;
        return c;
    }

    #endregion Override
}

public class DatabaseSchemaColumn(
    string columnName,
    string columnType,
    DbType columnDbType,
    bool isNullable,
    int ordinal,
    long? characterLengthMax = null,
    int? numericPrecision = null,
    int? numericScale = null,
    string? columnDefault = null)
    : DatabaseSchemaObject
{
    public string ColumnName { get; } = columnName;
    public string ColumnType { get; } = columnType.CheckNotNullTrimmed();
    public DbType ColumnDbType { get; } = columnDbType;
    public bool IsNullable { get; } = isNullable;
    public int Ordinal { get; } = ordinal;
    public long? CharacterLengthMax { get; } = characterLengthMax;
    public int? NumericPrecision { get; } = numericPrecision;
    public int? NumericScale { get; } = numericScale;
    public string? ColumnDefault { get; } = columnDefault;

    #region Override

    public override string ToString() => ColumnName;

    public override int GetHashCode() => Util.Hash(Ordinal);

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaColumn);

    public bool Equals(DatabaseSchemaColumn? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Ordinal.Equals(other.Ordinal)) return false;
        return true;
    }

    public int CompareTo(DatabaseSchemaColumn? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(other, this)) return 0;

        int c;
        if (0 != (c = Ordinal.CompareTo(other.Ordinal))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(ColumnName, other.ColumnName))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(ColumnType, other.ColumnType))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(ColumnDbType, other.ColumnDbType))) return c;
        if (0 != (c = IsNullable.CompareTo(other.IsNullable))) return c;
        return c;
    }

    #endregion Override
}

public class DatabaseSchemaTableColumn(DatabaseSchemaTable table, DatabaseSchemaColumn column) : DatabaseSchemaObject, IEquatable<DatabaseSchemaTableColumn>, IComparable<DatabaseSchemaTableColumn>
{
    public DatabaseSchemaTable Table { get; } = table;
    public DatabaseSchemaColumn Column { get; } = column;

    #region Override

    public override string ToString() => Table + "." + Column;

    public override int GetHashCode() => Util.Hash(Table, Column);

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTableColumn);

    public bool Equals(DatabaseSchemaTableColumn? other)
    {
        if (ReferenceEquals(other, null)) return false;
        if (ReferenceEquals(other, this)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Table, other.Table)) return false;
        if (!Util.IsEqual(Column, other.Column)) return false;
        return true;
    }

    public int CompareTo(DatabaseSchemaTableColumn? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(other, this)) return 0;

        int c;
        if (0 != (c = Table.CompareTo(other.Table))) return c;
        if (0 != (c = Column.CompareTo(other.Column))) return c;
        return c;
    }

    #endregion Override
}
