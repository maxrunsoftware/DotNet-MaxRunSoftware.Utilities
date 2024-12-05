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

using System.Drawing;
using System.Runtime.InteropServices;
using CC=System.ConsoleColor;

namespace MaxRunSoftware.Utilities.Common;

public partial class TerminalColor
{
    static TerminalColor()
    {
        if (Environment.GetEnvironmentVariable("MAXRUNSOFTWARE_NOCOLOR") == null) EnableOnWindows();
    }
    
    //private static uint ToUInt(TerminalColor c) => ToUInt(c.Color.A, c.Color.R, c.Color.G, c.Color.B);
    //private static uint ToUInt(Color c) => ToUInt(c.A, c.R, c.G, c.B);
    //private static uint ToUInt(byte red, byte green, byte blue) => ToUInt(255, red, green, blue);
    //private static uint ToUInt(byte alpha, byte red, byte green, byte blue) => BitConverter.ToUInt32(new[] { alpha, red, green, blue });
    
    private TerminalColor(
        byte color8,
        string colorHex,
        string colorName,
        byte? color4Foreground = null,
        byte? color4Background = null,
        ConsoleColor? consoleColor = null)
    {
        var c = ColorTranslator.FromHtml(colorHex);
        if (Constant.Hex_Color.TryGetValue(ColorTranslator.ToHtml(c), out var cc))
        {
            Color = cc;
        }
        else
        {
            Color = c;
        }
        
        Color8 = color8;
        ColorName = colorName;
        if (color4Foreground != null) Color4Foreground = (TerminalSGR)color4Foreground;
        if (color4Background != null) Color4Background = (TerminalSGR)color4Background;
        ConsoleColor = consoleColor;
        ColorHex = ColorTranslator.ToHtml(Color);
        
        if (Constant.Color_KnownColor.TryGetValue(Color, out var knownColor))
        {
            KnownColor = knownColor;
        }
    }
    
    public byte Color8 { get; }
    public string ColorHex { get; }
    public string ColorName { get; }
    public TerminalSGR? Color4Foreground { get; }
    public TerminalSGR? Color4Background { get; }
    public ConsoleColor? ConsoleColor { get; }
    public KnownColor? KnownColor { get; }
    public Color Color { get; }
    
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<ImmutableDictionary<ConsoleColor, TerminalColor>> _ConsoleColor_TerminalColor =
        new(() => Colors.Where(o => o.ConsoleColor != null).ToImmutableDictionary(o => o.ConsoleColor!.Value, o => o));
    // ReSharper disable once InconsistentNaming
    public static ImmutableDictionary<ConsoleColor, TerminalColor> ConsoleColor_TerminalColor => _ConsoleColor_TerminalColor.Value;
    public static TerminalColor? Get(ConsoleColor color) => ConsoleColor_TerminalColor.GetValueOrDefault(color);
    
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<ImmutableDictionary<KnownColor, TerminalColor>> _KnownColor_TerminalColor =
        new(() => Colors.Where(o => o.KnownColor != null).ToImmutableDictionary(o => o.KnownColor!.Value, o => o));
    // ReSharper disable once InconsistentNaming
    public static ImmutableDictionary<KnownColor, TerminalColor> KnownColor_TerminalColor => _KnownColor_TerminalColor.Value;
    public static TerminalColor? Get(KnownColor color) => KnownColor_TerminalColor.GetValueOrDefault(color);
    
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<ImmutableDictionary<Color, TerminalColor>> _Color_TerminalColor = new(() =>
    {
        var b = ImmutableDictionary.CreateBuilder<Color, TerminalColor>(Constant.ColorEqualityComparer);
        foreach (var terminalColor in Colors) b.TryAdd(terminalColor.Color, terminalColor);
        return b.ToImmutable();
    });
    
    // ReSharper disable once InconsistentNaming
    public static ImmutableDictionary<Color, TerminalColor> Color_TerminalColor => _Color_TerminalColor.Value;
    public static TerminalColor? Get(Color color) => Color_TerminalColor.GetValueOrDefault(color);
}

