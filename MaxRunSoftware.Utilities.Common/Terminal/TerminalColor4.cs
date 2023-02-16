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

using System.Drawing;

namespace MaxRunSoftware.Utilities.Common;

public class TerminalColor4 : TerminalColor8
{
    static TerminalColor4()
    {
        colors4 = Lzy.Create(GetColors<TerminalColor4>);
    }

    public TerminalSGR Color4_Foreground { get; }
    public TerminalSGR Color4_Background { get; }

    public TerminalColor4(string colorString) : base(colorString)
    {
        Color4_Foreground = Text.CheckNotNull(nameof(Text)).Color4_Foreground.CheckNotNull(nameof(Text) + "." + nameof(Text.Color4_Foreground));
        Color4_Background = Text.CheckNotNull(nameof(Text)).Color4_Background.CheckNotNull(nameof(Text) + "." + nameof(Text.Color4_Background));
    }

    public TerminalColor4(Color color, string? colorName, byte color8, TerminalSGR color4foreground, TerminalSGR color4background) : base(color, colorName, color8)
    {
        Color4_Foreground = color4foreground;
        Color4_Background = color4background;
    }

    public static ImmutableArray<TerminalColor4> Colors4 => colors4.Value;
    private static readonly Lzy<ImmutableArray<TerminalColor4>> colors4;

}
