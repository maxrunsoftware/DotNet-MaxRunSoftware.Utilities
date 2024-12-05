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

using CsvHelper.Configuration;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedType.Global

namespace MaxRunSoftware.Utilities.Data.Tests;

#nullable enable

public class CsvTests_String(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, null);

public class CsvTests_UTF8(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Encoding.UTF8);

public class CsvTests_UTF8_With_BOM(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Constant.Encoding_UTF8_With_BOM);

public class CsvTests_UTF8_Without_BOM(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Constant.Encoding_UTF8_Without_BOM);

public class CsvTests_Unicode(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Encoding.Unicode);

public class CsvTests_ASCII(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Encoding.ASCII);

public class CsvTests_Latin1(ITestOutputHelper testOutputHelper) : CsvTests(testOutputHelper, Encoding.Latin1);

public abstract class CsvTests : TestBase
{
    protected readonly CsvConfiguration config;

    protected virtual ITable CreateTable(string data, CsvConfiguration c)
    {
        if (encoding == null) return Csv.ReadString(data, c);

        var file = CreateTestFile();
        Util.FileWrite(file, data, c.Encoding ?? encoding);
        return Csv.ReadFile(file, c, 0L, c.Encoding ?? encoding);
    }

    private readonly Encoding? encoding;

    protected CsvTests(ITestOutputHelper testOutputHelper, Encoding? encoding) : base(testOutputHelper)
    {
        config = Csv.CreateDefaultConfig();
        this.encoding = encoding;
        if (encoding != null) config.Encoding = encoding;
        log.LogDebug("Encoding={Encoding}", encoding?.EncodingName);
    }

    [SkippableFact]
    public void Read_Simple() => TestReadWithHeader(CreateTable("c1,c2,c3\na,b,c\nd,e,f", config), "c1,c2,c3|a,b,c|d,e,f");

    [SkippableFact]
    public void Read_Blank_Lines_Ignored()
    {
        config.IgnoreBlankLines = true;
        TestReadWithHeader(CreateTable("c1,c2,c3\na,b,c\n\nd,e,f\n", config), "c1,c2,c3|a,b,c|d,e,f");
    }

    [SkippableFact]
    public void Read_Duplicate_Column_Names()
    {
        config.IgnoreBlankLines = true;
        TestReadWithHeader(CreateTable("c,c,c\na,b,c\n\nd,e,f\n", config), "c,c,c|a,b,c|d,e,f");
    }

    [SkippableFact]
    public void Read_Uneven_Columns() => TestReadWithHeader(CreateTable("c1,c2,c3\na,b,c\nd,e,f,g\nh,i,j", config), "c1,c2,c3|a,b,c|d,e,f,g|h,i,j");

    [SkippableFact]
    public void Read_Partial_Double_Quotes() => TestReadWithHeader(CreateTable("c1,`c2`,c3\na,`b`,c\nd,e,`f`".Replace('`', '"'), config), "c1,c2,c3|a,b,c|d,e,f");

    [SkippableFact]
    public void Read_Without_Header()
    {
        config.HasHeaderRecord = false;
        TestReadWithoutHeader(CreateTable("a,b,c\nd,e,f", config), "a,b,c|d,e,f");
    }

    [SkippableFact]
    public void Read_Blank_Lines_Ignored_Without_Header()
    {
        config.HasHeaderRecord = false;
        config.IgnoreBlankLines = true;
        TestReadWithoutHeader(CreateTable("\na,b,c\n\nd,e,f\n", config), "a,b,c|d,e,f");
    }

    [SkippableFact]
    public void Write_Simple() => TestWrite(CreateTableMemory("c1,c2,c3\na,b,c\nd,e,f"), "c1,c2,c3\na,b,c\nd,e,f\n");

    [SkippableFact]
    public void Write_No_Header()
    {
        config.HasHeaderRecord = false;
        TestWrite(CreateTableMemory("c1,c2,c3\na,b,c\nd,e,f"), "a,b,c\nd,e,f\n");
    }

    [SkippableFact]
    public void Write_Includes_Blank_Lines_1() => TestWrite(CreateTableMemory("a\n\nc\nd"), "a\n\nc\nd\n");

    [SkippableFact]
    public void Write_Includes_Blank_Lines_End() => TestWrite(CreateTableMemory("a\n\nc\nd\n"), "a\n\nc\nd\n\n");


    private void TestWrite(ITable table, string expected)
    {
        string fileData;
        if (encoding == null)
        {
            fileData = Csv.WriteString(table, config);
        }
        else
        {
            var file = CreateTestFile();
            Csv.WriteFile(table, config, file);
            log.LogDebug("config.Encoding={Encoding}", config.Encoding?.EncodingName);
            fileData = Util.FileRead(file, config.Encoding.CheckNotNull());
        }

        fileData = fileData.ReplaceLineEndings("\n");
        Assert.Equal(expected, fileData);
    }

    private ITable CreateTableMemory(string data, bool hasHeader = true)
    {
        var list = data.SplitOn(Constant.NewLines.Append("|")).Select(SplitLine).ToList();
        var header = hasHeader ? list.PopHead() : null;
        var headerColumns = header?.Select((o, i) => new TableColumn(i, o?.ToString())).ToList();
        var rows = list.Select((o, i) => new TableRow(i, o)).ToList();
        return new Table(headerColumns, rows);
    }

    private void TestReadWithoutHeader(ITable table, string data)
    {
        var items = data.Split('|').Select(o => o.Replace('`', '"')).ToList();
        TestRead(table, null, items.ToArray());
    }
    private void TestReadWithHeader(ITable table, string data)
    {
        var items = data.Split('|').Select(o => o.Replace('`', '"')).ToList();
        var header = items.PopHead();
        if (string.Equals("null", header, StringComparison.OrdinalIgnoreCase)) header = null;

        TestRead(table, header, items.ToArray());
    }

    private static object?[] SplitLine(string row) => row.Split(',').Select(value => string.Equals("null", value, StringComparison.OrdinalIgnoreCase) ? null : value).Cast<object?>().ToArray();


    private void TestRead(ITable table, string? columns, params string[] rows)
    {
        if (columns == null)
        {
            Assert.Null(table.Columns);
        }
        else
        {
            Assert.NotNull(table.Columns);
            var columnParts = columns.Split(',');
            Assert.Equal(columnParts.Length, table.Columns.Count);
            Assert.Equal(columnParts, table.Columns.Select(o => o.Name));
            Assert.Equal(Enumerable.Range(0, columnParts.Length), table.Columns.Select(o => o.Index));
        }

        var expected = rows.Select(SplitLine).ToArray();
        var actual = table.Rows.Select(o => o.ToArray()).ToArray();

        Assert.Equal(expected, actual);

        Assert.Equal(rows.Length, table.Rows.Count);

        for (var i = 0; i < rows.Length; i++)
        {
            var rowParts = SplitLine(rows[i]);
            var row = table.Rows[i];
            Assert.Equal(i, row.Index);
            Assert.Equal(rowParts, row);
        }

        var c = 0;
        foreach (var row in table.Rows)
        {
            var rowParts = SplitLine(rows[c]);
            Assert.Equal(c, row.Index);
            Assert.Equal(rowParts, row);
            c++;
        }
    }
}
