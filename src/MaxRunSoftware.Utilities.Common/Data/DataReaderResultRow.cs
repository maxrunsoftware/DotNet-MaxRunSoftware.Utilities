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

public class DataReaderResultRow : TableRow
{
    public DataReaderResultRow(int index, object?[] data) : base(index, Parse(data, null)) { }

    internal DataReaderResultRow(int index, object?[] data, bool[] nullableColumnIndexes) : base(index, Parse(data, nullableColumnIndexes)) { }

    private static object?[] Parse(object?[] data, bool[]? nullableColumnIndexes)
    {
        if (nullableColumnIndexes == null)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == DBNull.Value) data[i] = null;
            }
        }
        else
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == null)
                {
                    nullableColumnIndexes[i] = true;
                }
                else if (data[i] == DBNull.Value)
                {
                    data[i] = null;
                    nullableColumnIndexes[i] = true;
                }
            }
        }

        return data;
    }
}
