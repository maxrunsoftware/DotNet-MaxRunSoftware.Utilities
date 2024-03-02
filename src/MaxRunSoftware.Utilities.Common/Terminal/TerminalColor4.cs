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

// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

public class TerminalColor4 : TerminalColor8
{
    static TerminalColor4() => colors4 = Lzy.Create(GetColors<TerminalColor4>);

    public TerminalSGR Color4_Foreground { get; }
    public TerminalSGR Color4_Background { get; }
    public ConsoleColor ConsoleColor { get; }

    public TerminalColor4(byte color8, byte red, byte green, byte blue, string? colorHex, string colorName, byte color4foreground, byte color4background, ConsoleColor consoleColor) :
        this(color8, red, green, blue, colorHex, colorName, (TerminalSGR)color4foreground, (TerminalSGR)color4background, consoleColor) { }

    public TerminalColor4(byte color8, byte red, byte green, byte blue, string? colorHex, string colorName, TerminalSGR color4foreground, TerminalSGR color4background, ConsoleColor consoleColor) : base(color8, red, green, blue, colorHex, colorName)
    {
        Color4_Foreground = color4foreground;
        Color4_Background = color4background;
        ConsoleColor = consoleColor;
    }


    public override string ToStringAnsiForeground() => Color4_Foreground.ToAnsi();

    public override string ToStringAnsiBackground() => Color4_Background.ToAnsi();

    public static ImmutableArray<TerminalColor4> Colors4 => colors4.Value;
    private static readonly Lzy<ImmutableArray<TerminalColor4>> colors4;
}
