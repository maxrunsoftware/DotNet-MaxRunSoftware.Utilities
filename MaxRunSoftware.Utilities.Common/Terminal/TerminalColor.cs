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

using System.Drawing;
// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

public class TerminalColor4 : TerminalColor
{
    public new TerminalSGR Color4_Foreground => base.Color4_Foreground.CheckNotNull();
    public new TerminalSGR Color4_Background => base.Color4_Background.CheckNotNull();
    public new byte Color8 => base.Color8.CheckNotNull();

    public TerminalColor4(string colorString) : base(colorString)
    {
        base.Color4_Foreground.CheckNotNull();
        base.Color4_Background.CheckNotNull();
        base.Color8.CheckNotNull();
    }
}

public partial class TerminalColor
{
    static TerminalColor()
    {
        colors = Lzy.Create(Colors_Build);
        colorsRGB = Lzy.Create(ColorsRGB_Build);
    }
    private static readonly Lzy<ImmutableArray<TerminalColor>> colors;
    public static ImmutableArray<TerminalColor> Colors => colors.Value;
    private static ImmutableArray<TerminalColor> Colors_Build()
    {
        return typeof(TerminalColor)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(o => o.CanRead && !o.CanWrite)
            .Where(o => o.PropertyType == typeof(TerminalColor))
            .Select(o => o.GetValue(null) as TerminalColor)
            .WhereNotNull()
            .OrderBy(o => o.Color8.CheckNotNull())
            .ToImmutableArray();
    }

    private static readonly Lzy<ImmutableDictionary<uint, TerminalColor>> colorsRGB;
    private static ImmutableDictionary<uint, TerminalColor> ColorsRGB_Build()
    {
        var d = new Dictionary<uint, TerminalColor>();
        foreach (var c in Colors)
        {
            var key = ToUInt(c);
            if (!d.ContainsKey(key)) d.Add(key, c);
        }
        return d.ToImmutableDictionary();
    }

    private static uint ToUInt(TerminalColor c) => BitConverter.ToUInt32(new[] { c.Color24.A, c.Color24.R, c.Color24.G, c.Color24.B });
    private static uint ToUInt(Color c) => BitConverter.ToUInt32(new[] { c.A, c.R, c.G, c.B });

    public TerminalSGR? Color4_Foreground { get; }
    public TerminalSGR? Color4_Background { get; }
    public byte? Color8 { get; }
    public Color Color24 { get; }
    public string Name { get; }

    public TerminalColor(Color color24, string? name = null, TerminalSGR? color4Foreground = default, TerminalSGR? color4Background = default, byte? color8 = default)
    {
        var existing = colorsRGB.Value.GetValueNullable(ToUInt(color24));
        Color4_Foreground = color4Foreground ?? existing?.Color4_Foreground;
        Color4_Background = color4Background ?? existing?.Color4_Background;
        Color8 = color8 ?? existing?.Color8;
        Color24 = color24;
        Name = name ?? existing?.Name ?? color24.Name;
    }
    public TerminalColor(string colorString)
    {
        var s = new string(colorString.Select(o => char.IsWhiteSpace(o) ? ' ' : o).ToArray());

        foreach (var item in new[] { ("( ", "("), (" )", ")"), (", ", ","), (" ,", ",") })
        {
            while (s.Contains(item.Item1)) s = s.Replace(item.Item1, item.Item2);
        }

        var rawColorParts = s.Split(' ').Select(o => o.TrimOrNull()).WhereNotNull().ToImmutableArray();
        var parts = new Queue<string>(rawColorParts);

        if (parts.Count.NotIn(4, 5, 6)) throw new ArgumentException($"Invalid {nameof(colorString)} with {rawColorParts.Length} parts " + (rawColorParts.Length == 0 ? "[]" : ("[ \"" + rawColorParts.ToStringDelimited("\", \"") + "\" ]")) + $" -> {colorString}", nameof(colorString));

        if (parts.Count == 6)
        {
            Color4_Foreground = (TerminalSGR)parts.Dequeue().ToByte();
            Color4_Background = (TerminalSGR)parts.Dequeue().ToByte();
        }

        Color8 = parts.Dequeue().ToByte();

        Color24 = parts.Dequeue().ToColor();
        if (parts.Count > 1)
        {
            var color24Other = parts.Dequeue().ToColor();
            if (!Color24.Equals(color24Other)) throw new ArgumentException($"Color RGB does not match Hex -> {Color24.ToCss()}  {color24Other.ToCss()}", nameof(colorString));
        }

        var nameChars = Constant.Chars_Alphanumeric.ToHashSet();
        Name = parts.Dequeue()
            .Select(o => nameChars.Contains(o) ? o : '#')
            .ToStringJoined()
            .Split('#')
            .TrimOrNull()
            .WhereNotNull()
            .Select(o => o.Capitalize())
            .ToStringDelimited("");

    }
}
