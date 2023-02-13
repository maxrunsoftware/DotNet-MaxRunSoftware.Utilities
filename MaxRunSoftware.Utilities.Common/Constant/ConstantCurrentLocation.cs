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

using System.Diagnostics;

namespace MaxRunSoftware.Utilities.Common;

// ReSharper disable InconsistentNaming
public static partial class Constant
{
    #region Path_Current_Locations

    public static readonly ImmutableArray<string> Path_Current_Locations = ImmutableArray.Create(Path_Current_Locations_Create().ToArray());

    private static List<string> Path_Current_Locations_Create()
    {
        // https://stackoverflow.com/questions/616584/how-do-i-get-the-name-of-the-current-executable-in-c
        var queries = new List<Func<string?>>
        {
            () => Environment.CurrentDirectory,
            () => Process.GetCurrentProcess().MainModule?.FileName,
            () => AppDomain.CurrentDomain.FriendlyName,
            () => Process.GetCurrentProcess().ProcessName,
            () => typeof(Constant).Assembly.Location,
            () => Path.GetFullPath("."),
        };

        var extensions = new[] { "exe", "dll" }.SelectMany(PermuteCase).ToHashSet(Path_StringComparer);

        var set = new HashSet<string>(Path_StringComparer);
        var list = new List<string>();
        foreach (var query in queries)
        {
            string path = null!;
            try
            {
                var pathNullable = query();
                pathNullable = TrimOrNull(pathNullable);
                if (pathNullable == null) continue;
                path = pathNullable;
            }
            catch { }

            try
            {
                path = Path.GetFullPath(path);
            }
            catch { }

            try
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    foreach (var extension in extensions)
                    {
                        try
                        {
                            if (!File.Exists(path + "." + extension)) continue;
                            path += "." + extension;
                            break;
                        }
                        catch { }
                    }
                }
            }
            catch { }

            if (!set.Add(path)) continue;

            list.Add(path);
        }

        return list;
    }

    #endregion Path_Current_Locations

    #region Path_Current_Directories

    public static readonly ImmutableArray<string> Path_Current_Directories = ImmutableArray.Create(Path_Current_Directories_Create().ToArray());

    private static List<string> Path_Current_Directories_Create()
    {
        var list = new List<string>();
        var set = new HashSet<string>(Path_StringComparer);

        foreach (var location in Path_Current_Locations)
        {
            try
            {
                if (Directory.Exists(location))
                {
                    if (set.Add(location)) list.Add(location);
                }
                else if (File.Exists(location))
                {
                    var location2 = Path.GetDirectoryName(location);
                    if (location2 == null) continue;

                    location2 = Path.GetFullPath(location2);
                    if (!Directory.Exists(location2)) continue;

                    if (set.Add(location2)) list.Add(location2);
                }
            }
            catch { }
        }

        return list;
    }

    public static readonly string? Path_Current_Directory = Path_Current_Directories.FirstOrDefault();

    #endregion Path_Current_Directories

    #region Path_Current_Files

    public static readonly ImmutableArray<string> Path_Current_Files = ImmutableArray.Create(Path_Current_Files_Create().ToArray());

    private static List<string> Path_Current_Files_Create()
    {
        var list = new List<string>();
        var set = new HashSet<string>(Path_IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);

        foreach (var location in Path_Current_Locations)
        {
            try
            {
                if (!File.Exists(location)) continue;
                if (set.Add(location)) list.Add(location);
            }
            catch { }
        }

        return list;
    }

    /// <summary>
    /// The current EXE file name. Could be a full file path, or a partial file path, or null
    /// </summary>
    public static readonly string? Path_Current_File = Path_Current_Files.FirstOrDefault();

    #endregion Path_Current_Files

    #region IsScriptExecuted

    /// <summary>
    /// Are we executing via a batch file or script or running the command directly from the console window?
    /// </summary>
    public static readonly bool IsScriptExecuted = IsScriptExecuted_Create();

    private static bool IsScriptExecuted_Create()
    {
        try
        {
            // http://stackoverflow.com/questions/3453220/how-to-detect-if-console-in-stdin-has-been-redirected?lq=1
            //return (0 == (System.Console.WindowHeight + System.Console.WindowWidth)) && System.Console.KeyAvailable;
            if (Console.WindowHeight != 0) return false;

            if (Console.WindowWidth != 0) return false;

            if (!Console.KeyAvailable) return false;

            return true;
        }
        catch (Exception e) { LogError(e); }

        return false;
    }

    #endregion IsScriptExecuted
}
