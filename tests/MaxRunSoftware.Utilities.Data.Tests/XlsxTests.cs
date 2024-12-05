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

using NanoXLSX;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global

namespace MaxRunSoftware.Utilities.Data.Tests;

#nullable enable

public class XlsxTests : TestBase
{

    public XlsxTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [SkippableFact]
    public void Read_Stream()
    {
        var file = "/Users/user/Downloads/Sample.xlsx";
        PrintWorkbook(file, log, options: null);
    }

    private static void PrintWorkbook(string file, ILogger log, ImportOptions? options = null)
    {
        using var stream = Util.FileOpenRead(file);
        PrintWorkbook(stream, log, options);
    }

    private static void PrintWorkbook(Stream stream, ILogger log, ImportOptions? options = null)
    {
        // https://github.com/rabanti-github/NanoXLSX/blob/master/Demo/Program.cs
        var b = Workbook.Load(stream, options: options); // Read the file stream
        foreach (var s in b.Worksheets)
        {

            log.LogInformation("WORKSHEET");
            log.LogInformation("  SheetID: {SheetID}", s.SheetID);
            log.LogInformation("  SheetName: {SheetName}", s.SheetName);

            log.LogInformation("  CurrentCellDirection: {CurrentCellDirection}", s.CurrentCellDirection);
            log.LogInformation("  GetCurrentColumnNumber: {GetCurrentColumnNumber}", s.GetCurrentColumnNumber());
            log.LogInformation("  GetCurrentRowNumber: {GetCurrentRowNumber}", s.GetCurrentRowNumber());
            log.LogInformation("  GetFirstCellAddress: {GetFirstCellAddress}", s.GetFirstCellAddress());
            log.LogInformation("  GetFirstColumnNumber: {GetFirstColumnNumber}", s.GetFirstColumnNumber());
            log.LogInformation("  GetFirstDataCellAddress: {GetFirstDataCellAddress}", s.GetFirstDataCellAddress());
            log.LogInformation("  GetFirstDataColumnNumber: {GetFirstDataColumnNumber}", s.GetFirstDataColumnNumber());
            log.LogInformation("  GetFirstDataRowNumber: {GetFirstDataRowNumber}", s.GetFirstDataRowNumber());
            log.LogInformation("  GetFirstRowNumber: {GetFirstRowNumber}", s.GetFirstRowNumber());
            log.LogInformation("  GetLastCellAddress: {GetLastCellAddress}", s.GetLastCellAddress());
            log.LogInformation("  GetLastColumnNumber: {GetLastColumnNumber}", s.GetLastColumnNumber());
            log.LogInformation("  GetLastDataCellAddress: {GetLastDataCellAddress}", s.GetLastDataCellAddress());
            log.LogInformation("  GetLastDataColumnNumber: {GetLastDataColumnNumber}", s.GetLastDataColumnNumber());
            log.LogInformation("  GetLastDataRowNumber: {GetLastDataRowNumber}", s.GetLastDataRowNumber());
            log.LogInformation("  GetLastRowNumber: {GetLastRowNumber}", s.GetLastRowNumber());

            log.LogInformation("");
            
            log.LogInformation("  CELLS:");
            
            log.LogInformation("    Count:{Count}", s.Cells.Count);
            foreach (var cell in s.Cells)
            {
                PrintCell(cell.Key, cell.Value, log);
            }
            
            log.LogInformation("");
            log.LogInformation("");
        }
    }

    private static void PrintCell(string cellKey, Cell cell, ILogger log)
    {
        static string InternalID(Cell cell)
        {
            var o = cell.CellStyle?.InternalID;
            return o == null ? string.Empty : o.Value.ToString();
        }
        static string CurrentNumberFormat(Cell cell)
        {
            var o = cell.CellStyle?.CurrentNumberFormat?.Number;
            return o == null ? string.Empty : o.Value.ToString();
        }

        log.LogInformation(
            "    key:{Key}  address:{CellAddress}  column:{Column}  row:{Row}  Type:{DataType}  Style:{Style}  NumberFormat:{NumberFormat}  Value:{Value}",
            cellKey,
            cell.CellAddress,
            cell.CellAddress2.Column,
            cell.CellAddress2.Row,
            cell.DataType,
            InternalID(cell),
            CurrentNumberFormat(cell),
            cell.Value
        );
    }
    
}
