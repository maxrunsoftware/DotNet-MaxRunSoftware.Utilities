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

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Provides functions for changing terminal output colors
/// https://github.com/riezebosch/crayon
/// https://stackoverflow.com/questions/4842424/list-of-ansi-color-escape-sequences
/// https://en.wikipedia.org/wiki/ANSI_escape_code#Platform_support
/// https://github.com/termstandard/colors
/// https://www.lihaoyi.com/post/BuildyourownCommandLinewithANSIescapecodes.html
/// https://stackoverflow.com/a/48720492
/// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
/// https://www.ditig.com/256-colors-cheat-sheet
/// https://rich.readthedocs.io/en/stable/appendix/colors.html
/// </summary>
[PublicAPI]
public static class TerminalFormat
{
    public static string FormatTerminal(this string str, TerminalColor? foreground, TerminalColor? background, params TerminalSGR[] sgrs) => FormatTerminalInternal(str, foreground, background, sgrs);

    private static string FormatTerminalInternal(string str, TerminalColor? foreground, TerminalColor? background, TerminalSGR[] sgrs)
    {
        var sb = new StringBuilder();

        // clear existing formatting
        sb.Append(TerminalSGR.Reset.ToAnsi());

        // modifiers like bold, blinking, etc
        foreach (var sgr in sgrs) sb.Append(sgr.ToAnsi());

        // colors
        if (foreground != null) sb.Append(foreground.ToStringAnsiForeground());
        if (background != null) sb.Append(background.ToStringAnsiBackground());

        // actual text
        sb.Append(str);

        // clear future formatting
        sb.Append(TerminalSGR.Reset.ToAnsi());

        return sb.ToString();
    }
}
