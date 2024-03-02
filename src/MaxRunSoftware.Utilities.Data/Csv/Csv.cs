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

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace MaxRunSoftware.Utilities.Data;

public static class Csv
{
    public static CsvConfiguration CreateDefaultConfig() => new(CultureInfo.InvariantCulture)
    {
        BufferSize = (int)Bytes.Mega.Value * 1,
        ProcessFieldBufferSize = (int)Bytes.Kilo.Value * 8,
        DetectColumnCountChanges = false,
        IgnoreBlankLines = false,
        HasHeaderRecord = true,
        Encoding = Constant.Encoding_UTF8_Without_BOM,
    };

    public static ITable ReadString(string data, CsvConfiguration config)
    {
        IReadOnlyList<ITableColumn>? columns = null;
        var rows = new List<ITableRow>();

        using (var r = new StringReader(data))
        {
            using (var csv = new CsvReader(r, config))
            {
                if (!csv.Read()) return new Table(columns, rows);

                if (config.HasHeaderRecord)
                {
                    columns = csv.ReadColumns();
                    if (!csv.Read()) return new Table(columns, rows);
                }

                var rowIndex = 0;
                do
                {
                    var row = csv.ReadRow(rowIndex);
                    rows.Add(row);
                    rowIndex++;
                } while (csv.Read());

                return new Table(columns, rows);
            }
        }
    }

    public static ITable ReadFile(string file, CsvConfiguration config, long streamThreshold = Constant.Bytes_Mega * 100L, Encoding? encoding = null)
    {
        file.CheckFileExists();
        encoding ??= config.Encoding;
        encoding = encoding.CheckNotNull();

        var fileLength = streamThreshold == long.MaxValue ? 0 : Util.FileGetLength(file);
        if (fileLength <= streamThreshold)
        {
            return ReadString(Util.FileRead(file, encoding), config);
        }

        return new CsvTableStreaming(() => new(Util.FileOpenRead(file), encoding), config);
    }

    public static string WriteString(ITable table, CsvConfiguration config)
    {
        using var w = new StringWriter();
        WriteStream(table, config, w);
        w.Flush();
        return w.ToString();
    }

    public static void WriteFile(ITable table, CsvConfiguration config, string file, Encoding? encoding = null)
    {
        using var s = Util.FileOpenWrite(file);
        WriteStream(table, config, s, encoding);
    }

    public static void WriteStream(ITable table, CsvConfiguration config, Stream stream, Encoding? encoding = null)
    {
        encoding ??= config.Encoding;
        encoding = encoding.CheckNotNull();
        using var w = new StreamWriter(stream, encoding);
        WriteStream(table, config, w);
    }
    public static void WriteStream(ITable table, CsvConfiguration config, TextWriter writer)
    {
        using var csv = new CsvWriter(writer, config);
        if (config.HasHeaderRecord)
        {
            if (table.Columns == null) throw new ArgumentException($"{nameof(CsvConfiguration)}.{nameof(CsvConfiguration.HasHeaderRecord)}={config.HasHeaderRecord} but {table.GetType().NameFormatted()}.{table.Columns}=null", nameof(config));
            foreach (var column in table.Columns.OrderBy(o => o.Index))
            {
                csv.WriteField(column.Name ?? string.Empty);
            }

            csv.NextRecord();
        }

        foreach (var row in table.Rows)
        {
            foreach (var item in row)
            {
                var itemString = item == null ? string.Empty : item.ToStringGuessFormat() ?? string.Empty;
                csv.WriteField(itemString);
            }

            csv.NextRecord();
        }

        csv.Flush();
    }
}
