// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

using Microsoft.Extensions.Logging;
// ReSharper disable StringLiteralTypo

namespace MaxRunSoftware.Utilities.Tests;

public class ConstantLoggingTests : TestBase
{
    public ConstantLoggingTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [Theory]
    [InlineData("n")]
    [InlineData("None")]
    [InlineData("noNE")]
    public void String_LogLevel_Contains_None(string name) => String_LogLevel_Test(name, LogLevel.None);

    [Theory]
    [InlineData("t")]
    [InlineData("Trace")]
    [InlineData("traCe")]
    public void String_LogLevel_Contains_Trace(string name) => String_LogLevel_Test(name, LogLevel.Trace);

    [Theory]
    [InlineData("d")]
    [InlineData("Debug")]
    [InlineData("deBug")]
    public void String_LogLevel_Contains_Debug(string name) => String_LogLevel_Test(name, LogLevel.Debug);

    [Theory]
    [InlineData("i")]
    [InlineData("Information")]
    [InlineData("InfoRmation")]
    [InlineData("info")]
    [InlineData("iNfO")]
    public void String_LogLevel_Contains_Information(string name) => String_LogLevel_Test(name, LogLevel.Information);


    [Theory]
    [InlineData("w")]
    [InlineData("Warning")]
    [InlineData("WaRning")]
    [InlineData("warn")]
    [InlineData("WaRn")]
    public void String_LogLevel_Contains_Warning(string name) => String_LogLevel_Test(name, LogLevel.Warning);

    [Theory]
    [InlineData("e")]
    [InlineData("Error")]
    [InlineData("erRor")]
    public void String_LogLevel_Contains_Error(string name) => String_LogLevel_Test(name, LogLevel.Error);

    [Theory]
    [InlineData("c")]
    [InlineData("Critical")]
    [InlineData("CriTical")]
    public void String_LogLevel_Contains_Critical(string name) => String_LogLevel_Test(name, LogLevel.Critical);


    private void String_LogLevel_Test(string name, LogLevel level)
    {
        Assert.True(Constant.String_LogLevel.ContainsKey(name));
        var logLevel = Constant.String_LogLevel[name];
        Assert.Equal(level, logLevel);
    }

}
