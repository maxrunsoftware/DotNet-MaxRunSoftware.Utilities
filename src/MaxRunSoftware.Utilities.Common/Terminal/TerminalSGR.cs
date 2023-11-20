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

// ReSharper disable InconsistentNaming
// ReSharper disable CommentTypo

using System.Drawing;

namespace MaxRunSoftware.Utilities.Common;

[PublicAPI]
public enum TerminalSGR : byte
{
    /// <summary>
    /// <p>All attributes off</p>
    /// </summary>
    Reset = 0,

    /// <summary>
    /// <p>Bold or increased intensity</p>
    /// <p>As with faint, the color change is a PC (SCO / CGA) invention.</p>
    /// </summary>
    Intensity_Bold = 1,

    /// <summary>
    /// <p>Faint, decreased intensity, or dim</p>
    /// <p>May be implemented as a light font weight like bold.</p>
    /// </summary>
    Intensity_Faint = 2,

    /// <summary>
    /// <p>Not widely supported.</p>
    /// <p>Sometimes treated as inverse or blink.</p>
    /// </summary>
    Italic = 3,

    /// <summary>
    /// <p>Style extensions exist for Kitty, VTE, mintty and iTerm2.</p>
    /// </summary>
    Underline = 4,

    /// <summary>
    /// <p>Sets blinking to less than 150 times per minute</p>
    /// </summary>
    Blink_Slow = 5,

    /// <summary>
    /// <p>MS-DOS ANSI.SYS, 150+ per minute</p>
    /// <p>Not widely supported.</p>
    /// </summary>
    Blink_Fast = 6,

    /// <summary>
    /// <p>Reverse video or invert</p>
    /// <p>Swap foreground and background colors</p>
    /// <p>Inconsistent emulation</p>
    /// </summary>
    SwapForegroundBackground = 7,

    /// <summary>
    /// <p>Conceal or hide</p>
    /// <p>Not widely supported.</p>
    /// </summary>
    Conceal = 8,

    /// <summary>
    /// <p>Crossed-out, or strike</p>
    /// <p>Characters legible but marked as if for deletion.</p>
    /// <p>Not supported in Terminal.app</p>
    /// </summary>
    CrossOut = 9,

    /// <summary>
    /// <p>Primary (default) font</p>
    /// </summary>
    Font_Primary = 10,

    /// <summary>
    /// <p>Alternative font 1</p>
    /// </summary>
    Font_Alternative_1 = 11,

    /// <summary>
    /// <p>Alternative font 2</p>
    /// </summary>
    Font_Alternative_2 = 12,

    /// <summary>
    /// <p>Alternative font 3</p>
    /// </summary>
    Font_Alternative_3 = 13,

    /// <summary>
    /// <p>Alternative font 4</p>
    /// </summary>
    Font_Alternative_4 = 14,

    /// <summary>
    /// <p>Alternative font 5</p>
    /// </summary>
    Font_Alternative_5 = 15,

    /// <summary>
    /// <p>Alternative font 6</p>
    /// </summary>
    Font_Alternative_6 = 16,

    /// <summary>
    /// <p>Alternative font 7</p>
    /// </summary>
    Font_Alternative_7 = 17,

    /// <summary>
    /// <p>Alternative font 8</p>
    /// </summary>
    Font_Alternative_8 = 18,

    /// <summary>
    /// <p>Alternative font 9</p>
    /// </summary>
    Font_Alternative_9 = 19,

    /// <summary>
    /// <p>Fraktur (Gothic)</p>
    /// <p>Rarely supported</p>
    /// </summary>
    Gothic = 20,

    /// <summary>
    /// <p>Doubly underlined or not bold</p>
    /// <p>Double-underline per ECMA-48:8.3.117, but instead disables bold intensity on several terminals, including in the Linux kernel's console before version 4.17.</p>
    /// </summary>
    Underline_Double = 21,

    /// <summary>
    /// <p>Normal intensity</p>
    /// <p>Neither bold nor faint</p>
    /// <p>color changes where intensity is implemented as such.</p>
    /// </summary>
    Intensity_Normal = 22,

    /// <summary>
    /// <p>Neither italic, nor blackletter</p>
    /// </summary>
    Italic_Off = 23,

    /// <summary>
    /// <p>Neither singly nor doubly underlined</p>
    /// </summary>
    Underline_Off = 24,

    /// <summary>
    /// <p>Turn blinking off</p>
    /// </summary>
    Blinking_Off = 25,

    /// <summary>
    /// <p>Proportional spacing</p>
    /// <p>ITU T.61 and T.416, not known to be used on terminals</p>
    /// </summary>
    ProportionalSpacing = 26,

    /// <summary>
    /// <p>Not reversed</p>
    /// </summary>
    Reverse_Off = 27,

    /// <summary>
    /// <p>Reveal, not concealed</p>
    /// </summary>
    Conceal_Off = 28,

    /// <summary>
    /// <p>Not crossed out</p>
    /// </summary>
    CrossOut_Off = 29,

    #region Color_Foreground_{color}

