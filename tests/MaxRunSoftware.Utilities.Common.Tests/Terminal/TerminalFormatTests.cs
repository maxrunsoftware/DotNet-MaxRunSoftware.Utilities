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

namespace MaxRunSoftware.Utilities.Common.Tests.Terminal;

public class TerminalFormatTests(ITestOutputHelper testOutputHelper) : TestBase(testOutputHelper)
{
    
    public static TheoryData<ConsoleColor?> ConsoleColorsData => new(Enum.GetValues<ConsoleColor>().Select(o => (ConsoleColor?)o).Prepend(null));
    
    [SkippableTheory]
    [MemberData(nameof(ConsoleColorsData))]
    public void ConsoleColorsForeground(ConsoleColor? fg)
    {
        TerminalFormat.IsDebugEnabled = false;
        foreach (var bg in Enum.GetValues<ConsoleColor>().Select(o => (ConsoleColor?)o).Prepend(null))
        {
            var text = "Foreground:" + (fg == null ? "null" : fg.ToString()) + " | " + "Background:" + (bg == null ? "null" : bg.ToString());
            var textFormatted = text.FormatTerminal(fg, bg);
            testOutputHelperWrapper.WriteLine(textFormatted);
        }
    }
    
    [SkippableTheory]
    [MemberData(nameof(ConsoleColorsData))]
    public void ConsoleColorsForegroundDebug(ConsoleColor? fg)
    {
        TerminalFormat.IsDebugEnabled = true;
        foreach (var bg in Enum.GetValues<ConsoleColor>().Select(o => (ConsoleColor?)o).Prepend(null))
        {
            var text = "Foreground:" + (fg == null ? "null" : fg.ToString()) + " | " + "Background:" + (bg == null ? "null" : bg.ToString());
            var textFormatted = text.FormatTerminal(fg, bg);
            testOutputHelperWrapper.WriteLine(textFormatted);
        }
        TerminalFormat.IsDebugEnabled = false;
    }
    
    [SkippableTheory]
    [MemberData(nameof(ConsoleColorsData))]
    public void ConsoleColorsBackground(ConsoleColor? bg)
    {
        TerminalFormat.IsDebugEnabled = false;
        foreach (var fg in Enum.GetValues<ConsoleColor>().Select(o => (ConsoleColor?)o).Prepend(null))
        {
            var text = "Foreground:" + (fg == null ? "null" : fg.ToString()) + " | " + "Background:" + (bg == null ? "null" : bg.ToString());
            var textFormatted = text.FormatTerminal(fg, bg);
            testOutputHelperWrapper.WriteLine(textFormatted);
        }
    }
    
    [SkippableTheory]
    [MemberData(nameof(ConsoleColorsData))]
    public void ConsoleColorsBackgroundDebug(ConsoleColor? bg)
    {
        TerminalFormat.IsDebugEnabled = true;
        foreach (var fg in Enum.GetValues<ConsoleColor>().Select(o => (ConsoleColor?)o).Prepend(null))
        {
            var text = "Foreground:" + (fg == null ? "null" : fg.ToString()) + " | " + "Background:" + (bg == null ? "null" : bg.ToString());
            var textFormatted = text.FormatTerminal(fg, bg);
            testOutputHelperWrapper.WriteLine(textFormatted);
        }
        TerminalFormat.IsDebugEnabled = false;
    }
}
