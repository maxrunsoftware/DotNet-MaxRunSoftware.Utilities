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

using NanoXLSX;
using NanoXLSX.Styles;

namespace MaxRunSoftware.Utilities.Data;

public static class Xlsx
{
    public static ImportOptions CreateDefaultConfig() => new()
    {
        EnforceDateTimesAsNumbers = false,
        GlobalEnforcingType = ImportOptions.GlobalType.EverythingToString,
    };
    
    public static string GetColumnName(int columnIndex)
    {
        // https://stackoverflow.com/a/17186287
        columnIndex.CheckMin(0);
        var d = columnIndex;
        var name = "";

        while (d > 0)
        {
            var mod = (d - 1) % 26;
            name = Convert.ToChar('A' + mod) + name;
            d = (d - mod) / 26;
        }

        return name;
    }

    public static int GetColumnIndex(string columnName)
    {
        Cell.ResolveCellCoordinate(columnName, out var column, out _, out _);
        return column;
    }
    
    public static IEnumerable<(string, ITable)> ReadFile(string file, ImportOptions importOptions)
    {
        file.CheckFileExists();
        var bytes = Util.FileRead(file);
        return ReadBytes(bytes, importOptions);
    }
    
    public static IEnumerable<(string, ITable)> ReadBytes(byte[] bytes, ImportOptions importOptions)
    {
        var stream = new MemoryStream(bytes);
        return ReadStream(stream, importOptions);
    }

    public static IEnumerable<(string, ITable)> ReadStream(Stream stream, ImportOptions importOptions)
    {
        var b = Workbook.Load(stream, options: importOptions); // Read the file stream
        foreach (var s in b.Worksheets)
        {
            var sheetName = s.SheetName;
            var table = ReadWorksheet(s);
            yield return (sheetName, table);
        }
    }

    public static ITable ReadWorksheet(Worksheet worksheet)
    {
        var columnIndexMax = -1;
        var rowIndexMax = -1;

        var columns = new Dictionary<int, Dictionary<int, object>>();
        foreach (var cell in worksheet.Cells.Values)
        {
            var cellAddress = cell.CellAddress2;
            var columnIndex = cellAddress.Column;
            var rowIndex = cellAddress.Row;
            var cellValue = ReadCell(cell);
            if (cellValue == null) continue;
            
            columnIndexMax = Math.Max(columnIndexMax, columnIndex);
            rowIndexMax = Math.Max(rowIndexMax, rowIndex);

            if (!columns.TryGetValue(columnIndex, out var rowDict))
            {
                rowDict = new();
                columns.Add(columnIndex, rowDict);
            }

            rowDict[rowIndex] = cellValue;
        }

        
        var tableColumns = new List<TableColumn>();
        for (var i = 0; i < columnIndexMax; i++)
        {
            tableColumns.Add(new(i, GetColumnName(i)));
        }
        
        var tableRows = new List<TableRow>();
        for (var i = 0; i < rowIndexMax; i++)
        {
            var array = new object?[columnIndexMax];
            for (var j = 0; j < columnIndexMax; j++)
            {
                array[j] = columns.TryGetValue(j, out var rowDict) && rowDict.TryGetValue(i, out var rowValue) ? rowValue : null;
            }
            tableRows.Add(new(i, array));
        }
        
        return new Table(tableColumns, tableRows);
    }

    public static object? ReadCell(Cell cell)
    {
        //var t = cell.DataType;
        var cellValue = cell.Value;
            
        var cellValueString = cellValue.ToString();
        if (string.IsNullOrEmpty(cellValueString)) return null;
        
        var cellValueStringTrimmed = cellValueString.TrimOrNull();
        if (cellValueStringTrimmed == null) return null;

        return cellValue;
    }

    public static void WriteStream(Stream stream, IEnumerable<XlsxWriterSheet> sheets)
    {
        var wb = new Workbook(false);
        foreach (var sheet in sheets)
        {
           wb.AddWorksheet(sheet.SheetName, true);
           WriteWorksheet(wb, sheet);
               
        }
    }

    private static void WriteWorksheet(Workbook workbook, XlsxWriterSheet sheet)
    {
        workbook.AddWorksheet(sheet.SheetName, true);
        //workbook.CurrentWorksheet.CurrentCellDirection = Worksheet.CellDirection.ColumnToColumn;
        workbook.CurrentWorksheet.CurrentCellDirection = Worksheet.CellDirection.Disabled;

        var table = sheet.Table;
        var outputRow = 0;
        var columns = table.Columns?.ToArray() ?? [];
        if (columns.Length > 0 && sheet.HeaderInclude)
        {
            var headerFormatter = sheet.HeaderFormatter;
            foreach (var column in columns)
            {
                object? headerValue = column.Name;
                Style? headerStyle = null;
                if (headerFormatter != null)
                {
                    var cell = new XlsxWriterCellHeader
                    {
                        SheetName = sheet.SheetName,
                        Table = table,
                        ColumnIndex = column.Index,
                        Column = column,
                        Value = headerValue,
                    };

                    headerFormatter(cell);
                    headerValue = cell.Value;
                    headerStyle = cell.Style;

                }

                workbook.CurrentWorksheet.AddCell(headerValue, column.Index, outputRow, headerStyle);
            }

            outputRow++;
        }

        var dataFormatter = sheet.DataFormatter;
        var currentRowIndex = 0;
        foreach (var row in table.Rows)
        {
            var currentColumnIndex = 0;
            foreach (var rowValue in row)
            {
                object? dataValue = rowValue;
                Style? dataStyle = null;
                if (dataFormatter != null)
                {
                    var cell = new XlsxWriterCellData
                    {
                        SheetName = sheet.SheetName,
                        Table = table,
                        ColumnIndex = currentColumnIndex,
                        Column = columns.GetAtIndexOrDefault(currentColumnIndex),
                        RowIndex = currentRowIndex,
                        Row = row,
                        Value = dataValue,
                    };
                    dataFormatter(cell);
                    dataValue = cell.Value;
                    dataStyle = cell.Style;
                }

                workbook.CurrentWorksheet.AddCell(dataValue, currentColumnIndex, outputRow, dataStyle);
                currentColumnIndex++;
            }

            currentRowIndex++;
            outputRow++;
        }

    }
}

public class XlsxWriterSheet
{
    public required string SheetName { get; set; }
    public required ITable Table { get; set; }
    
    public bool HeaderInclude { get; set; } = true;
    public Action<XlsxWriterCellHeader>? HeaderFormatter { get; set; }
    
    public bool DataInclude { get; set; } = true;
    public Action<XlsxWriterCellData>? DataFormatter { get; set; }
    
}

public class XlsxWriterCellHeader
{
    public required string SheetName { get; init; }
    public required ITable Table { get; init; }
    public required int ColumnIndex { get; init; }
    public required ITableColumn Column { get; init; }
    public object? Value { get; set; }
    public Style? Style { get; set; }
}

public class XlsxWriterCellData
{
    public required string SheetName { get; init; }
    public required ITable Table { get; init; }
    public required int ColumnIndex { get; init; }
    public required ITableColumn? Column { get; init; }
    public required int RowIndex { get; init; }
    public required ITableRow Row { get; init; }
    public object? Value { get; set; }
    public Style? Style { get; set; }
}
