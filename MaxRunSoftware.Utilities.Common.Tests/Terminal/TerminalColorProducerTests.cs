// Copyright (c) 2023 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common.Tests.Terminal;

public class TerminalColorProducerTests : TestBase
{
    public TerminalColorProducerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    [SkippableFact]
    public void Rainbow_Works()
    {
        for (var percent = Percent.MinValue; percent <= Percent.MaxValue; percent++)
        {
            testOutputHelperWrapper.WriteLine(string.Empty);
            var p = new TerminalColorProducerRainbow(percent);
            for (var i = 0; i < 10; i++)
            {
                var color = p.Next();
                var text = (TestMethodName ?? "???") + $" {percent.ToString(),3}%   R: {color.Color.R.ToString(),3}   G: {color.Color.G.ToString(),3}   B: {color.Color.B.ToString(),3}";
                text = text.FormatTerminal(color, null);
                testOutputHelperWrapper.WriteLine(text);
            }

        }
    }
}
