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

public static class ExtensionsColor
{
    public static List<Color> Shift(this Color startColor, Color endColor, int steps)
    {
        steps.CheckMin(1);
        
        // https://stackoverflow.com/a/2011839
        var colorList = new List<Color>();
        for(var i=0; i<steps; i++)
        {
            var rAverage = startColor.R + (int)((endColor.R - startColor.R) * i / steps);
            var gAverage = startColor.G + (int)((endColor.G - startColor.G) * i / steps);
            var bAverage = startColor.B + (int)((endColor.B - startColor.B) * i / steps);
            
            colorList.Add(Color.FromArgb(ToByte(rAverage), ToByte(gAverage), ToByte(bAverage)));
        }
        return colorList;
        
        static byte ToByte(int value) => (byte)Math.Max(Math.Min(value, 255), 0);
    }

    public static Color Shift(this Color startColor, Color endColor, double percentShifted)
    {
        percentShifted.CheckMin(ConstantNumber<double>.Zero).CheckMax(ConstantNumber<double>.One);

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
