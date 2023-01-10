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

public abstract class DatabaseSchemaObject
{
    protected DatabaseSchemaObject(string name, DatabaseSchemaObject? parent = null)
    {
        Name = name;
        Parent = parent;
        NameFullParts = Parent == null
            ? ImmutableArray.Create(Name)
            : Parent.NameFullParts.Add(Name);
        NameFull = NameFullParts.ToStringDelimited(".");
        getHashCode = new Lzy<int>(GetHashCodeCreateIndirect);
    }
    public ImmutableArray<string> NameFullParts { get; }
    protected DatabaseSchemaObject? Parent { get; }
    public string Name { get; }
    public string NameFull { get; }
    public override string ToString() => Name;
    private readonly Lzy<int> getHashCode;
    protected int GetHashCodeValue => getHashCode.Value;
    protected virtual int GetHashCodeCreate() => Util.Hash(Parent, Name.ToUpperInvariant());
    private int GetHashCodeCreateIndirect() => GetHashCodeCreate();
}


public class DatabaseSchemaDatabase : DatabaseSchemaObject, IEquatable<DatabaseSchemaDatabase>, IComparable<DatabaseSchemaDatabase>
{
    public DatabaseSchemaDatabase(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0])) { }
    public DatabaseSchemaDatabase(string databaseName) : base(databaseName) {}

    public int CompareTo(DatabaseSchemaDatabase? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        return c;
    }
    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaDatabase);
    public override int GetHashCode() => GetHashCodeValue;
    public bool Equals(DatabaseSchemaDatabase? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqualCaseInsensitive(Name, other.Name)) return false;
        return true;
    }
}

public class DatabaseSchemaSchema : DatabaseSchemaObject, IEquatable<DatabaseSchemaSchema>, IComparable<DatabaseSchemaSchema>
{
    public DatabaseSchemaDatabase Database => (DatabaseSchemaDatabase)Parent! ?? throw new NullReferenceException();

    public DatabaseSchemaSchema(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0]), row[1].CheckNotNull(columns[1])) { }
    public DatabaseSchemaSchema(string databaseName, string schemaName) : this(new DatabaseSchemaDatabase(databaseName), schemaName) { }
    public DatabaseSchemaSchema(DatabaseSchemaDatabase database, string schemaName) : base(schemaName, database) {}

    public int CompareTo(DatabaseSchemaSchema? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Database.CompareTo(other.Database))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        return c;
    }

    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaSchema);
    public override int GetHashCode() => GetHashCodeValue;
    public bool Equals(DatabaseSchemaSchema? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Parent, other.Parent)) return false;
        if (!Util.IsEqualCaseInsensitive(Name, other.Name)) return false;
        return true;
    }


}

public class DatabaseSchemaTable : DatabaseSchemaObject, IEquatable<DatabaseSchemaTable>, IComparable<DatabaseSchemaTable>
{
    public DatabaseSchemaSchema Schema => (DatabaseSchemaSchema)Parent! ?? throw new NullReferenceException();

    public DatabaseSchemaTable(string?[] row, Dictionary<int, string> columns) : this(row[0].CheckNotNull(columns[0]), row[1].CheckNotNull(columns[1]), row[2].CheckNotNull(columns[2])) { }
    public DatabaseSchemaTable(string databaseName, string schemaName, string tableName) : this(new DatabaseSchemaSchema(databaseName, schemaName), tableName) { }

    public DatabaseSchemaTable(DatabaseSchemaSchema schema, string tableName) : base(tableName, schema) {}

    public int CompareTo(DatabaseSchemaTable? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Schema.CompareTo(other.Schema))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        return c;
    }
    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTable);
    public override int GetHashCode() => GetHashCodeValue;
    public bool Equals(DatabaseSchemaTable? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Parent, other.Parent)) return false;
        if (!Util.IsEqualCaseInsensitive(Name, other.Name)) return false;
        return true;
    }

}

public class DatabaseSchemaTableColumn : DatabaseSchemaObject, IEquatable<DatabaseSchemaTableColumn>, IComparable<DatabaseSchemaTableColumn>
{
    public DatabaseSchemaTable Table => (DatabaseSchemaTable)Parent! ?? throw new NullReferenceException();

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
    ) : base(columnName, table)
    {
        ColumnType = columnType.CheckNotNullTrimmed();

        ColumnDbType = columnDbType;
        IsNullable = isNullable;
        Ordinal = ordinal;
        CharacterLengthMax = characterLengthMax;
        NumericPrecision = numericPrecision;
        NumericScale = numericScale;
        ColumnDefault = columnDefault;
    }
    protected override int GetHashCodeCreate() => Util.Hash(base.GetHashCodeCreate(), Ordinal);

    public int CompareTo(DatabaseSchemaTableColumn? other)
    {
        if (ReferenceEquals(other, null)) return 1;
        if (ReferenceEquals(this, other)) return 0;

        int c;
        if (0 != (c = Table.CompareTo(other.Table))) return c;
        if (0 != (c = Ordinal.CompareTo(other.Ordinal))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(Name, other.Name))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(ColumnType, other.ColumnType))) return c;
        if (0 != (c = Constant.StringComparer_OrdinalIgnoreCase_Ordinal.Compare(ColumnDbType, other.ColumnDbType))) return c;
        if (0 != (c = IsNullable.CompareTo(other.IsNullable))) return c;
        return c;
    }
    public override bool Equals(object? obj) => Equals(obj as DatabaseSchemaTableColumn);

    public bool Equals(DatabaseSchemaTableColumn? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqual(Parent, other.Parent)) return false;
        if (!Util.IsEqualCaseInsensitive(Name, other.Name)) return false;
        if (Ordinal != other.Ordinal) return false;

        return true;
    }

    public override int GetHashCode() => GetHashCodeValue;

}
