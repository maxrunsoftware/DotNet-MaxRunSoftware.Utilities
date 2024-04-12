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

namespace MaxRunSoftware.Utilities.Testing.Tests;

public class TestBaseBaseTests(ITestOutputHelper testOutputHelper) : TestBaseBase(testOutputHelper)
{
    [SkippableFact]
    public void Log_Colors()
    {
        IsColorEnabled = true;
        LogLevel = LogLevel.Trace;
        var logLevels = Util.GetEnumValues<LogLevel>().OrderBy(o => (int)o).ToArray();
        foreach (var logLevel in logLevels)
        {
            log.Log(logLevel, "My [{LogLevel}] Message", logLevel.ToString());
        }
    }
}
