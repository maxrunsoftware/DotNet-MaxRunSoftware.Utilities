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

using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMethodReturnValue.Local

namespace MaxRunSoftware.Utilities.Common;

/// <summary>
/// Provides functions for changing output colors
/// https://github.com/riezebosch/crayon
/// https://stackoverflow.com/questions/4842424/list-of-ansi-color-escape-sequences
/// https://en.wikipedia.org/wiki/ANSI_escape_code#Platform_support
/// https://github.com/termstandard/colors
/// https://www.lihaoyi.com/post/BuildyourownCommandLinewithANSIescapecodes.html
/// https://stackoverflow.com/a/48720492
/// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
/// https://www.ditig.com/256-colors-cheat-sheet
/// https://rich.readthedocs.io/en/stable/appendix/colors.html
/// </summary>
/// <code>
/// using System;
/// using static Crayon.Output;
///
/// namespace Crayon.ConsoleApp
/// {
///     static class Program
///     {
///         private static void Main()
///         {
///             Console.WriteLine(Green($"green {Bold("bold")} {Red("red")} green"));
///             Console.WriteLine("normal");
///             Console.WriteLine(Green().Reversed().Text("green"));
///             Console.WriteLine(Green().Reversed("green"));
///             Console.WriteLine($"{Bright.Green().Text("Bright")} and {Green("normal")} green");
///
///             Console.WriteLine(Green($"The difference {Bold("between bold")}, {Bright.Green("bright green")} and {Dim("dim")}"));
///
///             Console.WriteLine(Green().Bold().Underline().Reversed().Text("hoi!"));
///
///             Console.WriteLine(
///                 Bold().Green().Text($"starting green {Red("then red")} must be green again"));
///
///             Console.WriteLine(Rgb(55, 115, 155).Text("from rgb!"));
///             Console.WriteLine(Black().Background.Rgb(55, 115, 155).Text("from rgb!"));
///             Console.WriteLine(Rgb(55, 115, 155).Background.Green().Text("from rgb!"));
///
///             Console.WriteLine(Red().Reversed().Green("green"));
///
///             var rainbow = new Rainbow(0.5);
///             for (var i = 0; i &lt; 15; i++)
///             {
///                 Console.WriteLine(rainbow.Next().Bold().Text("rainbow"));
///             }
///         }
///     }
/// }
/// </code>
[PublicAPI]
public static class ConsoleColorCrayon
{
    // MIT License
    //
    // Copyright (c) 2019 Manuel Riezebosch
    //
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    //
    // The above copyright notice and this permission notice shall be included in all
    // copies or substantial portions of the Software.
    //
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    // SOFTWARE.

    /*

    */

    public enum Color8
    {
        Black = 30,
        Red = 31,
        Green = 32,
        Yellow = 33,
        Blue = 34,
        Magenta = 35,
        Cyan = 36,
        White = 37,
    }

    public enum Decoration
    {
        Bold = 1,
        Dim = 2,
        Underline = 4,
        Reversed = 7,
    }

    private const string Reset = "\u001b[0m";

    private static string Escape(params int[] codes) => $"\u001b[{string.Join(';', codes)}m";

    public interface IAppendable
    {
        IOutput Append(string format);
    }

    public interface IColors
    {
        IOutput Colorize(Color8 color);
        IOutput Colorize(byte r, byte g, byte b);
    }

    public interface IDecorations
    {
        IOutput Decorate(Decoration decoration);
    }

    // public interface IBright : IAppendable, IColors { }
    // public interface IBackground : IAppendable, IColors { }

    public interface IOutput : IAppendable, IColors, IDecorations
    {
        // IBright Bright { get; }
        // IBackground Background { get; }
        string Text(string text);
    }

    private abstract class ColorizeBase : IAppendable, IColors
    {
        public virtual IOutput Colorize(Color8 color) => Append((int)color);
        public virtual IOutput Colorize(byte r, byte g, byte b) => throw new NotImplementedException();
        public virtual IOutput Append(byte code1, byte code2, byte r, byte g, byte b) => Append(Escape(code1, code2, r, g, b));
        public virtual IOutput Append(int code) => Append(Escape(code));
        public abstract IOutput Append(string format);
    }

