// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
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

using CsvHelper;

namespace MaxRunSoftware.Utilities.Data;

internal static class CsvHelperExtensions
{
    public static IReadOnlyList<ITableColumn>? ReadColumns(this CsvReader csv)
    {
        csv.ReadHeader();
        var header = csv.HeaderRecord;
        if (header == null) return null;

        var array = new ITableColumn[header.Length];

        for (var i = 0; i < array.Length; i++)
        {
            array[i] = new TableColumn(i, header[i]);
        }

        return array;
    }

    public static ITableRow ReadRow(this CsvReader csv, int index) => new TableRow(index, csv.Parser?.Record ?? Array.Empty<object?>());
}
