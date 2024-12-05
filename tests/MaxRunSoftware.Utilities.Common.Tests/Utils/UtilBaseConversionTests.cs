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

using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common.Tests.Utils;

public class UtilBaseConversionTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableTheory]
    [InlineData(new byte[] {0}, "00")]
    [InlineData(new byte[] {1}, "01")]
    [InlineData(new byte[] {15}, "0F")]
    [InlineData(new byte[] {16}, "10")]
    [InlineData(new byte[] {255}, "FF")]
    [InlineData(new byte[] {0, 0}, "0000")]
    [InlineData(new byte[] {0, 1}, "0001")]
    [InlineData(new byte[] {1, 0}, "0100")]
    [InlineData(new byte[] {1, 1}, "0101")]
    [InlineData(new byte[] {255, 1}, "FF01")]
    [InlineData(new byte[] {255, 255}, "FFFF")]
    [InlineData(new byte[] {1, 2, 3}, "010203")]
    public void Base16(byte[] bytes, string str)
    {
        Assert.Equal(str, Util.Base16(bytes));
        Assert.Equal(bytes, Util.Base16(str));

        ReadOnlySpan<byte> spanByte = bytes.AsSpan();
        Span<char> spanChar = new char[spanByte.Length * 2];
        Util.Base16(spanByte, spanChar);
        var spanString = new string(spanChar);
        Assert.Equal(str, spanString);
    }
    
    

    [SkippableTheory]
    [InlineData(0, "00")]
    [InlineData(1, "01")]
    [InlineData(15, "0F")]
    [InlineData(16, "10")]
    [InlineData(255, "FF")]
    public void Base16_Encode_Single(byte b, string expected)
    {
        Assert.Equal(expected, Util.Base16(b));
    }

    
    
   

}