    /// <summary>
    /// <p>Set foreground color to Black</p>
    /// </summary>
    Color_Foreground_Black = 30,

    /// <summary>
    /// <p>Set foreground color to Red</p>
    /// </summary>
    Color_Foreground_Red = 31,

    /// <summary>
    /// <p>Set foreground color to Green</p>
    /// </summary>
    Color_Foreground_Green = 32,

    /// <summary>
    /// <p>Set foreground color to Yellow</p>
    /// </summary>
    Color_Foreground_Yellow = 33,

    /// <summary>
    /// <p>Set foreground color to Blue</p>
    /// </summary>
    Color_Foreground_Blue = 34,

    /// <summary>
    /// <p>Set foreground color to Magenta</p>
    /// </summary>
    Color_Foreground_Magenta = 35,

    /// <summary>
    /// <p>Set foreground color to Cyan</p>
    /// </summary>
    Color_Foreground_Cyan = 36,

    /// <summary>
    /// <p>Set foreground color to White</p>
    /// </summary>
    Color_Foreground_White = 37,

    /// <summary>
    /// <p>Set foreground color</p>
    /// <p>Next arguments are 5;n or 2;r;g;b</p>
    /// </summary>
    Color_Foreground = 38,

    /// <summary>
    /// <p>Default foreground color</p>
    /// <p>Implementation defined (according to standard)</p>
    /// </summary>
    Color_Foreground_Default = 39,

    #endregion Color_Foreground_{color}

    #region Color_Background_{color}

    /// <summary>
    /// <p>Set background color to Black</p>
    /// </summary>
    Color_Background_Black = 40,

    /// <summary>
    /// <p>Set background color to Red</p>
    /// </summary>
    Color_Background_Red = 41,

    /// <summary>
    /// <p>Set background color to Green</p>
    /// </summary>
    Color_Background_Green = 42,

    /// <summary>
    /// <p>Set background color to Yellow</p>
    /// </summary>
    Color_Background_Yellow = 43,

    /// <summary>
    /// <p>Set background color to Blue</p>
    /// </summary>
    Color_Background_Blue = 44,

    /// <summary>
    /// <p>Set background color to Magenta</p>
    /// </summary>
    Color_Background_Magenta = 45,

    /// <summary>
    /// <p>Set background color to Cyan</p>
    /// </summary>
    Color_Background_Cyan = 46,

    /// <summary>
    /// <p>Set background color to White</p>
    /// </summary>
    Color_Background_White = 47,

    /// <summary>
    /// <p>Set background color</p>
    /// <p>Next arguments are 5;n or 2;r;g;b</p>
    /// </summary>
    Color_Background = 48,

    /// <summary>
    /// <p>Implementation defined (according to standard)</p>
    /// </summary>
    Color_Background_Default = 49,

    #endregion Color_Background_{color}

    /// <summary>
    /// <p>Disable proportional spacing T.61 and T.416</p>
    /// </summary>
    ProportionalSpacing_Off = 50,

    /// <summary>
    /// <p>Implemented as "emoji variation selector" in mintty.</p>
    /// </summary>
    Frame = 51,

    /// <summary>
    /// <p>Encircled</p>
    /// </summary>
    Encircle = 52,

    /// <summary>
    /// <p>Not supported in Terminal.app</p>
    /// </summary>
    Overline = 53,

    /// <summary>
    /// <p>Neither framed nor encircled</p>
    /// </summary>
    Encircled_Off = 54,

    /// <summary>
    /// <p>Not overlined</p>
    /// </summary>
    Overline_Off = 55,

    /// <summary>
    /// <p>Set underline color</p>
    /// <p>Not in standard</p>
    /// <p>implemented in Kitty, VTE, mintty, and iTerm2.</p>
    /// <p>Next arguments are 5;n or 2;r;g;b</p>
    /// </summary>
    Color_Underline = 58,

    /// <summary>
    /// <p>Default underline color</p>
    /// <p>Not in standard</p>
    /// <p>implemented in Kitty, VTE, mintty, and iTerm2.</p>
    /// </summary>
    Color_Underline_Default = 59,

    #region Ideogram

    /// <summary>
    /// <p>Ideogram underline or right side line</p>
    /// <p>Rarely supported</p>
    /// </summary>
    Ideogram_Underline = 60,

    /// <summary>
    /// <p>Ideogram double underline, or double line on the right side</p>
    /// </summary>
    Ideogram_DoubleUnderline = 61,

    /// <summary>
    /// <p>Ideogram overline or left side line</p>
    /// </summary>
    Ideogram_Overline = 62,

    /// <summary>
    /// <p>Ideogram double overline, or double line on the left side</p>
    /// </summary>
    Ideogram_DoubleOverline = 63,

    /// <summary>
    /// <p>Ideogram stress marking</p>
    /// </summary>
    Ideogram_StressMarking = 64,

    /// <summary>
    /// <p>No ideogram attributes</p>
    /// <p>Reset the effects of all of 60–64</p>
    /// </summary>
    Ideogram_Off = 65,

    #endregion Ideogram