    private abstract class ColorizeChainedBase : ColorizeBase
    {
        private readonly IOutput chain;
        protected ColorizeChainedBase(IOutput chain) => this.chain = chain;
        public override IOutput Append(string format) => chain.Append(format);
    }

    // private class BackgroundClass : ColorizeChainedBase, IBackground
    // {
    //     public BackgroundClass(IOutput chain) : base(chain) { }
    //     public override IOutput Colorize(byte r, byte g, byte b) => Append(48, 2, r, g, b); // $"\u001b[48;2;{r};{g};{b}m"
    //     public override IOutput Append(int code) => base.Append(code + 10); // $"\u001b[{code + 10}m"
    // }

    // private class BrightClass : ColorizeChainedBase, IBright
    // {
    //     public BrightClass(IOutput chain) : base(chain) { }
    //     public override IOutput Append(int code) => Append(Escape(code, 1)); // $"\u001b[{code};1m"
    // }

    private abstract class OutputBase : ColorizeBase, IOutput
    {
        public override IOutput Colorize(byte r, byte g, byte b) => Append(38, 2, r, g, b); // $"\u001b[38;2;{r};{g};{b}m"
        public virtual IOutput Decorate(Decoration decoration) => Append((int)decoration);
        // public virtual IBright Bright => new BrightClass(this);
        // public virtual IBackground Background => new BackgroundClass(this);
        public abstract string Text(string text);
    }

    private class OutputBuilder : OutputBase
    {
        private readonly StringBuilder builder = new();

        public override string Text(string text)
        {
            var format = builder.ToString();
            var textReformatted = text.Replace(Reset, $"{Reset}{format}");
            return builder.Append(textReformatted).Append(Reset).ToString();
        }

        public override IOutput Append(string format)
        {
            builder.Append(format);
            return this;
        }
    }

    private class OutputBuilderIgnoreFormat : OutputBase
    {
        public override string Text(string text) => text;
        public override IOutput Append(string format) => this;
    }

    public interface IRainbow
    {
        public IOutput Next();
    }

    private class RainbowClass : IRainbow
    {
        private readonly double frequency;
        private int index;

        public RainbowClass(double frequency) => this.frequency = frequency;

        public IOutput Next()
        {
            var i = index++;

            // ReSharper disable once InlineTemporaryVariable
            var f = frequency;

            var r = Convert.ToByte(Math.Round(Math.Sin(f * i) * 127 + 128));
            var g = Convert.ToByte(Math.Round(Math.Sin(f * i + 2) * 127 + 128));
            var b = Convert.ToByte(Math.Round(Math.Sin(f * i + 4) * 127 + 128));

            return Colorize(r, g, b);
        }
    }

    #region Methods

    /// <summary>
    /// https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    /// </summary>
    private static class ColorsOnWindows
    {
        public static bool Enable()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return true;

