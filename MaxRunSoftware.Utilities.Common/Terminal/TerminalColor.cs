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
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace MaxRunSoftware.Utilities.Common;

public class TerminalColor
{
    static TerminalColor()
    {
        if (Environment.GetEnvironmentVariable("MAXRUNSOFTWARE_NOCOLOR") == null) EnableOnWindows();
    }

    protected TerminalColorTextParser? Text { get; }
    public Color Color { get; }
    public string ColorName { get; }

    protected TerminalColor(Color color, string? colorName)
    {
        Color = color;
        ColorName = colorName ?? color.Name;
    }

    protected TerminalColor(string colorString)
    {
        Text = TerminalColorTextParser.Parse(colorString);
        Color = Text.Color24;
        ColorName = Text.ColorName;
    }

    #region Static

    public static TerminalColor GetTerminalColor(byte red, byte green, byte blue) => GetTerminalColor(255, red, green, blue);

    public static TerminalColor GetTerminalColor(byte alpha, byte red, byte green, byte blue)
    {
        var color = Color.FromArgb(alpha, red, green, blue);
        return TerminalColor8.GetColor8(color) ?? new TerminalColor(color, null);
    }

    #region EnableOnWindows

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

    #endregion EnableOnWindows

    #endregion Static
}
