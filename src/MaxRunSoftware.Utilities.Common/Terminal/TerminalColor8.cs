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

public class TerminalColor8 : TerminalColor
{
    static TerminalColor8()
    {
        colors8 = Lzy.Create(GetColors<TerminalColor8>);
        colors8RGB = Lzy.Create(() =>
        {
            var d = new Dictionary<uint, TerminalColor8>();
            foreach (var c in Colors8)
            {
                var key = ToUInt(c);
                d.TryAdd(key, c);
            }
            return d.ToImmutableDictionary();
        });
    }

    public byte Color8 { get; }

    public TerminalColor8(Color color, string? colorName, byte color8, string? colorHex = null) : base(color, colorName, colorHex)
    {
        Color8 = color8;
    }

    public TerminalColor8(byte color8, byte red, byte green, byte blue, string? colorHex, string colorName) : this(Color.FromArgb(red, green, blue), colorName, color8, colorHex) { }

    public override string ToStringAnsiForeground() => TerminalSGR.Color_Foreground.ToAnsi(Color8);

    public override string ToStringAnsiBackground() => TerminalSGR.Color_Background.ToAnsi(Color8);


    public static ImmutableArray<TerminalColor8> Colors8 => colors8.Value;
    // ReSharper disable once InconsistentNaming
    private static readonly Lzy<ImmutableArray<TerminalColor8>> colors8;

    // ReSharper disable once InconsistentNaming
    private static readonly Lzy<ImmutableDictionary<uint, TerminalColor8>> colors8RGB;
    internal static TerminalColor8? GetColor8(Color color) =>
        colors8RGB.Value.TryGetValue(ToUInt(color), out var terminalColor)
            ? terminalColor
            : null;


    #region Helpers

    private static uint ToUInt(TerminalColor c) => ToUInt(c.Color.A, c.Color.R, c.Color.G, c.Color.B);
    private static uint ToUInt(Color c) => ToUInt(c.A, c.R, c.G, c.B);
    // ReSharper disable once UnusedMember.Local
    private static uint ToUInt(byte red, byte green, byte blue) => ToUInt(255, red, green, blue);
    private static uint ToUInt(byte alpha, byte red, byte green, byte blue) => BitConverter.ToUInt32(new[] { alpha, red, green, blue });

    protected static ImmutableArray<T> GetColors<T>() where T : TerminalColor8 =>
        typeof(TerminalColors)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(o => o.CanRead && !o.CanWrite)
            .Where(o => o.PropertyType.IsAssignableTo(typeof(T)))
            .Select(o => o.GetValue(null) as T)
            .WhereNotNull()
            .OrderBy(o => o.Color8)
            .ToImmutableArray();


    #endregion Helpers
}
