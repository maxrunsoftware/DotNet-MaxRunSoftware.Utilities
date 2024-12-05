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

public partial class DataReaderResult
{
    public int Index { get; }
    public DataReaderResultColumnCollection Columns { get; }
    public DataReaderResultRowCollection Rows { get; }

    public DataReaderResult(IDataReader reader, int index = 0)
    {
        //Constant.GetLogger<DataReaderResult>().LogTrace("Reading " + nameof(DataReaderResult) + "[{Index}]", index);
        Index = index;
        Columns = new(reader, this); // Important to construct Columns before Rows
        Rows = new(reader, this);
    }

}

public partial class DataReaderResult : ITable
{
    IReadOnlyList<ITableRow> ITable.Rows => Rows;
    IReadOnlyList<ITableColumn> ITable.Columns => Columns;
}
