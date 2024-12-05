// Copyright (c) 2024 Max Run Software (dev@maxrunsoftware.com)
// 
// Licensed under the Apache License, Version 2.0 (the "License")
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

public static class TerminalSGRExtensions
{
    private static volatile bool isDebugEnabled;

    internal static bool IsDebugEnabled
    {
        get => isDebugEnabled;
        set => isDebugEnabled = value;
    }
    
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

    private static string ToAnsiInternal(this TerminalSGR sgr, params byte[] values)
    {
        var suffix = (byte)sgr + (values.Length == 0 ? string.Empty : ";" + string.Join(";", values)) + "m";
        var ansi = "\u001b[" + suffix;
        if (IsDebugEnabled) ansi += "\\u001b[" + suffix;
        return ansi;
    }
    
    public static string ToAnsi(this TerminalSGR[] sgrs) => sgrs.OrEmpty().Select(ToAnsi).ToStringDelimited("");
}
