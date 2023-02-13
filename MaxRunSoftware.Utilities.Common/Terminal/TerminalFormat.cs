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

// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

using System.Drawing;
using System.Runtime.InteropServices;

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
    private static readonly object locker = new();

    static TerminalFormat()
    {
        if (Environment.GetEnvironmentVariable("MAXRUNSOFTWARE_NOCOLOR") == null) EnableOnWindows();

        mapColor4 = Lzy.Create(MapColor4Create);
    }

    public static bool EnableOnWindows()
    {
        lock (locker)
        {
            return ColorsOnWindows.Enable();
        }
    }

    public static ImmutableDictionary<Color, (TerminalColor4 Color, TerminalColor4 ColorBright)> Color_TerminalColor4 => mapColor4.Value;
    private static readonly Lzy<ImmutableDictionary<Color, (TerminalColor4 Color, TerminalColor4 ColorBright)>> mapColor4;
    private static ImmutableDictionary<Color, (TerminalColor4 Color, TerminalColor4 ColorBright)> MapColor4Create()
    {
        return new Dictionary<Color, (TerminalColor4 Color, TerminalColor4 ColorBright)>
        {
            { Color.Black, (TerminalColor.Black, TerminalColor.BrightBlack) },
            { Color.Red, (TerminalColor.Red, TerminalColor.BrightRed) },
            { Color.Green, (TerminalColor.Green, TerminalColor.BrightGreen) },
            { Color.Yellow, (TerminalColor.Yellow, TerminalColor.BrightYellow) },
            { Color.Blue, (TerminalColor.Blue, TerminalColor.BrightBlue) },
            { Color.Magenta, (TerminalColor.Magenta, TerminalColor.BrightMagenta) },
            { Color.Cyan, (TerminalColor.Cyan, TerminalColor.BrightCyan) },
            { Color.White, (TerminalColor.White, TerminalColor.BrightWhite) },
        }.ToImmutableDictionary();
    }

    /// <summary>
    /// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    /// </summary>
    private static class ColorsOnWindows
    {
        public static bool Enable()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;

            var iStdOut = GetStdHandle(StdOutputHandle);
            return GetConsoleMode(iStdOut, out var outConsoleMode) &&
                   SetConsoleMode(iStdOut, outConsoleMode | EnableVirtualTerminalProcessing);
        }

        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        //[DllImport("kernel32.dll")]
        //public static extern uint GetLastError();
    }

    #region FormatTerminal4

    public static string FormatTerminal4(this string str, TerminalColor? foreground, TerminalColor? background, params TerminalSGR[] sgrs)
    {
        if (foreground != null && foreground.Color4_Foreground == null) throw new ArgumentException($"{foreground.GetType().NameFormatted()}.{nameof(foreground.Color4_Foreground)} does not contain a value", nameof(foreground));
        if (background != null && background.Color4_Background == null) throw new ArgumentException($"{background.GetType().NameFormatted()}.{nameof(background.Color4_Background)} does not contain a value", nameof(background));

        return FormatTerminal4(str, foreground?.Color4_Foreground, background?.Color4_Background, sgrs);
    }

    public static string FormatTerminal4(this string str, TerminalSGR? foreground, TerminalSGR? background, params TerminalSGR[] sgrs) =>
        FormatTerminalInternal(str, foreground?.ToAnsi(), background?.ToAnsi(), sgrs);

    #endregion FormatTerminal4

    #region FormatTerminal8

    public static string FormatTerminal8(this string str, TerminalColor? foreground, TerminalColor? background, params TerminalSGR[] sgrs) =>
        FormatTerminal8(str, foreground?.Color8, background?.Color8, sgrs);

    public static string FormatTerminal8(this string str, byte? foreground, byte? background, params TerminalSGR[] sgrs) =>
        FormatTerminalInternal(str, foreground == null ? null : TerminalSGR.Color_Foreground.ToAnsi(foreground.Value), background == null ? null : TerminalSGR.Color_Background.ToAnsi(background.Value), sgrs);

    #endregion FormatTerminal8

    #region FormatTerminal24

    public static string FormatTerminal24(this string str, TerminalColor? foreground, TerminalColor? background, params TerminalSGR[] sgrs) =>
        FormatTerminal24(str, foreground?.Color24, background?.Color24, sgrs);

    public static string FormatTerminal24(this string str, Color? foreground, Color? background, params TerminalSGR[] sgrs) =>
        FormatTerminalInternal(str, foreground == null ? null : TerminalSGR.Color_Foreground.ToAnsi(foreground.Value.R, foreground.Value.G, foreground.Value.B), background == null ? null : TerminalSGR.Color_Background.ToAnsi(background.Value.R, background.Value.G, background.Value.B), sgrs);

    #endregion FormatTerminal24

    private static string FormatTerminalInternal(string str, string? foreground, string? background, TerminalSGR[] sgrs)
    {
        var sb = new StringBuilder();
        sb.Append(TerminalSGR.Reset.ToAnsi());
        foreach (var sgr in sgrs)
        {
            sb.Append(sgr.ToAnsi());
        }

        if (foreground != null) sb.Append(foreground);
        if (background != null) sb.Append(background);
        sb.Append(str);
        sb.Append(TerminalSGR.Reset.ToAnsi());
        return sb.ToString();
    }

}