    /// <summary>
    /// <p>Implemented only in mintty</p>
    /// </summary>
    Superscript = 73,

    /// <summary>
    /// <p>Subscript</p>
    /// </summary>
    Subscript = 74,

    /// <summary>
    /// <p>Neither superscript nor subscript</p>
    /// </summary>
    SuperscriptSubscript_Off = 75,

    #region Color_Foreground_{color}_Bright

    /// <summary>
    /// <p>Set foreground color to bright Black</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Black_Bright = 90,

    /// <summary>
    /// <p>Set foreground color to bright Red</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Red_Bright = 91,

    /// <summary>
    /// <p>Set foreground color to bright Green</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Green_Bright = 92,

    /// <summary>
    /// <p>Set foreground color to bright Yellow</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Yellow_Bright = 93,

    /// <summary>
    /// <p>Set foreground color to bright Blue</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Blue_Bright = 94,

    /// <summary>
    /// <p>Set foreground color to bright Magenta</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Magenta_Bright = 95,

    /// <summary>
    /// <p>Set foreground color to bright Cyan</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_Cyan_Bright = 96,

    /// <summary>
    /// <p>Set foreground color to bright White</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Foreground_White_Bright = 97,

    #endregion Color_Foreground_{color}_Bright

    #region Color_Background_{color}_Bright

    /// <summary>
    /// <p>Set background color to bright Black</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Black_Bright = 100,

    /// <summary>
    /// <p>Set background color to bright Red</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Red_Bright = 101,

    /// <summary>
    /// <p>Set background color to bright Green</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Green_Bright = 102,

    /// <summary>
    /// <p>Set background color to bright Yellow</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Yellow_Bright = 103,

    /// <summary>
    /// <p>Set background color to bright Blue</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Blue_Bright = 104,

    /// <summary>
    /// <p>Set background color to bright Magenta</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Magenta_Bright = 105,

    /// <summary>
    /// <p>Set background color to bright Cyan</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_Cyan_Bright = 106,

    /// <summary>
    /// <p>Set background color to bright White</p>
    /// <p>Not in standard</p>
    /// <p>Originally implemented by aixterm</p>
    /// </summary>
    Color_Background_White_Bright = 107,

    #endregion Color_Background_{color}_Bright
}

public static class TerminalSGRExtensions
{
    private static TerminalSGR[] Range(params (int MinInclusive, int MaxInclusive)[] ranges)
    {
        var list = new List<int>();
        foreach (var range in ranges)
        {
            for (var i = range.MinInclusive; i <= range.MaxInclusive; i++)
            {
                list.Add(i);
            }
        }

        return ToSGR(list.ToArray());
    }

    private static TerminalSGR[] ToSGR(params int[] codes) => codes.Select(o => (byte)o).Select(o => (TerminalSGR)o).ToArray();

    private static readonly ImmutableHashSet<TerminalSGR> isColor = Range((30, 37), (40, 47), (90, 97), (100, 107)).ToImmutableHashSet();
    public static bool IsColor(this TerminalSGR sgr) => isColor.Contains(sgr);

    private static readonly ImmutableHashSet<TerminalSGR> isColorBright = Range((90, 97), (100, 107)).ToImmutableHashSet();
    public static bool IsColorBright(this TerminalSGR sgr) => isColorBright.Contains(sgr);

    private static readonly ImmutableHashSet<TerminalSGR> requiresColorParameters = ToSGR(38, 48, 58).ToImmutableHashSet();
    public static bool RequiresColorParameters(this TerminalSGR sgr) => requiresColorParameters.Contains(sgr);

    public static string ToAnsi(this TerminalSGR sgr) => sgr.RequiresColorParameters()
        ? throw new ArgumentException($"{nameof(TerminalSGR)}.{Enum.GetName(sgr)}={(int)sgr} requires color parameters")
        : ToAnsiInternal(sgr);

    public static string ToAnsi(this TerminalSGR sgr, byte color) => sgr.RequiresColorParameters()
        ? ToAnsiInternal(sgr, 5, color)
        : throw new ArgumentException($"{nameof(TerminalSGR)}.{Enum.GetName(sgr)}={(int)sgr} does not support color parameters");

    public static string ToAnsi(this TerminalSGR sgr, byte red, byte green, byte blue) => sgr.RequiresColorParameters()
        ? ToAnsiInternal(sgr, 2, red, green, blue)
        : throw new ArgumentException($"{nameof(TerminalSGR)}.{Enum.GetName(sgr)}={(int)sgr} does not support color parameters");

    public static string ToAnsi(this TerminalSGR sgr, Color color) => sgr.ToAnsi(color.R, color.G, color.B);

    private static string ToAnsiInternal(this TerminalSGR sgr, params byte[] values) =>
        "\u001b[" + (byte)sgr + (values.Length == 0 ? string.Empty : ";" + string.Join(";", values)) + "m";

    public static string ToAnsi(this TerminalSGR[] sgrs) => sgrs.OrEmpty().Select(ToAnsi).ToStringDelimited("");
}
