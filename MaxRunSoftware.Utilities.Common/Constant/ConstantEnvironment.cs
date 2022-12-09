// Copyright (c) 2022 Max Run Software (dev@maxrunsoftware.com)
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

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    #region OS

    /// <summary>
    /// Operating System are we currently running
    /// </summary>
    public static readonly OSPlatform OS = OS_Create();

    private static OSPlatform OS_Create()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return OSPlatform.Windows;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return OSPlatform.Linux;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return OSPlatform.OSX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)) return OSPlatform.FreeBSD;
        }
        catch (Exception e) { LogError(e); }

        // Unknown OS
        return OSPlatform.Windows;
    }

    /// <summary>
    /// Are we running on a Windows platform?
    /// </summary>
    public static readonly bool OS_Windows = OS == OSPlatform.Windows;

    /// <summary>
    /// Are we running on a UNIX/LINUX platform?
    /// </summary>
    public static readonly bool OS_Unix = OS == OSPlatform.Linux || OS == OSPlatform.FreeBSD;

    /// <summary>
    /// Are we running on a Mac/Apple platform?
    /// </summary>
    public static readonly bool OS_Mac = OS == OSPlatform.OSX;

    /// <summary>
    /// Are we running on a 32-bit operating system?
    /// </summary>
    public static readonly bool OS_x32 = !Environment.Is64BitOperatingSystem;

    /// <summary>
    /// Are we running on a 64-bit operating system?
    /// </summary>
    public static readonly bool OS_x64 = Environment.Is64BitOperatingSystem;

    #endregion OS

    #region NewLine

    public static readonly string NewLine = Environment.NewLine;
    public const string NewLine_Windows = "\r\n";
    public const string NewLine_Unix = "\n";
    public const string NewLine_Mac = "\r";

    #endregion NewLine

    #region Encoding

    /// <summary>
    /// UTF8 encoding WITHOUT the Byte Order Marker
    /// </summary>
    public static readonly Encoding Encoding_UTF8 = new UTF8Encoding(false); // Thread safe according to https://stackoverflow.com/a/3024405

    /// <summary>
    /// UTF8 encoding WITH the Byte Order Marker
    /// </summary>
    public static readonly Encoding Encoding_UTF8_BOM = new UTF8Encoding(true); // Thread safe according to https://stackoverflow.com/a/3024405

    public static readonly ImmutableDictionary<string, Encoding> Encodings = Encodings_Create();
    private static ImmutableDictionary<string, Encoding> Encodings_Create()
    {
        var d = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase);
        foreach (var encoding in Encoding.GetEncodings())
        {
            var enc = encoding.GetEncoding();
            d[encoding.Name] = enc;
            d[encoding.DisplayName] = enc;
        }
        return d.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    #endregion Encoding

    #region Path

    public static readonly ImmutableHashSet<char> PathDelimiters = PathDelimiters_Create();

    private static ImmutableHashSet<char> PathDelimiters_Create()
    {
        var hs = new HashSet<char>(new[] { '/', '\\', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
        var b = ImmutableHashSet.CreateBuilder<char>();
        foreach (var c in hs) b.Add(c);
        return b.ToImmutable();
    }

    public static readonly ImmutableHashSet<string> PathDelimiters_String = ImmutableHashSet.Create(PathDelimiters.Select(o => o.ToString()).ToArray());

    public static readonly bool Path_IsCaseSensitive = !OS_Windows;
    public static readonly StringComparer Path_StringComparer = Path_IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
    public static readonly StringComparison Path_StringComparison = Path_IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    #endregion Path
}