            var iStdOut = GetStdHandle(StdOutputHandle);
            return GetConsoleMode(iStdOut, out var outConsoleMode) &&
                   SetConsoleMode(iStdOut, outConsoleMode | EnableVirtualTerminalProcessing);
        }

        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 0x0004;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
    }

    private static Func<IOutput> funcOutput = () => new OutputBuilderIgnoreFormat();

    static ConsoleColorCrayon()
    {
        if (Environment.GetEnvironmentVariable("MAXRUNSOFTWARE_NOCOLOR") == null) Enable();
    }

    public static void Enable()
    {
        ColorsOnWindows.Enable();
        funcOutput = () => new OutputBuilder();
    }

    public static void Disable() => funcOutput = () => new OutputBuilderIgnoreFormat();

    public static IOutput Colorize(Color8 color) => funcOutput().Colorize(color);
    public static string Colorize(Color8 color, string text) => funcOutput().Colorize(color, text);
    public static string Colorize(this IColors obj, Color8 color, string text) => obj.Colorize(color).Text(text);


    public static IOutput Black() => Colorize(Color8.Black);
    public static string Black(string text) => Colorize(Color8.Black, text);
    public static IOutput Black(this IColors obj) => obj.Colorize(Color8.Black);
    public static string Black(this IColors obj, string text) => obj.Colorize(Color8.Black, text);

    public static IOutput Red() => Colorize(Color8.Red);
    public static string Red(string text) => Colorize(Color8.Red, text);
    public static IOutput Red(this IColors obj) => obj.Colorize(Color8.Red);
    public static string Red(this IColors obj, string text) => obj.Colorize(Color8.Red, text);

    public static IOutput Green() => Colorize(Color8.Green);
    public static string Green(string text) => Colorize(Color8.Green, text);
    public static IOutput Green(this IColors obj) => obj.Colorize(Color8.Green);
    public static string Green(this IColors obj, string text) => obj.Colorize(Color8.Green, text);

    public static IOutput Yellow() => Colorize(Color8.Yellow);
    public static string Yellow(string text) => Colorize(Color8.Yellow, text);
    public static IOutput Yellow(this IColors obj) => obj.Colorize(Color8.Yellow);
    public static string Yellow(this IColors obj, string text) => obj.Colorize(Color8.Yellow, text);

    public static IOutput Blue() => Colorize(Color8.Blue);
    public static string Blue(string text) => Colorize(Color8.Blue, text);
    public static IOutput Blue(this IColors obj) => obj.Colorize(Color8.Blue);
    public static string Blue(this IColors obj, string text) => obj.Colorize(Color8.Blue, text);

    public static IOutput Magenta() => Colorize(Color8.Magenta);
    public static string Magenta(string text) => Colorize(Color8.Magenta, text);
    public static IOutput Magenta(this IColors obj) => obj.Colorize(Color8.Magenta);
    public static string Magenta(this IColors obj, string text) => obj.Colorize(Color8.Magenta, text);

    public static IOutput Cyan() => Colorize(Color8.Cyan);
    public static string Cyan(string text) => Colorize(Color8.Cyan, text);
    public static IOutput Cyan(this IColors obj) => obj.Colorize(Color8.Cyan);
    public static string Cyan(this IColors obj, string text) => obj.Colorize(Color8.Cyan, text);

    public static IOutput White() => Colorize(Color8.White);
    public static string White(string text) => Colorize(Color8.White, text);
    public static IOutput White(this IColors obj) => obj.Colorize(Color8.White);
    public static string White(this IColors obj, string text) => obj.Colorize(Color8.White, text);


    public static IOutput Colorize(byte r, byte g, byte b) => funcOutput().Colorize(r, g, b);
    public static string Colorize(byte r, byte g, byte b, string text) => funcOutput().Colorize(r, g, b, text);
    public static string Colorize(this IColors obj, byte r, byte g, byte b, string text) => obj.Colorize(r, g, b).Text(text);


    public static IOutput Decorate(Decoration decoration) => funcOutput().Decorate(decoration);
    public static string Decorate(Decoration decoration, string text) => funcOutput().Decorate(decoration, text);
    public static string Decorate(this IDecorations obj, Decoration decoration, string text) => obj.Decorate(decoration).Text(text);

    public static IOutput Bold() => Decorate(Decoration.Bold);
    public static string Bold(string text) => Decorate(Decoration.Bold, text);
    public static IOutput Bold(this IDecorations obj) => obj.Decorate(Decoration.Bold);
    public static string Bold(this IDecorations obj, string text) => obj.Decorate(Decoration.Bold, text);

    public static IOutput Dim() => Decorate(Decoration.Dim);
    public static string Dim(string text) => Decorate(Decoration.Dim, text);
    public static IOutput Dim(this IDecorations obj) => obj.Decorate(Decoration.Dim);
    public static string Dim(this IDecorations obj, string text) => obj.Decorate(Decoration.Dim, text);

    public static IOutput Underline() => Decorate(Decoration.Underline);
    public static string Underline(string text) => Decorate(Decoration.Underline, text);
    public static IOutput Underline(this IDecorations obj) => obj.Decorate(Decoration.Underline);
    public static string Underline(this IDecorations obj, string text) => obj.Decorate(Decoration.Underline, text);

    public static IOutput Reversed() => Decorate(Decoration.Reversed);
    public static string Reversed(string text) => Decorate(Decoration.Reversed, text);
    public static IOutput Reversed(this IDecorations obj) => obj.Decorate(Decoration.Reversed);
    public static string Reversed(this IDecorations obj, string text) => obj.Decorate(Decoration.Reversed, text);


    // public static IBackground Background => new BackgroundClass(funcOutput());
    // public static IBright Bright => new BrightClass(funcOutput());


    public static IRainbow Rainbow(double frequency) => new RainbowClass(frequency);


    #endregion Methods
}
