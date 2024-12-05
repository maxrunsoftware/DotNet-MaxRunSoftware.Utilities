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

public class DataReaderSchemaColumn(int index, string columnName, Type fieldType, string dataTypeName)
{
    public int Index { get; } = index;
    public string ColumnName { get; } = columnName;
    public Type FieldType { get; } = fieldType;
    public string DataTypeName { get; } = dataTypeName;

    public DataReaderSchemaColumn(IDataReader reader, int columnIndex) : this(
        columnIndex, 
        reader.GetName(columnIndex), 
        reader.GetFieldType(columnIndex), 
        reader.GetDataTypeName(columnIndex)
        ) { }

    public static List<DataReaderSchemaColumn> Create(IDataReader reader) =>
        Enumerable.Range(0, reader.FieldCount).Select(i => new DataReaderSchemaColumn(reader, i)).ToList();
}
