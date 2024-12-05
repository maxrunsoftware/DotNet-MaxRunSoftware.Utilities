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

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

// ReSharper disable once InconsistentNaming
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
public class UtilIOTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableTheory]
    [InlineData(nameof(Encoding.Unicode))]
    [InlineData(nameof(Encoding.BigEndianUnicode))]
    [InlineData(nameof(Encoding.UTF32))]
    [InlineData(nameof(Encoding.UTF8))]
    [InlineData(nameof(Encoding.Default))]
    [InlineData(nameof(Encoding.Latin1))]
    [InlineData(nameof(Encoding.ASCII))]
    public void FileRead_Strips_BOM(string encoding)
    {
        static Encoding GetEncoding(string encoding) => encoding switch
        {
            nameof(Encoding.Unicode) => Encoding.Unicode,
            nameof(Encoding.BigEndianUnicode) => Encoding.BigEndianUnicode,
            nameof(Encoding.UTF32) => Encoding.UTF32,
            nameof(Encoding.UTF8) => Encoding.UTF8,
            nameof(Encoding.Default) => Encoding.Default,
            nameof(Encoding.Latin1) => Encoding.Latin1,
            nameof(Encoding.ASCII) => Encoding.ASCII,
            _ => throw new NotImplementedException(encoding),
        };

        var enc = GetEncoding(encoding);
        var str = "foo\nbar\n";
        var file = CreateTestFile();
        using (var fs = Util.FileOpenWrite(file))
        {
            using (var fw = new StreamWriter(fs, enc))
            {
                fw.Write(str);
            }
        }

        var data = Util.FileRead(file, enc);
        Assert.Equal(str, data);
    }
}
