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

public static class ExtensionsColor
{
    public static string ToCss(this Color color)
    {
        var a = (color.A * (100d / 255d) / 100d).ToString(MidpointRounding.AwayFromZero, 1);
        return $"rgba({color.R}, {color.G}, {color.B}, {a})";
    }

    public static string ToHex(this Color color)
    {
        var sb = new StringBuilder(9);
        sb.Append('#');
        sb.Append(color.R.ToString("X2"));
        sb.Append(color.G.ToString("X2"));
        sb.Append(color.B.ToString("X2"));
        if (color.A < byte.MaxValue) sb.Append(color.A.ToString("X2"));
        return sb.ToString().ToUpperInvariant();
    }

    public static Color Shift(this Color startColor, Color endColor, double percentShifted)
    {
        if (percentShifted > 1d) percentShifted = 1d;

        if (percentShifted < 0d) percentShifted = 0d;

        //int rStart = startColor.R;
        //int rEnd = endColor.R;
        var r = ShiftPercent(startColor.R, endColor.R, percentShifted);
        var g = ShiftPercent(startColor.G, endColor.G, percentShifted);
        var b = ShiftPercent(startColor.B, endColor.B, percentShifted);
        var a = ShiftPercent(startColor.A, endColor.A, percentShifted);

        return Color.FromArgb(a, r, g, b);
    }

    private static byte ShiftPercent(byte start, byte end, double percent)
    {
        if (start == end) return start;

        var d = (double)start - end;
        d = d * percent;
        var i = int.Parse(d.ToString(MidpointRounding.AwayFromZero, 0));
        i = start - i;
        if (start < end && i > end) i = end;

        if (start > end && i < end) i = end;

        if (i > byte.MaxValue) i = byte.MaxValue;

        if (i < byte.MinValue) i = byte.MinValue;

        var by = (byte)i;
        return by;
    }

}
