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

using System.Drawing;

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
/// https://en.wikipedia.org/wiki/ANSI_escape_code#3-bit_and_4-bit
/// </summary>
[PublicAPI]
public static class TerminalFormat
{
    public static bool IsDebugEnabled
    {
        get => TerminalSGRExtensions.IsDebugEnabled;
        set => TerminalSGRExtensions.IsDebugEnabled = value;
    }
    
    private static ImmutableDictionary<KnownColor, Color> SystemDrawingKnownColors { get; } =
        Enum.GetValues<KnownColor>()
            .Select(o => (k: o, v: Color.FromKnownColor(o)))
            .ToImmutableDictionary(o=> o.k, o=> o.v);

    public static string FormatTerminal(this string? str, ConsoleColor? foreground, ConsoleColor? background = null, TerminalSGR[]? sgrs = null) =>
        FormatTerminal(
            str,
            foreground == null ? null : TerminalColor.Get(foreground.Value),
            background == null ? null : TerminalColor.Get(background.Value),
            sgrs
        );

    public static string FormatTerminal(this string? str, TerminalColor? foreground, TerminalColor? background = null, TerminalSGR[]? sgrs = null) => 
        FormatTerminal(
            str,
            foreground?.Color,
            background?.Color,
            sgrs
        );
    
    public static string FormatTerminal(this string? str, KnownColor? foreground, KnownColor? background = null, TerminalSGR[]? sgrs = null) =>
        FormatTerminal(
            str,
            foreground == null ? null : SystemDrawingKnownColors[foreground.Value],
            background == null ? null : SystemDrawingKnownColors[background.Value],
            sgrs
        );
    
    public static string FormatTerminal(this string? str, Color? foreground, Color? background = null, TerminalSGR[]? sgrs = null)
    {
        var sb = new StringBuilder();

        // clear existing formatting
        sb.Append(TerminalSGR.Reset.ToAnsi());

        // modifiers like bold, blinking, etc
        foreach (var sgr in sgrs.OrEmpty()) sb.Append(sgr.ToAnsi());

        // foreground color
        if (foreground != null)
        {
            var tc = TerminalColor.Get(foreground.Value);
            if (tc != null)
            {
                if (tc.Color4Foreground != null)
                {
                    sb.Append(tc.Color4Foreground.Value.ToAnsi());
                }
                else
                {
                    sb.Append(TerminalSGR.Color_Foreground.ToAnsi(tc.Color8));
                }
            }
            else
            {
                sb.Append(TerminalSGR.Color_Foreground.ToAnsi(foreground.Value));
            }
        }
        
        // background color
        if (background != null)
        {
            var tc = TerminalColor.Get(background.Value);
            if (tc != null)
            {
                if (tc.Color4Background != null)
                {
                    sb.Append(tc.Color4Background.Value.ToAnsi());
                }
                else
                {
                    sb.Append(TerminalSGR.Color_Background.ToAnsi(tc.Color8));
                }
            }
            else
            {
                sb.Append(TerminalSGR.Color_Background.ToAnsi(background.Value));
            }
        }

        // actual text
        sb.Append(str);

        // clear future formatting
        sb.Append(TerminalSGR.Reset.ToAnsi());

        return sb.ToString();
    }
}
