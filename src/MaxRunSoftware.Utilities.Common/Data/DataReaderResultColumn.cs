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

namespace MaxRunSoftware.Utilities.Common;

public class DataReaderResultColumn(DataReaderSchemaColumn schemaColumn, DataReaderResultColumnCollection columnCollection) : ITableColumn
{
    private DataReaderSchemaColumn SchemaColumn { get; } = schemaColumn.CheckNotNull(nameof(schemaColumn));

    public DataReaderResultColumnCollection ColumnCollection { get; } = columnCollection;

    public int Index => SchemaColumn.Index;
    public string Name => SchemaColumn.ColumnName;
    public Type Type => SchemaColumn.FieldType;
    public string DataTypeName => SchemaColumn.DataTypeName;

    public bool IsNullable => ColumnCollection.nullableColumns[Index];
}
