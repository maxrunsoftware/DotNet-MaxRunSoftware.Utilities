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

public abstract class SqlSchemaObject
{
    protected static int Hash(params string?[] values) => Util.HashEnumerable(values.Select(o => o?.ToUpperInvariant()));
}

public class SqlSchemaDatabase : SqlSchemaObject, IEquatable<SqlSchemaDatabase>
{
    private readonly int hashCode;
    public string DatabaseName { get; }

    public SqlSchemaDatabase(string? databaseName)
    {
        DatabaseName = databaseName.CheckNotNullTrimmed();

        hashCode = Hash(DatabaseName);
    }

    public override bool Equals(object? obj) => Equals(obj as SqlSchemaDatabase);

    public bool Equals(SqlSchemaDatabase? other)
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

public class SqlSchemaSchema : SqlSchemaObject, IEquatable<SqlSchemaSchema>
{
    private readonly int hashCode;
    public string DatabaseName { get; }
    public string SchemaName { get; }

    public SqlSchemaSchema(string? databaseName, string? schemaName)
    {
        DatabaseName = databaseName.CheckNotNullTrimmed();
        SchemaName = schemaName.CheckNotNullTrimmed();

        hashCode = Hash(DatabaseName, SchemaName);
    }

    public override bool Equals(object? obj) => Equals(obj as SqlSchemaSchema);

    public bool Equals(SqlSchemaSchema? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqualCaseInsensitive(DatabaseName, other.DatabaseName)) return false;
        if (!Util.IsEqualCaseInsensitive(SchemaName, other.SchemaName)) return false;
        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => SchemaName;
}

public class SqlSchemaTable : SqlSchemaObject, IEquatable<SqlSchemaTable>
{
    private readonly int hashCode;
    public string DatabaseName { get; }
    public string SchemaName { get; }
    public string TableName { get; }

    public SqlSchemaTable(string? databaseName, string? schemaName, string? tableName)
    {
        DatabaseName = databaseName.CheckNotNullTrimmed();
        SchemaName = schemaName.CheckNotNullTrimmed();
        TableName = tableName.CheckNotNullTrimmed();

        hashCode = Hash(DatabaseName, SchemaName, TableName);
    }

    public override bool Equals(object? obj) => Equals(obj as SqlSchemaTable);

    public bool Equals(SqlSchemaTable? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqualCaseInsensitive(DatabaseName, other.DatabaseName)) return false;
        if (!Util.IsEqualCaseInsensitive(SchemaName, other.SchemaName)) return false;
        if (!Util.IsEqualCaseInsensitive(TableName, other.TableName)) return false;
        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => TableName;
}

public class SqlSchemaTableColumn : SqlSchemaObject, IEquatable<SqlSchemaTableColumn>
{
    private readonly int hashCode;
    public string DatabaseName { get; }
    public string SchemaName { get; }
    public string TableName { get; }
    public string ColumnName { get; }
    public string ColumnType { get; }

    public DbType ColumnDbType { get; }
    public bool IsNullable { get; }
    public int Ordinal { get; }
    public long? CharacterLengthMax { get; }
    public int? NumericPrecision { get; }
    public int? NumericScale { get; }
    public string? ColumnDefault { get; }

    public SqlSchemaTableColumn(
        string? databaseName,
        string? schemaName,
        string? tableName,
        string? columnName,
        string? columnType,
        DbType columnDbType,
        bool isNullable,
        int ordinal,
        long? characterLengthMax = null,
        int? numericPrecision = null,
        int? numericScale = null,
        string? columnDefault = null
    )
    {
        DatabaseName = databaseName.CheckNotNullTrimmed();
        SchemaName = schemaName.CheckNotNullTrimmed();
        TableName = tableName.CheckNotNullTrimmed();
        ColumnName = columnName.CheckNotNullTrimmed();
        ColumnType = columnType.CheckNotNullTrimmed();

        ColumnDbType = columnDbType;
        IsNullable = isNullable;
        Ordinal = ordinal;
        CharacterLengthMax = characterLengthMax;
        NumericPrecision = numericPrecision;
        NumericScale = numericScale;
        ColumnDefault = columnDefault;

        hashCode = Hash(DatabaseName, SchemaName, TableName, ColumnName, Ordinal.ToString());
    }

    public override bool Equals(object? obj) => Equals(obj as SqlSchemaTableColumn);

    public bool Equals(SqlSchemaTableColumn? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetHashCode() != other.GetHashCode()) return false;
        if (!Util.IsEqualCaseInsensitive(DatabaseName, other.DatabaseName)) return false;
        if (!Util.IsEqualCaseInsensitive(SchemaName, other.SchemaName)) return false;
        if (!Util.IsEqualCaseInsensitive(TableName, other.TableName)) return false;
        if (!Util.IsEqualCaseInsensitive(ColumnName, other.ColumnName)) return false;
        if (Ordinal != other.Ordinal) return false;

        return true;
    }

    public override int GetHashCode() => hashCode;

    public override string ToString() => ColumnName;
}
