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

namespace MaxRunSoftware.Utilities.Common;

public class TerminalColorTextParser
{
    public static TerminalColorTextParser Parse(string colorString) => new(colorString);
        
    public string ColorName { get; }
    public TerminalSGR? Color4_Foreground { get; }
    public TerminalSGR? Color4_Background { get; }
    public byte? Color8 { get; }
    public Color Color24 { get; }

    /// <summary>
    /// <c>
    /// " 34   44    4  rgb(0,0,128)      #000080  blue                   "
    /// </c>
    /// <c>
    /// "           21  rgb(0,0,255)      #0000ff  blue1                  "
    /// </c>
    /// </summary>
    /// <param name="colorString">The color string text to parse</param>
    private TerminalColorTextParser(string colorString)
    {
        var s = new string(colorString.Select(o => char.IsWhiteSpace(o) ? ' ' : o).ToArray());

        foreach (var item in new[] { ("( ", "("), (" )", ")"), (", ", ","), (" ,", ",") })
        {
            while (s.Contains(item.Item1))
            {
                s = s.Replace(item.Item1, item.Item2);
            }
        }

        var rawColorParts = s.Split(' ').Select(o => o.TrimOrNull()).WhereNotNull().ToImmutableArray();
        var parts = new Queue<string>(rawColorParts);

        if (parts.Count.NotIn(4, 5, 6)) throw new ArgumentException($"Invalid {nameof(colorString)} with {rawColorParts.Length} parts " + (rawColorParts.Length == 0 ? "[]" : "[ \"" + rawColorParts.ToStringDelimited("\", \"") + "\" ]") + $" -> {colorString}", nameof(colorString));

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
        ColorName = parts.Dequeue()
            .Select(o => nameChars.Contains(o) ? o : '#')
            .ToStringJoined()
            .Split('#')
            .TrimOrNull()
            .WhereNotNull()
            .Select(o => o.Capitalize())
            .ToStringDelimited("")
            .TrimOrNull()
            .CheckNotNull("ColorName");
    }

}