public partial class TerminalColor
{
    public static bool EnableOnWindows() => ColorsOnWindows.Enable();

    /// <summary>
    /// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    /// </summary>
    private static class ColorsOnWindows
    {
        private static readonly object locker = new();
        public static bool Enable()
        {
            lock (locker)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;

                var iStdOut = GetStdHandle(StdOutputHandle);
                return GetConsoleMode(iStdOut, out var outConsoleMode) &&
                       SetConsoleMode(iStdOut, outConsoleMode | EnableVirtualTerminalProcessing);
            }
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
}

public partial class TerminalColor
{
    public static ImmutableArray<TerminalColor> Colors { get; } =
    [
        new(0, "#000000", "black", 30, 40, CC.Black),
        new(1, "#800000", "red", 31, 41, CC.DarkRed),
        new(2, "#008000", "green", 32, 42, CC.DarkGreen),
        new(3, "#808000", "yellow", 33, 43, CC.DarkYellow),
        new(4, "#000080", "blue", 34, 44, CC.DarkBlue),
        new(5, "#800080", "magenta", 35, 45, CC.DarkMagenta),
        new(6, "#008080", "cyan", 36, 46, CC.DarkCyan),
        new(7, "#c0c0c0", "white", 37, 47, CC.DarkGray),
        new(8, "#808080", "bright_black", 90, 100, CC.Gray),
        new(9, "#ff0000", "bright_red  ", 91, 101, CC.Red),
        new(10, "#00ff00", "bright_green", 92, 102, CC.Green),
        new(11, "#ffff00", "bright_yellow", 93, 103, CC.Yellow),
        new(12, "#0000ff", "bright_blue ", 94, 104, CC.Blue),
        new(13, "#ff00ff", "bright_magenta", 95, 105, CC.Magenta),
        new(14, "#00ffff", "bright_cyan ", 96, 106, CC.Cyan),
        new(15, "#ffffff", "bright_white", 97, 107, CC.White),
        new(16, "#000000", "grey0"),
        new(17, "#00005f", "navy_blue"),
        new(18, "#000087", "dark_blue"),
        new(20, "#0000d7", "blue3"),
        new(21, "#0000ff", "blue1"),
        new(22, "#005f00", "dark_green"),
        new(25, "#005faf", "deep_sky_blue4"),
        new(26, "#005fd7", "dodger_blue3"),
        new(27, "#005fff", "dodger_blue2"),
        new(28, "#008700", "green4"),
        new(29, "#00875f", "spring_green4"),
        new(30, "#008787", "turquoise4"),
        new(32, "#0087d7", "deep_sky_blue3"),
        new(33, "#0087ff", "dodger_blue1"),
        new(36, "#00af87", "dark_cyan"),
        new(37, "#00afaf", "light_sea_green"),
        new(38, "#00afd7", "deep_sky_blue2"),
        new(39, "#00afff", "deep_sky_blue1"),
        new(40, "#00d700", "green3"),
        new(41, "#00d75f", "spring_green3"),
        new(43, "#00d7af", "cyan3"),
        new(44, "#00d7d7", "dark_turquoise"),
        new(45, "#00d7ff", "turquoise2"),
        new(46, "#00ff00", "green1"),
        new(47, "#00ff5f", "spring_green2"),
        new(48, "#00ff87", "spring_green1"),
        new(49, "#00ffaf", "medium_spring_green"),
        new(50, "#00ffd7", "cyan2"),
        new(51, "#00ffff", "cyan1"),
        new(55, "#5f00af", "purple4"),
        new(56, "#5f00d7", "purple3"),
        new(57, "#5f00ff", "blue_violet"),
        new(59, "#5f5f5f", "grey37"),
        new(60, "#5f5f87", "medium_purple4"),
        new(62, "#5f5fd7", "slate_blue3"),
        new(63, "#5f5fff", "royal_blue1"),
        new(64, "#5f8700", "chartreuse4"),
        new(66, "#5f8787", "pale_turquoise4"),
        new(67, "#5f87af", "steel_blue"),
        new(68, "#5f87d7", "steel_blue3"),
        new(69, "#5f87ff", "cornflower_blue"),
        new(71, "#5faf5f", "dark_sea_green4"),
        new(73, "#5fafaf", "cadet_blue"),
        new(74, "#5fafd7", "sky_blue3"),
        new(76, "#5fd700", "chartreuse3"),
        new(78, "#5fd787", "sea_green3"),
        new(79, "#5fd7af", "aquamarine3"),
        new(80, "#5fd7d7", "medium_turquoise"),
        new(81, "#5fd7ff", "steel_blue1"),
        new(83, "#5fff5f", "sea_green2"),
        new(85, "#5fffaf", "sea_green1"),
        new(87, "#5fffff", "dark_slate_gray2"),
        new(88, "#870000", "dark_red"),
        new(91, "#8700af", "dark_magenta"),
        new(94, "#875f00", "orange4"),
        new(95, "#875f5f", "light_pink4"),
        new(96, "#875f87", "plum4"),
        new(98, "#875fd7", "medium_purple3"),
        new(99, "#875fff", "slate_blue1"),
        new(101, "#87875f", "wheat4"),
        new(102, "#878787", "grey53"),
        new(103, "#8787af", "light_slate_grey"),
        new(104, "#8787d7", "medium_purple"),
        new(105, "#8787ff", "light_slate_blue"),
        new(106, "#87af00", "yellow4"),
        new(108, "#87af87", "dark_sea_green"),
        new(110, "#87afd7", "light_sky_blue3"),
        new(111, "#87afff", "sky_blue2"),
        new(112, "#87d700", "chartreuse2"),
        new(114, "#87d787", "pale_green3"),
        new(116, "#87d7d7", "dark_slate_gray3"),
        new(117, "#87d7ff", "sky_blue1"),
        new(118, "#87ff00", "chartreuse1"),
        new(120, "#87ff87", "light_green"),
        new(122, "#87ffd7", "aquamarine1"),
        new(123, "#87ffff", "dark_slate_gray1"),
        new(125, "#af005f", "deep_pink4"),
        new(126, "#af0087", "medium_violet_red"),
        new(128, "#af00d7", "dark_violet"),
        new(129, "#af00ff", "purple"),
        new(133, "#af5faf", "medium_orchid3"),
        new(134, "#af5fd7", "medium_orchid"),
        new(136, "#af8700", "dark_goldenrod"),
        new(138, "#af8787", "rosy_brown"),
        new(139, "#af87af", "grey63"),
        new(140, "#af87d7", "medium_purple2"),
        new(141, "#af87ff", "medium_purple1"),
        new(143, "#afaf5f", "dark_khaki"),
        new(144, "#afaf87", "navajo_white3"),
        new(145, "#afafaf", "grey69"),
        new(146, "#afafd7", "light_steel_blue3"),
        new(147, "#afafff", "light_steel_blue"),
        new(149, "#afd75f", "dark_olive_green3"),
        new(150, "#afd787", "dark_sea_green3"),
        new(152, "#afd7d7", "light_cyan3"),
        new(153, "#afd7ff", "light_sky_blue1"),
        new(154, "#afff00", "green_yellow"),
        new(155, "#afff5f", "dark_olive_green2"),
        new(156, "#afff87", "pale_green1"),
        new(157, "#afffaf", "dark_sea_green2"),
        new(159, "#afffff", "pale_turquoise1"),
        new(160, "#d70000", "red3"),
        new(162, "#d70087", "deep_pink3"),
        new(164, "#d700d7", "magenta3"),
        new(166, "#d75f00", "dark_orange3"),
        new(167, "#d75f5f", "indian_red"),
        new(168, "#d75f87", "hot_pink3"),
        new(169, "#d75faf", "hot_pink2"),
        new(170, "#d75fd7", "orchid"),
        new(172, "#d78700", "orange3"),
        new(173, "#d7875f", "light_salmon3"),
        new(174, "#d78787", "light_pink3"),
        new(175, "#d787af", "pink3"),
        new(176, "#d787d7", "plum3"),
        new(177, "#d787ff", "violet"),
        new(178, "#d7af00", "gold3"),
        new(179, "#d7af5f", "light_goldenrod3"),
        new(180, "#d7af87", "tan"),
        new(181, "#d7afaf", "misty_rose3"),
        new(182, "#d7afd7", "thistle3"),
        new(183, "#d7afff", "plum2"),
        new(184, "#d7d700", "yellow3"),
        new(185, "#d7d75f", "khaki3"),
        new(187, "#d7d7af", "light_yellow3"),
        new(188, "#d7d7d7", "grey84"),
        new(189, "#d7d7ff", "light_steel_blue1"),
        new(190, "#d7ff00", "yellow2"),
        new(192, "#d7ff87", "dark_olive_green1"),
        new(193, "#d7ffaf", "dark_sea_green1"),
        new(194, "#d7ffd7", "honeydew2"),
        new(195, "#d7ffff", "light_cyan1"),
        new(196, "#ff0000", "red1"),
        new(197, "#ff005f", "deep_pink2"),
        new(199, "#ff00af", "deep_pink1"),
        new(200, "#ff00d7", "magenta2"),
        new(201, "#ff00ff", "magenta1"),
        new(202, "#ff5f00", "orange_red1"),
        new(204, "#ff5f87", "indian_red1"),
        new(206, "#ff5fd7", "hot_pink"),
        new(207, "#ff5fff", "medium_orchid1"),
        new(208, "#ff8700", "dark_orange"),
        new(209, "#ff875f", "salmon1"),
        new(210, "#ff8787", "light_coral"),
        new(211, "#ff87af", "pale_violet_red1"),
        new(212, "#ff87d7", "orchid2"),
        new(213, "#ff87ff", "orchid1"),
        new(214, "#ffaf00", "orange1"),
        new(215, "#ffaf5f", "sandy_brown"),
        new(216, "#ffaf87", "light_salmon1"),
        new(217, "#ffafaf", "light_pink1"),
        new(218, "#ffafd7", "pink1"),
        new(219, "#ffafff", "plum1"),
        new(220, "#ffd700", "gold1"),
        new(222, "#ffd787", "light_goldenrod2"),
        new(223, "#ffd7af", "navajo_white1"),
        new(224, "#ffd7d7", "misty_rose1"),
        new(225, "#ffd7ff", "thistle1"),
        new(226, "#ffff00", "yellow1"),
        new(227, "#ffff5f", "light_goldenrod1"),
        new(228, "#ffff87", "khaki1"),
        new(229, "#ffffaf", "wheat1"),
        new(230, "#ffffd7", "cornsilk1"),
        new(231, "#ffffff", "grey100"),
        new(232, "#080808", "grey3"),
        new(233, "#121212", "grey7"),
        new(234, "#1c1c1c", "grey11"),
        new(235, "#262626", "grey15"),
        new(236, "#303030", "grey19"),
        new(237, "#3a3a3a", "grey23"),
        new(238, "#444444", "grey27"),
        new(239, "#4e4e4e", "grey30"),
        new(240, "#585858", "grey35"),
        new(241, "#626262", "grey39"),
        new(242, "#6c6c6c", "grey42"),
        new(243, "#767676", "grey46"),
        new(244, "#808080", "grey50"),
        new(245, "#8a8a8a", "grey54"),
        new(246, "#949494", "grey58"),
        new(247, "#9e9e9e", "grey62"),
        new(248, "#a8a8a8", "grey66"),
        new(249, "#b2b2b2", "grey70"),
        new(250, "#bcbcbc", "grey74"),
        new(251, "#c6c6c6", "grey78"),
        new(252, "#d0d0d0", "grey82"),
        new(253, "#dadada", "grey85"),
        new(254, "#e4e4e4", "grey89"),
        new(255, "#eeeeee", "grey93"),
    ];
}
